using UnityEngine;

public class PlayerLightningAttackState : PlayerAttackState
{
    private float dashSpeed = 25f;
    private float dashDuration = 0.2f;
    private float dashTimer;
    private float facingDir; // 0이면 제자리, 1/-1이면 해당 방향

   
    //나중에 판정 여기서 수정(몬스터 연동 됐을때)
    public float hitRange = 0.5f; // 적 감지 전방 거리
    public Vector2 hitSize = new Vector2(1.0f, 1.0f); // 공격 판정 박스 크기

    public PlayerLightningAttackState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();
        comboInputRegistered = false;
        dashTimer = dashDuration;
        bool isFacingRight = player.isFacingRight;
        // [요구사항 1, 2, 3] 입력 방향에 따른 고정값 설정
        float moveInput = player.inputReader.MoveValue.x;

        if (moveInput > 0 && isFacingRight)
        {
            facingDir = 1f; // 오른쪽을 보고 있고 오른쪽 입력
        }
        else if (moveInput < 0 && !isFacingRight)
        {
            facingDir = -1f; // 왼쪽을 보고 있고 왼쪽 입력
        }
        else
        {
            facingDir = 0f; // 반대 방향키 입력 혹은 중립(제자리)
        }

        player.rb.gravityScale = 0f;
        Debug.Log("라이트닝 컷 발사!");
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        // [요구사항 4] 적 감지 로직 (물리 적용 전 즉시 확인)
        if (facingDir != 0) // 움직일 때만 감지
        {
            Vector2 boxPos = (Vector2)player.transform.position + new Vector2(facingDir * hitRange, 0);
            Collider2D hitEnemy = Physics2D.OverlapBox(boxPos, hitSize, 0f, player.enemyLayer);

            if (hitEnemy != null && hitEnemy.CompareTag("Enemy"))
            {
                Debug.Log("적중! 돌진 중단!");
                dashTimer = 0; // 즉시 돌진 종료
                // 여기서 바로 공격 상태(예: player.AttackState)로 전이하고 싶다면
                // stateMachine.ChangeState(player.XXXState); 
                return;
            }
        }

        // --- 기존 비탈길 로직 유지 ---
        bool detectedFlatGround = false;
        RaycastHit2D[] hits = Physics2D.BoxCastAll(player.cd.bounds.center, player.cd.bounds.size * 0.9f, 0f, Vector2.down, 0.3f, player.GetCurrentGroundMask());

        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider != player.ignoredDropCollider && Vector2.Angle(Vector2.up, hit.normal) <= 0.1f)
            {
                detectedFlatGround = true;
                break;
            }
        }

        if (dashTimer > 0)
        {
            dashTimer -= Time.deltaTime;

            if (facingDir == 0)
            {
                player.SetVelocity(0f, 0f);
                return; // 아래 로직(비탈길 이동)을 타지 않게 함
            }
          

            if (player.OnSlope())
            {
                if (detectedFlatGround)
                {
                    player.SetVelocity(facingDir * dashSpeed, -0.1f);
                }
                else
                {
                    Vector2 dashDir = player.GetSlopeMoveDirection(new Vector2(facingDir, 0));
                    player.SetVelocity(dashDir.x * dashSpeed, dashDir.y * dashSpeed);
                }
            }
            else
            {
                float yVelocity = detectedFlatGround ? -0.05f : 0f;
                player.SetVelocity(facingDir * dashSpeed, yVelocity);
            }
        }
        else
        {
            player.SetVelocity(0f, 0f);
        }
    }


    
    public override void Exit()
    {
        base.Exit();
        player.SetVelocity(0f, 0f);
        player.rb.gravityScale = 1f;

    }
}