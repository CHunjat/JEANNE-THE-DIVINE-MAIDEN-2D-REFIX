using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour
{
    [Header("체크포인트 고유 정보")]
    public Transform spawnPoint; // 텔레포트 시 도착할 내 위치

    [Header("UI 연결")]
    public GameObject interactPromptUI;
    public GameObject menuUI;

    [Header("텔레포트 서브메뉴 세팅")]
    public GameObject teleportMenuUI; // 목적지 고르는 UI 패널 
    public GameObject[] teleportCursors; // 목적지 UI들의 하이라이트들
    public Checkpoint[] destinations; // 갈 수 있는 목적지 체크포인트들을
    private int currentTeleportIndex = 0;
    private bool isInTeleportMenu = false; // 현재 텔레포트 창이 열려있는가?

    [Header("암전 연출 (SpriteRenderer 연결 필수)")]
    public SpriteRenderer dimmerSprite;

    [Header("메뉴 네비게이션 UI")]
    public GameObject[] menuCursors;
    private int currentMenuIndex = 0;

    [Header("연출 설정")]
    public float fadeDuration = 0.5f;
    public float interactCooldown = 0.6f;
    private float nextInteractTime = 0f;

    private bool isPlayerInRange = false;
    private bool isUnlocked;
    private PlayerController playerObj;
    private bool isRestingProcess = false;

    private void Start()
    {
        if (interactPromptUI != null) interactPromptUI.SetActive(false);
        if (menuUI != null) menuUI.SetActive(false);
        if (teleportMenuUI != null) teleportMenuUI.SetActive(false);

        // Dimmer 초기화
        if (dimmerSprite != null)
        {
            Color c = dimmerSprite.color;
            c.a = 0f;
            dimmerSprite.color = c;
            dimmerSprite.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            isPlayerInRange = true;
            playerObj = col.GetComponent<PlayerController>();
            if (!menuUI.activeSelf && !isInTeleportMenu) interactPromptUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            isPlayerInRange = false;
            playerObj = null;
            interactPromptUI.SetActive(false);
            menuUI.SetActive(false);
            if (teleportMenuUI != null) teleportMenuUI.SetActive(false);
            isInTeleportMenu = false;
        }
    }

    private void Update()
    {
        if (isRestingProcess) return;

        // 0순위: 텔레포트 목적지 메뉴가 열려있을 때의 키보드 조작
        if (isInTeleportMenu && teleportMenuUI != null)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                currentTeleportIndex = (currentTeleportIndex == 0) ? teleportCursors.Length - 1 : currentTeleportIndex - 1;
                UpdateTeleportCursorUI();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                currentTeleportIndex = (currentTeleportIndex == teleportCursors.Length - 1) ? 0 : currentTeleportIndex + 1;
                UpdateTeleportCursorUI();
            }

            // C키를 누르면 선택한 목적지로 텔포
            if (Input.GetKeyDown(KeyCode.C))
            {
                if (destinations.Length > 0 && destinations[currentTeleportIndex] != null)
                {
                    Checkpoint dest = destinations[currentTeleportIndex];

                    // 🔥 목적지가 자기 자신(현재 모닥불)일 때!
                    if (dest == this)
                    {
                        Debug.Log("<color=yellow>현재 위치로는 텔레포트할 수 없습니다!</color>");
                        isInTeleportMenu = false;
                        teleportMenuUI.SetActive(false);
                        menuUI.SetActive(true);
                    }
                    else // 다른 곳일 때만 정상 텔레포트!
                    {
                        StartCoroutine(TeleportRoutine(dest));
                    }
                }
                else
                {
                    Debug.LogWarning("해당 슬롯에 연결된 목적지 체크포인트가 없습니다!");
                }
            }

            // ESC를 누르면 텔레포트 창 끄고 원래 모닥불 메뉴로 뒤로가기
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                isInTeleportMenu = false;
                teleportMenuUI.SetActive(false);
                menuUI.SetActive(true);
            }
            return; // 텔레포트 창이 켜져있을 땐 아래 메인메뉴 로직을 무시
        }

        // 1순위: 메인 메뉴가 열려있을 때
        if (menuUI.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                currentMenuIndex = (currentMenuIndex == 0) ? menuCursors.Length - 1 : currentMenuIndex - 1;
                UpdateCursorUI();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                currentMenuIndex = (currentMenuIndex == menuCursors.Length - 1) ? 0 : currentMenuIndex + 1;
                UpdateCursorUI();
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                if (currentMenuIndex == 0) ExecuteRest();
                else if (currentMenuIndex == 1) ExecuteTeleport();
                else if (currentMenuIndex == 2) ExecuteSkillMenu();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                nextInteractTime = Time.time + interactCooldown;
                menuUI.SetActive(false);
                interactPromptUI.SetActive(true);
                playerObj.StateMachine.ChangeState(playerObj.StandUpState);
                StartCoroutine(FadeAlpha(0f));
            }
        }
        // 2순위: 메뉴가 닫혀있고, 플레이어가 범위 내에 있을 때
        else if (isPlayerInRange && playerObj != null)
        {
            bool isBaseState = (playerObj.StateMachine.CurrentState == playerObj.IdleState ||
                                playerObj.StateMachine.CurrentState == playerObj.MoveState);
            bool isSprinting = playerObj.isSprinting;
            bool isSafeState = isBaseState && !isSprinting;
            bool canInteract = isSafeState && !menuUI.activeSelf;

            if (canInteract)
            {
                if (!interactPromptUI.activeSelf) interactPromptUI.SetActive(true);

                if (Input.GetKeyDown(KeyCode.C) && Time.time >= nextInteractTime)
                {
                    playerObj.StateMachine.ChangeState(playerObj.RestState);
                    StartCoroutine(OpenMenuRoutine());
                }
            }
            else
            {
                if (interactPromptUI.activeSelf) interactPromptUI.SetActive(false);
            }
        }
    }

    private void ExecuteSkillMenu()
    {
        return;
    }

    private IEnumerator OpenMenuRoutine()
    {
        interactPromptUI.SetActive(false);

        if (dimmerSprite != null)
        {
            dimmerSprite.sortingOrder = -5;
            dimmerSprite.gameObject.SetActive(true);
        }

        yield return StartCoroutine(FadeAlpha(0.6f));

        menuUI.SetActive(true);
        currentMenuIndex = 0;
        UpdateCursorUI();
    }

    private void UpdateCursorUI()
    {
        for (int i = 0; i < menuCursors.Length; i++)
            if (menuCursors[i] != null) menuCursors[i].SetActive(i == currentMenuIndex);
    }

    private void ExecuteRest()
    {
        menuUI.SetActive(false);
        StartCoroutine(RestEffectRoutine());
    }

    //텔레포트 서브메뉴 진입
    private void ExecuteTeleport()
    {
        if (teleportMenuUI != null && teleportCursors.Length > 0)
        {
            menuUI.SetActive(false);
            isInTeleportMenu = true;
            currentTeleportIndex = 0;
            UpdateTeleportCursorUI();
            teleportMenuUI.SetActive(true);
        }
        else
        {
            Debug.LogWarning("텔레포트 메뉴 UI 또는 커서가 세팅되지 않았습니다.");
        }
    }

    private void UpdateTeleportCursorUI()
    {
        for (int i = 0; i < teleportCursors.Length; i++)
            if (teleportCursors[i] != null) teleportCursors[i].SetActive(i == currentTeleportIndex);
    }

    //암전 -> 이동 -> 바통터치 로직
    private IEnumerator TeleportRoutine(Checkpoint destination)
    {
        isRestingProcess = true;
        isInTeleportMenu = false;
        if (teleportMenuUI != null) teleportMenuUI.SetActive(false);

        PlayerController p = playerObj;

        // 1. 출발지에서 완전 암전 (화면 다 덮기)
        if (dimmerSprite != null) dimmerSprite.sortingOrder = 5;
        yield return StartCoroutine(FadeAlpha(1f));

        // 2. 이동 직전에 '목적지'의 암전막도 미리 100% 까맣게 활성화
        if (destination.dimmerSprite != null)
        {
            destination.dimmerSprite.sortingOrder = 5;
            destination.dimmerSprite.gameObject.SetActive(true);
            Color destColor = destination.dimmerSprite.color;
            destColor.a = 1f;
            destination.dimmerSprite.color = destColor;
        }

        // 3. 물리적 좌표 이동
        p.rb.linearVelocity = Vector2.zero;
        p.transform.position = destination.spawnPoint.position;

        // 4. 출발지 암전 끄기
        if (dimmerSprite != null)
        {
            Color c = dimmerSprite.color;
            c.a = 0f;
            dimmerSprite.color = c;
            dimmerSprite.gameObject.SetActive(false);
        }

        // 5. 목적지에서 코루틴 바통 터치!
        destination.StartCoroutine(destination.FadeInAfterTeleport(p));

        isRestingProcess = false;
    }

    // 텔레포트 도착 직후, 화면을 유지했다가 밝혀주는 코루틴
    public IEnumerator FadeInAfterTeleport(PlayerController arrivedPlayer)
    {
        isRestingProcess = true;
        playerObj = arrivedPlayer;
        isPlayerInRange = true;
        isUnlocked = true;

        // 1. 이동 완료 후 암전(까만 화면) 상태 자연스럽게 대기 유지
        yield return new WaitForSeconds(0.5f);

        // 2. 화면 서서히 완전하게 밝아짐 (FadeAlpha(0f) 내부에서 SetActive(false) 처리됨)
        if (dimmerSprite != null)
        {
            yield return StartCoroutine(FadeAlpha(0f));

            // 3. 🔥 [버그 해결 핵심] 다 밝아진 직후, 메뉴창 배경용으로 세팅할 때 무조건 오브젝트 다시 켜기!
            dimmerSprite.sortingOrder = -5;
            Color c = dimmerSprite.color;
            c.a = 0.6f;
            dimmerSprite.color = c;
            dimmerSprite.gameObject.SetActive(true); // 👈 이게 빠져서 다음 휴식 때 먹통이 된 거였습니다.
        }

        // 4. 도착한 곳의 메인 메뉴 열어주기!
        menuUI.SetActive(true);
        currentMenuIndex = 0;
        UpdateCursorUI();

        isRestingProcess = false;
    }

    private IEnumerator RestEffectRoutine()
    {
        isRestingProcess = true;

        // 🔥 [안전장치 추가] 휴식 진입 시 암전막 확실하게 켜주기
        if (dimmerSprite != null)
        {
            dimmerSprite.sortingOrder = 5;
            dimmerSprite.gameObject.SetActive(true);
        }
        yield return StartCoroutine(FadeAlpha(1f));

        if (playerObj != null && playerObj.playerStats != null)
        {
            playerObj.playerStats.currentHp = playerObj.playerStats.baseMaxHp;
            playerObj.playerStats.currentMp = playerObj.playerStats.baseMaxMp;
        }

        yield return new WaitForSeconds(1f);

        if (dimmerSprite != null) dimmerSprite.sortingOrder = -5;
        yield return StartCoroutine(FadeAlpha(0f));

        playerObj.StateMachine.ChangeState(playerObj.StandUpState);
        isRestingProcess = false;
    }

    private IEnumerator FadeAlpha(float targetAlpha)
    {
        if (dimmerSprite == null) yield break;

        // 🔥 [안전장치 추가] 투명도가 0보다 크면 무조건 오브젝트 활성화하고 시작
        if (targetAlpha > 0f) dimmerSprite.gameObject.SetActive(true);

        Color c = dimmerSprite.color;
        float startAlpha = c.a;
        float time = 0;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            dimmerSprite.color = c;
            yield return null;
        }
        c.a = targetAlpha;
        dimmerSprite.color = c;

        if (targetAlpha <= 0f) dimmerSprite.gameObject.SetActive(false);
    }
}