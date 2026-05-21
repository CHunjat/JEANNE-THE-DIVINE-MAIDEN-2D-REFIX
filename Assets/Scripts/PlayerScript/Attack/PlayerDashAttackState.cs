using UnityEngine;

public class PlayerDashAttackState : PlayerAttackState
{
    private float slideSpeed;
    // 🔥 비탈길 전용 상수 속도 (찌르기보다 조금 더 빠른 느낌으로 설정)
    private float slopeFixedSpeed = 4f;

    public PlayerDashAttackState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();
        comboInputRegistered = false;

        if (player.OnSlope())
        {
            // 진입 쐐기: 수직 관성 즉시 제거 및 바닥 밀착
            player.rb.linearVelocity = new Vector2(player.rb.linearVelocity.x, 0f); // 2D Vector
            player.rb.MovePosition(player.rb.position + Vector2.down * 0.15f); // 2D Vector
        }

        if (player.isSprinting)
        {
            slideSpeed = player.moveSpeed * 1.2f;
            player.isSprinting = false;
        }
        else
        {
            slideSpeed = player.moveSpeed * 1.0f;
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        float facingDir = player.isFacingRight ? 1f : -1f;
        float normalizedTime = GetNormalizedTime();

        // ----------------------------------------------------
        // 🔥 [비탈길 전용 상수 로직]
        // ----------------------------------------------------
        if (player.OnSlope())
        {
            player.rb.gravityScale = 0f; // 2D gravityScale 사용 (중력 끄기)

            // 1. 공격 전반부 (0% ~ 80%): 상수 속도로 밀기
            // 찌르기와 마찬가지로 계산 오차를 줄이기 위해 상수를 강제 주입합니다.
            if (normalizedTime < 0.8f)
            {
                Vector2 moveDir = new Vector2(facingDir, 0f); // Vector2로 전환
                Vector2 slopeMoveDir = player.GetSlopeMoveDirection(moveDir);

                float finalSpeed = slopeFixedSpeed;
                if (slopeMoveDir.y < 0)
                {
                    // 내리막일 때 속도를 약 20~30% 줄여서 평지와 체감 속도를 맞춥니다.
                    finalSpeed = slopeFixedSpeed * 0.45f;
                }
                else if (slopeMoveDir.y > 0) // 오르막
                {
                    finalSpeed = slopeFixedSpeed * 1.6f;  // 오르막이 답답하면 여기서 속도를 가산
                }

                // 3타와 동일한 자석 수치 및 하향 압력(-4f) 적용
                float downwardStickiness = slopeMoveDir.y < 0 ? 2.0f : 1.0f;

                player.SetVelocity(
                    slopeMoveDir.x * finalSpeed,
                    (slopeMoveDir.y * finalSpeed * downwardStickiness) - 4f
                );
            }
            // 2. 후딜레이 (80% ~ 100%): 3타급 0,0 쐐기
            else
            {
                player.SetVelocity(0f, 0f);
            }
        }
        // ----------------------------------------------------
        // ⚡ [평지 전용 가변 로직] - 평지는 부드러운 감속 유지
        // ----------------------------------------------------
        else
        {
            player.rb.gravityScale = 1f; // 2D gravityScale 사용 (평지 Gravity 복구)

            if (normalizedTime < 0.5f)
            {
                player.SetVelocity(facingDir * slideSpeed, player.rb.linearVelocity.y);
            }
            else if (normalizedTime < 0.8f)
            {
                float slowedSpeed = Mathf.Lerp(slideSpeed, 0f, (normalizedTime - 0.5f) / 0.3f);
                player.SetVelocity(facingDir * slowedSpeed, player.rb.linearVelocity.y);
            }
            else
            {
                player.SetVelocity(0f, player.rb.linearVelocity.y);
            }
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 내리막 팅김 방지 가드
        if (!player.IsGrounded() && !player.OnSlope())
        {
            stateMachine.ChangeState(player.AirState);
            return;
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.SetVelocity(0f, player.rb.linearVelocity.y);
        player.rb.gravityScale = 1f; // 2D gravityScale 사용
    }
}