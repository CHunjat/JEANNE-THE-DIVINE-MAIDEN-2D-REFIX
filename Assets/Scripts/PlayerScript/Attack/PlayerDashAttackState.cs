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
            player.rb.gravityScale = 0f; // 중력 끄기

            if (normalizedTime < 0.8f)
            {
                Vector2 moveDir = new Vector2(facingDir, 0f);
                Vector2 slopeMoveDir = player.GetSlopeMoveDirection(moveDir);

                // X축과 Y축의 속도를 분리해서 제어합니다.
                float finalSpeedX = slopeFixedSpeed;
                float finalSpeedY = slopeFixedSpeed;

                if (slopeMoveDir.y < 0) // 내리막
                {
                    finalSpeedX = slopeFixedSpeed * 0.45f;
                    finalSpeedY = slopeFixedSpeed * 0.45f;
                }
                else if (slopeMoveDir.y > 0) // 오르막
                {
                    // ⚡ [개발자님 아이디어 적용] 오르막 밀착 로직
                    // Y축(올라가는 힘)은 시원하게 유지하되, X축(앞으로 가는 힘)을 줄여서 계단에 바짝 붙게 만듭니다.
                    finalSpeedX = slopeFixedSpeed * 1.0f; // 1.6f -> 1.0f로 깎아서 가로 튀어나감 방지
                    finalSpeedY = slopeFixedSpeed * 1.6f;
                }

                float downwardStickiness = slopeMoveDir.y < 0 ? 2.0f : 1.0f;
                float extraDownForce = slopeMoveDir.y < 0 ? 4f : 2f;

                // 분리된 X, Y 속도를 각각 적용
                player.SetVelocity(
                    slopeMoveDir.x * finalSpeedX,
                    (slopeMoveDir.y * finalSpeedY * downwardStickiness) - extraDownForce
                );
            }
            else
            {
                player.SetVelocity(0f, 0f);
            }
        }
        else
        {
            player.rb.gravityScale = 1f; // 평지 Gravity 복구

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