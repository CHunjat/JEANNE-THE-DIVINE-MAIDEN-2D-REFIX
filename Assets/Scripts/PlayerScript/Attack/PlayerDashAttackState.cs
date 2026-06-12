using UnityEngine;

public class PlayerDashAttackState : PlayerAttackState
{
    private float slideSpeed;
    // 🔥 비탈길 전용 상수 속도 (찌르기보다 조금 더 빠른 느낌으로 설정)
    private float slopeFixedSpeed = 4f;

    private float airborneTimer;

    public PlayerDashAttackState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();
        comboInputRegistered = false;

        airborneTimer = 0f; // 진입 시 타이머 초기화

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
        // 🔥 [핵심 1] 발 밑을 넓게 스캔해서 '평지(각도 0)'가 있는지 독자적으로 찾습니다.
        // ----------------------------------------------------
        bool detectedFlatGround = false;
        RaycastHit2D[] hits = Physics2D.BoxCastAll(player.cd.bounds.center, player.cd.bounds.size * 0.9f, 0f, Vector2.down, 0.3f, player.GetCurrentGroundMask());

        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider != player.ignoredDropCollider)
            {
                if (Vector2.Angle(Vector2.up, hit.normal) <= 0.1f) // 완전 평지 발견!
                {
                    detectedFlatGround = true;
                    break;
                }
            }
        }

        // ----------------------------------------------------
        // 🔥 [비탈길 전용 상수 로직 + 평지 교차점 보정]
        // ----------------------------------------------------
        if (player.OnSlope())
        {
            player.rb.gravityScale = 0f; // 중력 끄기

            if (normalizedTime < 0.8f)
            {
                // ★ [핵심 2] 비탈길과 평지가 '동시 감지'되었다면? (윗평지 교차점)
                if (detectedFlatGround)
                {
                    // 비탈길 오르막 벡터 무시! 윗평지에 찰싹 달라붙도록 미세 하강 벡터(-0.1f) 주입
                    // 속도는 평지 진입이므로 원래 slideSpeed를 사용해 쾌속 유지
                    player.SetVelocity(facingDir * slideSpeed, -0.1f);
                }
                else
                {
                    // 순수 비탈길일 때 (기존 대시 공격 로직 유지)
                    Vector2 moveDir = new Vector2(facingDir, 0f);
                    Vector2 slopeMoveDir = player.GetSlopeMoveDirection(moveDir);

                    float finalSpeedX = slopeFixedSpeed;
                    float finalSpeedY = slopeFixedSpeed;

                    if (slopeMoveDir.y < 0) // 내리막
                    {
                        finalSpeedX = slopeFixedSpeed * 0.45f;
                        finalSpeedY = slopeFixedSpeed * 0.45f;
                    }
                    else if (slopeMoveDir.y > 0) // 오르막
                    {
                        finalSpeedX = slopeFixedSpeed * 1.0f;
                        finalSpeedY = slopeFixedSpeed * 1.6f;
                    }

                    float downwardStickiness = slopeMoveDir.y < 0 ? 2.0f : 1.0f;
                    float extraDownForce = slopeMoveDir.y < 0 ? 4f : 2f;

                    player.SetVelocity(
                        slopeMoveDir.x * finalSpeedX,
                        (slopeMoveDir.y * finalSpeedY * downwardStickiness) - extraDownForce
                    );
                }
            }
            else
            {
                player.SetVelocity(0f, 0f);
            }
        }
        // ----------------------------------------------------
        // 🔥 [평지 로직]
        // ----------------------------------------------------
        else
        {
            player.rb.gravityScale = 1f; // 평지 Gravity 복구

            // ★ 순수 평지 대시 공격일 때도 바닥에 미세하게 눌러줘서 턱에서 안 뜨게 만듦
            float flatYVelocity = detectedFlatGround ? -0.05f : player.rb.linearVelocity.y;

            if (normalizedTime < 0.5f)
            {
                player.SetVelocity(facingDir * slideSpeed, flatYVelocity);
            }
            else if (normalizedTime < 0.8f)
            {
                float slowedSpeed = Mathf.Lerp(slideSpeed, 0f, (normalizedTime - 0.5f) / 0.3f);
                player.SetVelocity(facingDir * slowedSpeed, flatYVelocity);
            }
            else
            {
                player.SetVelocity(0f, flatYVelocity);
            }
        }
    }


    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (!player.IsGrounded() && !player.OnSlope())
        {
            airborneTimer += Time.deltaTime;

            // 0.15초 이상 확실하게 허공에 있을 때만 절벽에서 떨어진 것으로 간주하고 AirState로 전환
            if (airborneTimer > 0.15f)
            {
                stateMachine.ChangeState(player.AirState);
                return;
            }
        }
        else
        {
            // 바닥에 닿아있다면 타이머 즉시 리셋 (다시 단차를 만나도 0.15초 유예 보장)
            airborneTimer = 0f;
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.SetVelocity(0f, player.rb.linearVelocity.y);
        player.rb.gravityScale = 1f; // 2D gravityScale 사용
    }
}