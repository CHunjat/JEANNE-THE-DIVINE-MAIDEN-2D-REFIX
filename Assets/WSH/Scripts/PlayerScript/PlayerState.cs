using UnityEngine;

public abstract class PlayerState
{
    protected PlayerController player;
    protected PlayerStateMachine stateMachine;
    protected int animHash;
    protected float stateTimer; // 모든상태에서 타이머 할수있게 추가

    public PlayerState(PlayerController player, PlayerStateMachine stateMachine, string animName)
    {
        this.player = player;
        this.stateMachine = stateMachine;
        this.animHash = UnityEngine.Animator.StringToHash(animName);
    }

    public virtual void Enter() 
    {
        player.animator.CrossFade(animHash,0.1f);
        stateTimer = 0;
    } 
    public virtual void HandleInput() { }

    protected float GetNormalizedTime()
    {
        // 현재 0번 레이어에서 재생 중인 애니메이션 정보 가져오기
        AnimatorStateInfo currentInfo = player.animator.GetCurrentAnimatorStateInfo(0);
        return currentInfo.normalizedTime;
    }
    public virtual void LogicUpdate()
    {
        stateTimer += Time.deltaTime;
    }
    public virtual void PhysicsUpdate() { }
    public virtual void Exit() { }
}