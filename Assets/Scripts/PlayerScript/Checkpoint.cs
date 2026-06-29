using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Checkpoint : MonoBehaviour
{
    [Header("UI 연결")]
    public GameObject interactPromptUI;
    public GameObject menuUI;
    public Image backgroundFadeImage;

    [Header("연출 설정")]
    public float fadeDuration = 0.5f; // 까매지는 속도

    private bool isPlayerInRange = false;
    private PlayerController playerObj;

    private void Start()
    {
        if (interactPromptUI != null) interactPromptUI.SetActive(false);
        if (menuUI != null) menuUI.SetActive(false);
        if (backgroundFadeImage != null) backgroundFadeImage.gameObject.SetActive(false);
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
        // 1. 다가가서 F를 누르면 (상호작용 시작)
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.F) && !menuUI.activeSelf
            && playerObj.StateMachine.CurrentState != playerObj.RestState)
        {
            interactPromptUI.SetActive(false);
            menuUI.SetActive(true);

            // 플레이어를 앉히고 (ToRest -> Resting)
            playerObj.StateMachine.ChangeState(playerObj.RestState);

            // 배경을 살짝 까맣게 (스포트라이트 연출)
            StartCoroutine(FadeBackground(0.85f));
        }

        // 2. 취소(ESC)를 눌렀을 때 (휴식 안 하고 그냥 나감)
        if (menuUI.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            menuUI.SetActive(false);
            interactPromptUI.SetActive(true);

            // 플레이어를 일으키고 (Standing -> Idle)
            playerObj.StateMachine.ChangeState(playerObj.StandUpState);

            // 배경 다시 밝게
            StartCoroutine(FadeBackground(0f));
        }
    }

    public void OnClickRest()
    {
        menuUI.SetActive(false); // UI 숨기고
        StartCoroutine(RestEffectRoutine()); // 본격 휴식 연출 돌입
    }

    public void OnClickTeleport()
    {
        Debug.Log("순간이동 발동 (함수언제만들래?)");
        menuUI.SetActive(false);
    }

    // [휴식 연출] 화면 완전 암전 -> 회복 -> 기상
    private IEnumerator RestEffectRoutine()
    {
        // 1. 화면이 완전히 까매짐 (알파 1.0)
        yield return StartCoroutine(FadeBackground(1f));

        // 2. HP / MP 100% 회복
        if (playerObj.playerStats != null)
        {
            playerObj.playerStats.currentHp = playerObj.playerStats.maxHp;
            playerObj.playerStats.currentMp = playerObj.playerStats.MaxMp;
        }

        // 3. 완전 암전 상태로 1초 대기 (분위기 깡패)
        yield return new WaitForSeconds(1f);

        // 4. 화면 다시 완전히 밝아짐
        yield return StartCoroutine(FadeBackground(0f));

        // 5. 플레이어 기상 (Standing 애니메이션 후 자동으로 Idle 복귀)
        playerObj.StateMachine.ChangeState(playerObj.StandUpState);
    }

    // [공용 페이드 기능] 현재 알파값에서 목표 알파값으로 전환
    private IEnumerator FadeBackground(float targetAlpha)
    {
        backgroundFadeImage.gameObject.SetActive(true);
        Color c = backgroundFadeImage.color;
        float startAlpha = c.a;
        float time = 0;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            backgroundFadeImage.color = c;
            yield return null;
        }

        c.a = targetAlpha;
        backgroundFadeImage.color = c;

        if (targetAlpha == 0f)
        {
            backgroundFadeImage.gameObject.SetActive(false); // 완전히 밝아지면 이미지 끄기
        }
    }
}