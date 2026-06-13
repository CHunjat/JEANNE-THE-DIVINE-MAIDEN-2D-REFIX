using UnityEngine;
using System.Collections;

public class MidBossPattern4 : BossPatternBase
{
    [Header("점프 공격 설정 (기획자님, 여기서 수치 조절하시면 됩니다!)")]
    [SerializeField] private float airTime = 2f;                     // 보스가 점프해서 공중에 체공하는 시간 (초)
    [SerializeField] private float hitboxActiveDuration = 0.4f;      // 바닥에 쾅 찍고 나서 충격파 판정이 남아있는 시간 (초)

    [Header("히트박스 연결 (건드리지 마세요!)")]
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

        // 공중에 떠 있는 시간만큼 대기 (위에서 설정한 airTime 수치만큼)
        yield return new WaitForSeconds(airTime);

        // 플레이어 현재 위치 찾아서 보스를 거기로 순간이동 시킴 (타겟팅 낙하)
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            transform.position = playerObj.transform.position;

        // 보스 다시 화면에 나타남
        if (visual != null)
            visual.gameObject.SetActive(true);

        // 착지 애니메이션 재생!
        if (visualAnimator != null)
            visualAnimator.Play("jump attack land");

        // [가장 중요한 타격 판정 부분]
        if (landingHitbox != null)
        {
            landingHitbox.SetActive(true);                           // 바닥에 쾅! 찍는 순간 충격파(히트박스) 켜기
            yield return new WaitForSeconds(hitboxActiveDuration);   // 아주 잠깐만 판정 유지
            landingHitbox.SetActive(false);                          // 다시 끄기 (안 끄면 지나가다 계속 맞음)
        }

        isJumping = false;
    }
}