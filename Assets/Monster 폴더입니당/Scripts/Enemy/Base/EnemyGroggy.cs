using UnityEngine;
// =====================================================
// EnemyGroggy.cs
// =====================================================
[RequireComponent(typeof(EnemyFSM))]
public class EnemyGroggy : MonoBehaviour
{
    [Header("그로기 게이지 설정 (기획 확정 후 데이터 테이블과 연동 예정)")]
    [SerializeField] private float maxGauge = 100f;

    [Header("그로기 지속시간 / 회복 (임시값 - 몬스터 데이터 테이블 확정 시 교체)")]
    [SerializeField] private float groggyDuration = 5f;
    [SerializeField] private float recoverPerSecond = 2f;
    [SerializeField] private float recoverDelayAfterHit = 2f;

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
        if (!isGroggy && currentGauge > 0f && Time.time > lastHitTime + recoverDelayAfterHit)
        {
            currentGauge = Mathf.Max(0f, currentGauge - recoverPerSecond * Time.deltaTime);
        }
    }

    public void AddGauge(float rawDamage)
    {
        if (isGroggy) return;
        if (fsm != null && fsm.GetCurrentState() == EnemyFSM.EnemyState.Dead) return;

        lastHitTime = Time.time;
        float gaugeIncrease = rawDamage;

        currentGauge = Mathf.Min(currentGauge + gaugeIncrease, maxGauge);

        if (currentGauge >= maxGauge)
        {
            EnterGroggy();
        }
    }

    public float GetDamageMultiplier()
    {
        return isGroggy ? damageMultiplier : 1f;
    }

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

        // 수정 : 게이지가 차서 터진 그로기임을 명확하게 표시
        Debug.Log($"<color=orange><b>[EnemyGroggy] 게이지 100% 달성! 그로기 넉다운 진입! ({groggyDuration}초간)</b></color>");
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

        if (pendingPhaseTransition)
        {
            pendingPhaseTransition = false;
            SendMessage("OnGroggyEndedPhaseTransition", SendMessageOptions.DontRequireReceiver);
        }
    }
}