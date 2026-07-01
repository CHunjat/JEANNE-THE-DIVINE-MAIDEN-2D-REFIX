using UnityEngine;
// =====================================================
// EnemyGroggy.cs
// 그로기(Groggy) 시스템 - 보스 및 정예 몬스터 공용 컴포넌트.
// 이 컴포넌트를 그로기가 필요한 몬스터 오브젝트에 붙이면 동작함.
// (일반 몬스터에는 붙이지 않음 - 공용 시스템이지만 선택적 부착)
//
// [기획 확정 사항]
// - 게이지 0 → 100으로 쌓이는 방식
// - 모든 공격(평타 포함)에 게이지 상승, 패링은 게이지 안 오름
// - 게이지 가득 차면 그로기 진입, 일정 시간 무방비
// - 그로기 중 받는 피해 250% (기본값, 인스펙터에서 조절 가능)
// - 일정 시간 피격 안 하면 게이지 서서히 자연 회복
// - 그로기 끝나야 페이즈 전환 (보류 중이던 페이즈 전환은 그로기 해제 시 처리)
//
// [기획 미정 사항 - 추후 교체 예정]
// - 게이지 증가 공식 (데미지 * 저항값 * 공격타입 * k) → 현재는 데미지값 그대로 사용하는 임시 공식
// - 그로기 지속시간 / 자연회복 속도 → 몬스터 데이터 테이블 연동 예정, 현재는 인스펙터 임시값
// =====================================================
[RequireComponent(typeof(EnemyFSM))]
public class EnemyGroggy : MonoBehaviour
{
    [Header("그로기 게이지 설정 (기획 확정 후 데이터 테이블과 연동 예정)")]
    [SerializeField] private float maxGauge = 100f;

    [Header("그로기 지속시간 / 회복 (임시값 - 몬스터 데이터 테이블 확정 시 교체)")]
    [SerializeField] private float groggyDuration = 5f;
    [SerializeField] private float recoverPerSecond = 2f;
    [SerializeField] private float recoverDelayAfterHit = 2f; // 마지막 피격 후 이 시간 지나야 회복 시작

    [Header("그로기 중 데미지 배율 (기획: 250%)")]
    [SerializeField] private float damageMultiplier = 2.5f;

    [Header("애니메이션 트리거 이름")]
    [SerializeField] private string groggyTriggerName = "doKnockDown";

    private float currentGauge = 0f;
    private float lastHitTime = -999f;
    private bool isGroggy = false;
    private bool pendingPhaseTransition = false;

    private EnemyFSM fsm;
    private Animator animator;

    public bool IsGroggy => isGroggy;
    public float CurrentGauge => currentGauge;
    public float MaxGauge => maxGauge;

    private void Awake()
    {
        fsm = GetComponent<EnemyFSM>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        // 그로기 중이 아니고, 게이지가 남아있고, 마지막 피격 후 일정 시간 지났으면 서서히 회복
        if (!isGroggy && currentGauge > 0f && Time.time > lastHitTime + recoverDelayAfterHit)
        {
            currentGauge = Mathf.Max(0f, currentGauge - recoverPerSecond * Time.deltaTime);
        }
    }

    // 데미지 받을 때마다 외부(MidBoss.TakeDamage 등)에서 호출.
    // rawDamage는 배율 적용 전 원본 데미지값.
    public void AddGauge(float rawDamage)
    {
        if (isGroggy) return; // 이미 그로기 중이면 게이지 추가 안 함
        if (fsm != null && fsm.GetCurrentState() == EnemyFSM.EnemyState.Dead) return;

        lastHitTime = Time.time;

        // [임시 공식] 추후 "데미지 * (1-그로기저항값) * 공격타입 * k" 로 교체 예정
        float gaugeIncrease = rawDamage;

        currentGauge = Mathf.Min(currentGauge + gaugeIncrease, maxGauge);

        if (currentGauge >= maxGauge)
        {
            EnterGroggy();
        }
    }

    // 그로기 상태일 때 받는 데미지 배율 - MidBoss.TakeDamage에서 곱해서 사용
    public float GetDamageMultiplier()
    {
        return isGroggy ? damageMultiplier : 1f;
    }

    // 보스 쪽에서 페이즈 전환 조건을 만족했는데 지금 그로기 중이면, 그로기 끝날 때까지 보류시켜달라고 요청하는 함수
    public void RequestPendingPhaseTransition()
    {
        pendingPhaseTransition = true;
    }

    private void EnterGroggy()
    {
        isGroggy = true;
        currentGauge = maxGauge;

        if (animator != null)
        {
            animator.SetTrigger(groggyTriggerName);
        }

        if (fsm != null)
        {
            fsm.ForceChangeState(EnemyFSM.EnemyState.Groggy);
        }

        Invoke(nameof(ExitGroggy), groggyDuration);

        Debug.Log($"<color=orange>[{gameObject.name}] 그로기 진입! {groggyDuration}초간 무방비 상태.</color>");
    }

    private void ExitGroggy()
    {
        isGroggy = false;
        currentGauge = 0f;

        if (fsm != null && fsm.GetCurrentState() != EnemyFSM.EnemyState.Dead)
        {
            fsm.ForceChangeState(EnemyFSM.EnemyState.Chase);
        }

        Debug.Log($"<color=lime>[{gameObject.name}] 그로기 해제.</color>");

        // 그로기 중에 페이즈 전환 조건이 걸려있었다면 여기서 처리
        if (pendingPhaseTransition)
        {
            pendingPhaseTransition = false;
            SendMessage("OnGroggyEndedPhaseTransition", SendMessageOptions.DontRequireReceiver);
        }
    }
}