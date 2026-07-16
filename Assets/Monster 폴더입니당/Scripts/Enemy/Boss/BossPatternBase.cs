using UnityEngine;
// =====================================================
// BossPatternBase.cs
// 보스의 모든 패턴(공격 기술)이 공통으로 가지는 기반 클래스임.
// 쿨타임, 우선순위, 거리 타입 관리와 패턴 실행 기능을 담당함.
// (클리어링이 다른 패턴 실행 도중 끼어드는 것을 막기 위한 IsBusy 훅 추가)
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

    // [추가됨] 이 패턴이 지금 여러 타격이 이어지는 애니메이션 도중인지 여부.
    // 기본값 false. 여러 타격이 이어지는 패턴(6, 7, 8 등)만 오버라이드해서
    // 자기 isExecuting 상태를 연결해줌. MidBoss가 이걸 보고 클리어링 끼어들기를 막음.
    public virtual bool IsBusy => false;

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