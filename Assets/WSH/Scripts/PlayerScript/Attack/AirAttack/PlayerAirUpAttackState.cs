using UnityEngine;

public class PlayerAirUpAttackState : PlayerState
{
    private bool isExitingState; // 중복 탈출 방지용

    public PlayerAirUpAttackState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();
        isExitingState = false;

        // 1회 제한 잠금
        player.hasUsedAirUp = true;

        //  점프하던 그 힘(관성) 100% 보존!
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        float animLength = player.animator.GetCurrentAnimatorStateInfo(0).length;


        if (player.IsGrounded() && player.rb.linearVelocity.y <= 0.1f)
        {
            stateMachine.ChangeState(player.AirState); // 또는 LandState
            return;
        }

        // 애니메이션 끝나면 자연스럽게 추락 상태로 연결
        if (stateTimer >= animLength && !isExitingState)
        {
            isExitingState = true;
            stateMachine.ChangeState(player.AirState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        if (player.OnSlope())
        {
            player.rb.gravityScale = 0f; // 2D gravityScale 사용 (중력 끄기)
            player.SetVelocity(0f, 0f);   // 속도 완전 고정 (이동 공격이 아닐 경우)
        }
        else
        {
            // 평지라면 기존 중력/마찰력 로직 유지
            // player.SetVelocity(0f, player.rb.linearVelocity.y); 만약 x값 멈칫하게하고싶으면 이거 주석해제
        }
        // 핵심 2: 브레이크를 걸지 않고, AirState(일반 점프)와 완전히 똑같은 공중 이동 로직을 넣습니다.
        // 이렇게 하면 윗공격 중에도 점프 포물선이 유지되며, 방향키로 자연스러운 조작이 가능합니다.

        float xInput = player.inputReader.MoveValue.x;
        float currentX = player.rb.linearVelocity.x;

        // 대쉬 관성 감속 로직 (AirState와 동일)
        if (Mathf.Abs(currentX) > player.moveSpeed)
        {
            float targetX = xInput * player.moveSpeed;
            float lerpedX = Mathf.Lerp(currentX, targetX, Time.deltaTime * player.airDeceleration);
            player.SetVelocity(lerpedX, player.rb.linearVelocity.y);
        }
        else
        {
            // 일반 공중 이동 (점프 궤도 완벽 유지)
            player.SetVelocity(xInput * player.moveSpeed, player.rb.linearVelocity.y);
        }

        // 공격 중에도 방향 전환 허용 (필요 없으시면 이 줄 지워)
        if (xInput != 0) player.FlipController(xInput);
    }

    public override void Exit()
    {
        base.Exit();
        player.rb.gravityScale = 1f; // 2D gravityScale 사용 (상태 나갈 때 중력 원복 필수!)
    }
}