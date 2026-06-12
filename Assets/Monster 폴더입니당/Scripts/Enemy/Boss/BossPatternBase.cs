using UnityEngine;

// =====================================================
// BossPatternBase.cs
// 보스의 모든 패턴(공격 기술)이 공통으로 가지는 기반 클래스임.
// 쿨타임 관리와 패턴 실행 기능을 담당함.
// MidBossPattern1~5, FinalBossPattern1~5 등 모든 패턴 스크립트가
// 이 클래스를 상속받아 만들어짐.
//
// [사용 방법]
// 자식 클래스에서 OnExecute() 함수 안에 패턴 내용을 채우면 됨.
// =====================================================
public abstract class BossPatternBase : MonoBehaviour
{
    [Header("패턴 쿨타임 (초 단위) - 기획 확정 후 수정할 것")]
    [SerializeField] protected float cooldown = 3f;  // 패턴 재사용 대기 시간

    private float lastUsedTime = -999f;  // 마지막으로 패턴을 사용한 시간 (처음엔 항상 사용 가능하도록 -999 설정)

    // 현재 이 패턴을 사용할 수 있는지 확인함 (쿨타임이 지났는지 체크)
    public bool IsUsable()
    {
        return Time.time >= lastUsedTime + cooldown;
    }

    // 패턴 실행 - MidBoss나 FinalBoss의 OnAttack()에서 호출함
    public void Execute()
    {
        lastUsedTime = Time.time;  // 사용 시각 기록
        OnExecute();               // 실제 패턴 내용 실행
    }

    // 실제 패턴 내용 - 자식 클래스에서 반드시 구현해야 함
    protected abstract void OnExecute();
}