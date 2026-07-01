using UnityEngine;

// =====================================================
// BossPatternBase.cs
// 보스의 모든 패턴(공격 기술)이 공통으로 가지는 기반 클래스임.
// 쿨타임 관리와 패턴 실행 기능을 담당함.
// =====================================================
public abstract class BossPatternBase : MonoBehaviour
{
    [Header("패턴 쿨타임 (초 단위) - 기획 확정 후 수정할 것")]
    [SerializeField] protected float cooldown = 3f;  // 패턴 재사용 대기 시간임.

    private float lastUsedTime = -999f;  // 마지막으로 패턴을 사용한 시간 (처음엔 항상 사용 가능하도록 -999 설정함)

    // [수정됨] 자식 클래스(예: MidBossPattern5)에서 기획에 맞춰 조건을 덮어쓸 수 있도록 'virtual' 키워드를 추가함!
    public virtual bool IsUsable()
    {
        return Time.time >= lastUsedTime + cooldown;
    }

    // 패턴 실행 - MidBoss나 FinalBoss의 OnAttack()에서 호출함.
    public void Execute()
    {
        lastUsedTime = Time.time;  // 사용 시각 기록함.
        OnExecute();               // 실제 패턴 내용 실행함.
    }

    // 실제 패턴 내용 - 자식 클래스에서 반드시 구현해야 함.
    protected abstract void OnExecute();
}