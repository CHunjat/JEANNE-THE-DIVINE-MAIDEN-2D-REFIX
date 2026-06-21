using UnityEngine;
using System.Collections;

public class MidBossPattern4 : BossPatternBase
{
    [Header("점프 공격 설정 (기획서 맞춤)")]
    [SerializeField] private float trackTime = 2.7f;         // 공중에서 플레이어를 따라다니는 시간 (초)
    [SerializeField] private float dropDelay = 0.3f;         // 추적 멈추고 떨어지기 전 딜레이 (회피 타이밍!)
    [SerializeField] private float hitboxActiveDuration = 1.0f; // 바닥에 쾅 찍고 나서 충격파 판정이 남아있는 시간 (초)

    [Header("히트박스 연결 (건드리면 안되는 거)")]
    [SerializeField] private GameObject landingHitbox;

    private MidBoss owner;
    private bool isJumping = false;
    private Animator visualAnimator;

    private void Awake()
    {
        cooldown = 8f;  // 이거 임시 쿨타임. 나중에 밸런스 보고 수정.
        visualAnimator = GetComponentInChildren<Animator>();
        owner = GetComponent<MidBoss>();

        // 시작할 때는 당연히 타격 판정이 꺼져있어야 하니까 꺼둠
        if (landingHitbox != null)
            landingHitbox.SetActive(false);
    }

    protected override void OnExecute()
    {
        if (isJumping) return;
        Debug.Log("패턴 4번(점프 공격) 시작!");
        if (visualAnimator != null) visualAnimator.Play("jump attack up");

        // 점프 뛰고 -> 기다렸다가 -> 떨어지는 흐름 시작
        StartCoroutine(JumpRoutine());
    }

    private IEnumerator JumpRoutine()
    {
        isJumping = true;

        // 기획서대로 점프하면 화면에서 잠깐 안 보이게 숨기는 부분
        Transform visual = transform.Find("Visual");
        if (visual != null)
            visual.gameObject.SetActive(false);

        GameObject playerObj = GameObject.FindWithTag("Player");

        // 1. [핵심] 2.7초 동안 매 프레임마다 플레이어의 X 좌표를 따라다님! (유도탄)
        float timer = 0f;
        while (timer < trackTime)
        {
            if (playerObj != null)
            {
                // 보스의 X축 위치만 플레이어의 X축 위치로 계속 갱신 (Y축 공중 유지는 그대로)
                transform.position = new Vector2(playerObj.transform.position.x, transform.position.y);
            }
            timer += Time.deltaTime; // 시간 깎기
            yield return null;       // 1프레임 대기 후 while문 반복
        }

        // 2. [핵심] 2.7초 끝! 이제 추적을 딱 멈추고 0.3초 대기 (이때 플레이어가 대시로 도망가야 함!)
        yield return new WaitForSeconds(dropDelay);

        // 3. 0.3초 지났으니 낙하! (Y축 위치를 바닥/플레이어 위치로 맞춰줌)
        if (playerObj != null)
        {
            transform.position = new Vector2(transform.position.x, playerObj.transform.position.y);
        }

        // 4. 보스 다시 나타나고 착지 애니메이션 재생
        if (visual != null)
            visual.gameObject.SetActive(true);

        if (visualAnimator != null)
            visualAnimator.Play("jump attack land");

        // 5. 타격 판정 (저번에 깎아둔 1초 타이밍)
        if (landingHitbox != null)
        {
            landingHitbox.SetActive(true);                            // 바닥에 쾅! 찍는 순간 충격파(히트박스) 켜기
            yield return new WaitForSeconds(hitboxActiveDuration);    // 1초간 판정 유지
            landingHitbox.SetActive(false);                           // 다시 끄기 (안 끄면 지나가다 계속 맞음)
        }

        isJumping = false;
    }
}