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
        if (player.isSprinting)
        {
            float dir = player.isFacingRight ? 1f : -1f;
            float currentSpeed = player.sprintSpeed;

            if (player.OnSlope())
            {
                player.rb.gravityScale = 0f;
                Vector2 moveDir = new Vector2(dir, 0f);
                Vector2 slopeMoveDir = player.GetSlopeMoveDirection(moveDir);

                // 경사면 방향으로 전속력 이동
                player.rb.linearVelocity = slopeMoveDir * currentSpeed;

                // 🔥 [수정 2: 잃어버린 50f 접착제 부활!] 
                // 컨트롤러에서 꺼져버린 50f의 다운포스를 여기서 직접 꽂아버립니다.
                // 이제 구르는 내내 50f의 힘이 캐릭터 멱살을 잡고 내리막길에 찰싹 붙여줍니다!
                player.rb.AddForce(Vector2.down * 50f, ForceMode2D.Force);
            }
            else
            {
                player.rb.gravityScale = 1f;
                // 평지에서도 Y를 0으로 고정하지 않고 중력을 받게 둡니다.
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