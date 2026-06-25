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

        player.SetVelocity(0f, player.rb.linearVelocity.y);
        Debug.Log("힐 시전 시작... (무방비 상태)");
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

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
        AnimatorStateInfo stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);
        if (!stateInfo.IsName(player.anim_Heal)) return;


        // 애니메이션이 50% 진행되었을 때 진짜 회복 로직 
        if (!hasHealed && GetNormalizedTime() >= 0.5f)
        {
            hasHealed = true;

            // PlayerController에 연결해둔 스탯 스크립트의 Heal 함수를 부릅니다. (예: 50 회복)
            if (player.playerStats != null)
            {
                player.playerStats.Heal(player.healAmount, player.healMpCost); 
               
            }
        }

        if (GetNormalizedTime() >= 1.0f)
        {
            stateMachine.ChangeState(player.IdleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.rb.gravityScale = 1f;
    }
}