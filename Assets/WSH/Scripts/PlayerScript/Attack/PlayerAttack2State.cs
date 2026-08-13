using UnityEngine;

public class PlayerAttack2State : PlayerAttackState
{
    private new bool comboInputRegistered;

    public PlayerAttack2State(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();
        base.comboInputRegistered = false;

        player.SetVelocity(0f, player.rb.linearVelocity.y);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (player.inputReader.AttackPressed)
        {
            player.inputReader.AttackPressed = false;
            base.comboInputRegistered = true;
        }



        // 2타에서 3타로 넘어가는 타이밍 (마찬가지로 0.6f 조절)
        if (base.comboInputRegistered && stateTimer > 0.1f && GetNormalizedTime() >= 0.7f)
        {
            stateMachine.ChangeState(player.Attack3State); // 2타 스크립트면 Attack3State로
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        // 🔥 공격 중에는 비탈길에서 본드처럼 딱 붙어있어야 함
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
    }

    public override void Exit()
    {
        base.Exit();
        player.rb.gravityScale = 1f; // 2D gravityScale 사용 (상태 나갈 때 중력 원복 필수!)
    }
}