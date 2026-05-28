using UnityEngine;

public class PlayerLandState : PlayerState
{
    public PlayerLandState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        player.ToggleStairsCollision(true);
        stateTimer = 0f;
        player.ResetLandTimer();

        if (player.isSprinting)
        {
            // [스프린트 착지] 구르기를 위해 기존 X축 속도를 가져오되, 한계치(sprintSpeed)를 넘지 않게 제한
            float currentXVel = player.rb.linearVelocity.x;
            if (Mathf.Abs(currentXVel) > player.sprintSpeed)
            {
                currentXVel = Mathf.Sign(currentXVel) * player.sprintSpeed;
            }

            player.rb.linearVelocity = new Vector2(currentXVel, 0f);
            player.animator.Play(player.anim_SprintLand, 0, 0);
        }
        else
        {
            // [일반 / 그래플 착지] 미끄러짐을 완벽히 차단하기 위해 속도 즉시 0
            player.rb.linearVelocity = Vector2.zero;
            player.animator.CrossFade(animHash, 0.1f);
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 스프린트 착지(구르기)는 0.4초 대기, 일반 착지는 0.1초 대기
        if (player.isSprinting)
        {
            if (stateTimer < 0.4f) return;
        }
        else
        {
            if (stateTimer < 0.1f) return;
        }

        // 대기 시간이 끝난 후 입력이 있으면 이동 상태로 전환
        if (player.inputReader.MoveValue.x != 0)
        {
            stateMachine.ChangeState(player.MoveState);
            return;
        }

        // 입력이 없으면 0.5초 후 대기 상태로 전환
        if (stateTimer > 0.5f)
        {
            stateMachine.ChangeState(player.IdleState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        // 구르는(스프린트 착지) 동안의 물리 처리
        if (player.isSprinting)
        {
            float dir = player.isFacingRight ? 1f : -1f;
            float currentSpeed = player.sprintSpeed;

            if (player.OnSlope())
            {
                player.rb.gravityScale = 0f;
                Vector2 moveDir = new Vector2(dir, 0f);
                Vector2 slopeMoveDir = player.GetSlopeMoveDirection(moveDir);

                // 구르는 중 내리막을 만나도 붕 뜨지 않게 -4f 접착력 적용 (아주 좋습니다!)
                player.SetVelocity(slopeMoveDir.x * currentSpeed, (slopeMoveDir.y * currentSpeed) - 4f);
            }
            else
            {
                player.rb.gravityScale = 1f;
                player.SetVelocity(dir * currentSpeed, player.rb.linearVelocity.y);
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.rb.gravityScale = 1f;
    }
}