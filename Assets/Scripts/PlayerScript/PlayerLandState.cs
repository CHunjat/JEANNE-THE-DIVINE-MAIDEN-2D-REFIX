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

        // [구르기 판정] 안전하게 검사
        bool isStuck = Physics2D.OverlapBox(player.transform.position, player.cd.bounds.size * 0.9f, 0f, player.groundLayer | player.stairsLayer) != null;

        if (player.isSprinting)
        {
            // 🔥 [수정] 강제로 rb.linearVelocity = ... 로 속도를 0으로 덮어쓰지 마세요!
            // 착지 직전의 하강 속도를 그대로 유지한 상태로 애니메이션만 재생합니다.
            // 물리 엔진은 이 속도(관성)를 보고 비탈길인지 아닌지를 부드럽게 판단합니다.
            player.animator.Play(player.anim_SprintLand, 0, 0);
        }
        else
        {
            player.rb.linearVelocity = Vector2.zero;
            player.animator.CrossFade(animHash, 0.1f);
        }
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (player.isSprinting) { if (stateTimer < 0.4f) return; }
        else { if (stateTimer < 0.1f) return; }

        if (player.inputReader.MoveValue.x != 0) { stateMachine.ChangeState(player.MoveState); return; }
        if (stateTimer > 0.5f) { stateMachine.ChangeState(player.IdleState); }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        // 🔥 [1번 문제 해결] 랜딩 직후 모서리/경계면 턱에 부딪혀 위로 튀어오르려 할 때(반발력 차단)
        if (player.rb.linearVelocity.y > 0.05f)
        {
            // 위로 튀는 속도를 즉시 죽여서 점프대 현상을 원천 차단합니다.
            player.rb.linearVelocity = new Vector2(player.rb.linearVelocity.x, 0f);
        }

        if (player.isSprinting)
        {
            float dir = player.isFacingRight ? 1f : -1f;
            float currentSpeed = player.sprintSpeed;

            if (player.OnSlope())
            {
                player.rb.gravityScale = 0f;
                Vector2 moveDir = new Vector2(dir, 0f);
                Vector2 slopeMoveDir = player.GetSlopeMoveDirection(moveDir);

                float rollSpeed = currentSpeed;
                if (slopeMoveDir.y > 0)
                {
                    rollSpeed = 8.0f; // 오르막 제한 속도
                }
                else
                {
                    rollSpeed = player.sprintSpeed;
                }

                player.rb.linearVelocity = slopeMoveDir * rollSpeed;
                player.rb.AddForce(Vector2.down * 50f, ForceMode2D.Force);
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