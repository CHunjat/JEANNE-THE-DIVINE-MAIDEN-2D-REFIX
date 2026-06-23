using UnityEngine;

public class PlayerHeavyReadyState : PlayerState
{
    // 유저의 의도를 파악하기 위해 기다리는 시간 (0.15초 ~ 0.2초 추천)
    private float waitTime = 0.15f;

    public PlayerHeavyReadyState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();
        // 공격할 준비를 하니 그 자리에 멈추기
        player.SetVelocity(0f, player.rb.linearVelocity.y);
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        if (player.OnSlope())
        {
            player.rb.gravityScale = 0f; // 2D gravityScale 사용 (중력 끄기)
            player.SetVelocity(0f, 0f);   // 속도 완전 고정 (이동 공격이 아닐 경우)
        }
        else
        {
            // 평지라면 기존 중력/마찰력 로직 유지
            player.SetVelocity(0f, player.rb.linearVelocity.y);
        }
        player.SetVelocity(0f, player.rb.linearVelocity.y);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 1. 변수이용 0.15초를 기준으로 
        if (!player.inputReader.HeavyAttackHeld)
        {
            // TODO: 일반 강공격 State가 따로 있다면 거기로, 없으면 Idle스테이트로 ㄱㄱ
            stateMachine.ChangeState(player.IdleState);
            return;
        }

        // 2. 0.15초 동안 끝까지 꾹 누르고 버티면-> 진짜 기모으기(차지)
        if (stateTimer >= waitTime)
        {
            stateMachine.ChangeState(player.HeavyChargeState);
        }
    }
    public override void Exit()
    {
        base.Exit();
        player.rb.gravityScale = 1f; // 2D gravityScale 사용 (상태 나갈 때 중력 원복 필수!)
    }
}