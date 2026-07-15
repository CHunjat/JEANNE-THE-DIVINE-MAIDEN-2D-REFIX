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

                    // 🔥 [핵심 예외 처리] 목적지가 자기 자신(현재 모닥불)일 때!
                    if (dest == this)
                    {
                        Debug.Log("<color=yellow>현재 위치로는 텔레포트할 수 없습니다!</color>");
                        // 아무 일도 안 일어나게 하거나, 
                        // 아래처럼 창을 닫고 메인 메뉴로 돌려보내는 게 제일 깔끔합니다.
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
        Debug.Log("<color=cyan>스킬창 기능은 아직 구현되지 않았습니다.</color>");
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

        // 텔레포트 도중 OnTriggerExit2D가 playerObj를 null로 만들어버리는 것을 방지하기 위해,
        // 현재 플레이어 정보를 로컬 변수(p)에 꽉 붙잡아(Cache) 둡니다!
        PlayerController p = playerObj;

        // 1. 완전 암전 (플레이어 가리기)
        if (dimmerSprite != null) dimmerSprite.sortingOrder = 5;
        yield return StartCoroutine(FadeAlpha(1f));

        // 2. 물리 튕김 방지 및 좌표 텔레포트 (이제 p 를 사용합니다)
        p.rb.linearVelocity = Vector2.zero;
        p.transform.position = destination.spawnPoint.position;

        // 3. 맵 로딩 체감을 위한 안정화 대기
        yield return new WaitForSeconds(0.5f);

        // 4. 원래 있던 체크포인트의 암전막(Dimmer) 치우기
        if (dimmerSprite != null)
        {
            Color c = dimmerSprite.color;
            c.a = 0f;
            dimmerSprite.color = c;
            dimmerSprite.gameObject.SetActive(false);
        }

        // 5. 목적지 체크포인트가 화면 밝히기 바통터치!
        destination.StartFadeInFromTeleport(p);

       

        isRestingProcess = false;
    }

    // 텔레포트 도착 직후, 도착지 체크포인트가 화면을 밝혀주는 함수
    public void StartFadeInFromTeleport(PlayerController arrivedPlayer)
    {
        // 1. 도착지 체크포인트가 플레이어를 인식하게 만듦
        playerObj = arrivedPlayer;
        isPlayerInRange = true;
        isUnlocked = true; // 텔레포트로 도착했으니 당연히 해금됨!

        // 2. 암전막 설정 (메뉴창을 띄워야 하니 -5로 뒤로 깔기)
        if (dimmerSprite != null)
        {
            dimmerSprite.sortingOrder = -5;
            dimmerSprite.gameObject.SetActive(true);

            // 까만 화면(1f)에서 시작
            Color c = dimmerSprite.color;
            c.a = 1f;
            dimmerSprite.color = c;

            // 0.6f(메뉴 배경 밝기)로 서서히 밝아지기!
            StartCoroutine(FadeAlpha(0.6f));
        }

        // 3. 도착한 곳의 메인 메뉴 열어주기!
        menuUI.SetActive(true);
        currentMenuIndex = 0;
        UpdateCursorUI();
    }

    private IEnumerator RestEffectRoutine()
    {
        isRestingProcess = true;

        if (dimmerSprite != null) dimmerSprite.sortingOrder = 5;
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