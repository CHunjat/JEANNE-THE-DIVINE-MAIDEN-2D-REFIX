using UnityEngine;
using System.Collections;
using System;

public class Checkpoint : MonoBehaviour
{
    [Header("UI 연결")]
    public GameObject interactPromptUI;
    public GameObject menuUI;

    [Header("암전 연출 (SpriteRenderer 연결 필수)")]
    public SpriteRenderer dimmerSprite;

    [Header("메뉴 네비게이션 UI")]
    public GameObject[] menuCursors;
    private int currentMenuIndex = 0;

    [Header("연출 설정")]
    public float fadeDuration = 0.5f;
    public float interactCooldown = 0.6f; // 연타 방지용 쿨타임 (일어나는 애니메이션 길이에 맞춰 조절)
    private float nextInteractTime = 0f; // 다음 상호작용 가능 시간을 추적

    private bool isPlayerInRange = false;
    private PlayerController playerObj;
    private bool isRestingProcess = false;

    private void Start()
    {
        if (interactPromptUI != null) interactPromptUI.SetActive(false);
        if (menuUI != null) menuUI.SetActive(false);

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
            if (!menuUI.activeSelf) interactPromptUI.SetActive(true);
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
        }
    }

    private void Update()
    {
        if (isRestingProcess) return;

        // [핵심 변경] 1순위: 메뉴가 열려있을 때 조작을 가장 먼저 처리
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
                else if (currentMenuIndex == 2) ExecuteSkillMenu(); // 3번째 메뉴: 스킬창
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
        // 2순위: 메뉴가 닫혀있고, 플레이어가 범위 내에 있을 때 (상호작용 띄우기)
        else if (isPlayerInRange && playerObj != null)
        {
            // 현재 상태가 Idle이나 Move인지 확인
            bool isBaseState = (playerObj.StateMachine.CurrentState == playerObj.IdleState ||
                                playerObj.StateMachine.CurrentState == playerObj.MoveState);

            // 2. [스프린트 방어] MoveState이긴 한데, 지금 뛰고 있는가?
            bool isSprinting = playerObj.isSprinting;

            // 3. 최종 안전 상태: Idle/Move 상태이면서 동시에 뛰고 있지 않을 때!
            bool isSafeState = isBaseState && !isSprinting;

            // 상호작용 가능 여부
            bool canInteract = isSafeState && !menuUI.activeSelf;

            if (canInteract)
            {
                if (!interactPromptUI.activeSelf) interactPromptUI.SetActive(true);

                // C키 입력 시 메뉴 오픈
                if (Input.GetKeyDown(KeyCode.C) && Time.time >= nextInteractTime)
                {
                    playerObj.StateMachine.ChangeState(playerObj.RestState);
                    StartCoroutine(OpenMenuRoutine());
                }
            }
            else
            {
                // 스프린트 키를 누르는 즉시 UI가 꺼지고 C키가 안 먹힘
                if (interactPromptUI.activeSelf) interactPromptUI.SetActive(false);
            }
        }
    }

    private void ExecuteSkillMenu()
    {
        Debug.Log("<color=cyan>스킬창 기능은 아직 구현되지 않았습니다.</color>");

        // 아직 기능이 없으므로 메뉴를 닫지 않고 그냥 유지
        // 나중에 스킬 UI 띄우는 코드 추가하면 됨
        return;
    }

    private IEnumerator OpenMenuRoutine()
    {
        interactPromptUI.SetActive(false);

        // 메뉴 열기: 배경만 암전 (SortingOrder -5)
        if (dimmerSprite != null)
        {
            dimmerSprite.sortingOrder = -5;
            dimmerSprite.gameObject.SetActive(true);
        }

        yield return StartCoroutine(FadeAlpha(0.6f)); // 0.6 정도로 배경만 어둡게

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

    private void ExecuteTeleport()
    {
        Debug.Log("<color=yellow>텔레포트 기능은 아직 구현되지 않았습니다.</color>");

        // 기능을 만들기 전까지는 아래 코드를 실행하지 않고 여기서 바로 함수를 종료시킴!
        // (이렇게 하면 메뉴가 닫히지 않고 그대로 유지되어서 ESC로 안전하게 나갈 수 있음)
        return;
        //menuUI.SetActive(false); 기능 만들어지면 주석해제 ㅋㅋ아직 안만들었으니까..
    }

    private IEnumerator RestEffectRoutine()
    {
        isRestingProcess = true;

        // 완전 암전: 플레이어까지 가리기 위해 5로 변경
        if (dimmerSprite != null) dimmerSprite.sortingOrder = 5;
        yield return StartCoroutine(FadeAlpha(1f));

        if (playerObj != null && playerObj.playerStats != null)
        {
            playerObj.playerStats.currentHp = playerObj.playerStats.maxHp;
            playerObj.playerStats.currentMp = playerObj.playerStats.MaxMp;
        }

        yield return new WaitForSeconds(1f);

        // 다시 메뉴 상태(배경만 어두움)로 복귀
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