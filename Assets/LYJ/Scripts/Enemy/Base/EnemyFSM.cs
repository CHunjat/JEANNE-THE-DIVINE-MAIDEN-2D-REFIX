using UnityEngine;
// =====================================================
// EnemyFSM.cs
// 적의 행동 상태(FSM = Finite State Machine)를 관리하는 클래스임.
// Idle(대기) → Chase(추격) → Attack(공격) → Hit(피격) → Groggy(그로기) → Dead(사망)
// 상태에 따라 매 프레임 다른 행동을 수행함.
// EnemyBase를 상속받고, 각 몬스터 스크립트는 이 클래스를 상속받음.
// =====================================================
public abstract class EnemyFSM : EnemyBase
{
    // 적이 가질 수 있는 행동 상태 목록
    public enum EnemyState
    {
        Idle,    // 대기 상태 - 플레이어를 감지하지 못한 상태
        Chase,   // 추격 상태 - 플레이어를 감지하고 쫓아가는 상태
        Attack,  // 공격 상태 - 플레이어가 공격 범위 안에 들어온 상태
        Hit,     // 피격 상태 - 공격을 맞은 직후 상태
        Groggy,  // 그로기 상태 - 그로기 게이지가 가득 차서 무방비 상태
        Dead     // 사망 상태 - 체력이 0 이하가 된 상태
    }
    [Header("현재 상태 (인스펙터에서 실시간 확인 가능)")]
    [SerializeField] private EnemyState currentState;
    protected override void Awake()
    {
        base.Awake();
        currentState = EnemyState.Idle; // 시작은 항상 대기 상태
    }
    protected virtual void Update()
    {
        // 현재 상태에 맞는 함수를 매 프레임 실행함
        switch (currentState)
        {
            case EnemyState.Idle: OnIdle(); break;
            case EnemyState.Chase: OnChase(); break;
            case EnemyState.Attack: OnAttack(); break;
            case EnemyState.Hit: OnHit(); break;
            case EnemyState.Groggy: OnGroggy(); break;
            case EnemyState.Dead: OnDead(); break;
        }
    }
    // 상태 전환 함수 - 같은 상태로는 전환하지 않음 (불필요한 중복 실행 방지)
    protected void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        Debug.Log($"[{gameObject.name}] 상태 전환 → {newState}");
    }

    // [추가됨] 외부 컴포넌트(EnemyGroggy 등)에서 상태를 강제로 바꿀 수 있게 열어주는 함수.
    // ChangeState 자체는 protected로 안전하게 막아두고, 이 함수를 통해서만 외부 접근 허용.
    public void ForceChangeState(EnemyState newState)
    {
        ChangeState(newState);
    }

    // 현재 상태를 외부에서 읽을 수 있도록 반환함
    public EnemyState GetCurrentState() => currentState;
    // 각 상태별 동작 함수 - 자식 클래스(NormalMonster, MidBoss 등)에서 내용을 채움
    protected virtual void OnIdle() // 대기 상태
    {

    }
    protected virtual void OnChase()   // 추격 상태
    {
    }
    protected virtual void OnAttack()  // 공격 상태
    {
    }
    protected virtual void OnHit()     // 피격 상태
    {
    }
    protected virtual void OnGroggy()  // 그로기 상태 - 기본적으로 아무것도 안 함 (무방비)
    {

    }
    protected virtual void OnDead()    // 사망 상태
    {

    }
}