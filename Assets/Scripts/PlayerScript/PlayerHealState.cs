using UnityEngine;

public class PlayerHealState : PlayerState
{
    private bool hasHealed;

    public PlayerHealState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();
        hasHealed = false;

        // 힐을 시작하면 제자리에 멈춰야 함 (이동 불가)
        player.SetVelocity(0f, player.rb.linearVelocity.y);
        Debug.Log("힐 시전 시작... (무방비 상태)");
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        // 비탈길에서도 미끄러지지 않고 서서 힐을 하도록 고정
        if (player.OnSlope())
        {
            player.rb.gravityScale = 0f;
            player.SetVelocity(0f, 0f);
        }
        else
        {
            player.SetVelocity(0f, player.rb.linearVelocity.y);
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 애니메이션 진행도가 50%를 넘겼을 때 실제 체력을 회복시킴 (타이밍 조절 가능)
        if (!hasHealed && GetNormalizedTime() >= 0.5f)
        {
            hasHealed = true;
            Debug.Log("체력 회복 완료! (+50)");
            // TODO: player.GetComponent<PlayerStats>().Heal(50); 등 실제 회복 로직 연동
        }

        // 애니메이션이 완전히 끝나면 자동으로 Idle 상태로 복귀
        if (GetNormalizedTime() >= 1.0f)
        {
            stateMachine.ChangeState(player.IdleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.rb.gravityScale = 1f; // 중력 원상 복구
    }
}