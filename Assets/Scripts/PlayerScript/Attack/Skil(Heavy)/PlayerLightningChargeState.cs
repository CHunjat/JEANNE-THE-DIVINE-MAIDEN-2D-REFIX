using UnityEngine;

public class PlayerLightningChargeState : PlayerState
{
    private bool isFullyCharged;
    private float fullChargeTime = 1.0f; // 풀차지에 걸리는 시간

    public PlayerLightningChargeState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();
        isFullyCharged = false;
        player.SetVelocity(0f, player.rb.linearVelocity.y);

        Debug.Log("라이트닝 컷 기 모으는 중...");
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

        // ★ [핵심 수정] 여기서도 HeavyAttackHeld를 검사합니다. 손을 떼는 순간 공격!
        if (!player.inputReader.HeavyAttackHeld)
        {
            stateMachine.ChangeState(player.LightningAttackState);
            return;
        }

  

        // 풀차지 달성 체크 (이펙트 추가용)
        if (stateTimer >= fullChargeTime && !isFullyCharged)
        {
            isFullyCharged = true;
            Debug.Log("라이트닝 컷 풀차지 완료! (번쩍)");
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.rb.gravityScale = 1f;
    }
}