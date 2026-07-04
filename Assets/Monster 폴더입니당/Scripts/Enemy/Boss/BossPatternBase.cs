using UnityEngine;
// =====================================================
// BossPatternBase.cs
// 보스의 모든 패턴(공격 기술)이 공통으로 가지는 기반 클래스임.
// 쿨타임, 우선순위, 거리 타입 관리와 패턴 실행 기능을 담당함.
// =====================================================
public abstract class BossPatternBase : MonoBehaviour
{
    public enum DistanceType
    {
        Close,   // 근거리: 0~5m
        Mid,     // 중거리: 5~10m
        Far,     // 원거리: 10~20m
        Any      // 거리 무관
    }

    [Header("패턴 기본 설정 (기획자 조절)")]
    [SerializeField] protected float cooldown = 3f;
    [SerializeField] public int priority = 3;           // 1이 가장 높은 우선순위
    [SerializeField] public DistanceType distanceType = DistanceType.Mid;

    [Header("추격 중 사용 여부")]
    [SerializeField] public bool canUseInChase = false; // true면 빨간 원 밖(추격 중)에서도 쿨타임 돌면 쏨

    private float lastUsedTime = -999f;

    public virtual bool IsUsable()
    {
        return Time.time >= lastUsedTime + cooldown;
    }

    public void Execute()
    {
        lastUsedTime = Time.time;
        OnExecute();
    }

    protected abstract void OnExecute();
}