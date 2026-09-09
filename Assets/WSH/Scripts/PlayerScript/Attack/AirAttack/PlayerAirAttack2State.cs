using UnityEngine;

public class PlayerAirAttack2State : PlayerState
{
    private bool isExitingState;

    public PlayerAirAttack2State(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();
        isExitingState = false;

        player.currentAirActionCount++; // 카운트 꽉 참 (더 이상 공격 불가)

        player.rb.gravityScale = 0f; // 2D gravityScale 사용

        // 2타는 1타보다 체공 반동을 살짝 줄이거나 다르게 세팅하면 더 찰집니다.
        float currentX = player.rb.linearVelocity.x;
        player.SetVelocity(currentX * 0.5f, player.airAttackBounceForce * 0.8f);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        float animLength = player.animator.GetCurrentAnimatorStateInfo(0).length;

        // 2타는 콤보 끝이므로 애니메이션 끝나면 무조건 떨어짐
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
            player.SetVelocity(0f, player.rb.linearVelocity.y);
        }
        //공중 브레이크: '스르륵'을 '통!'으로 바꿔주는 마법
        float currentX = player.rb.linearVelocity.x;
        float currentY = player.rb.linearVelocity.y;

        // Lerp를 이용해 현재 속도를 0으로 빠르게 깎아내립니다.
        // 뒤의 숫자(5f, 15f)가 클수록 브레이크가 강하게 걸립니다.
        float newX = Mathf.Lerp(currentX, 0f, Time.deltaTime * 5f);  // X축(앞으로 가는 힘) 제동
        float newY = Mathf.Lerp(currentY, 0f, Time.deltaTime * 10f); // ⭐️ Y축(위로 뜨는 힘) 급제동!

        player.SetVelocity(newX, newY);
    }

    public override void Exit()
    {
        base.Exit();
        player.rb.gravityScale = 1f; // 2D gravityScale 사용 (상태를 나갈 때 무조건 중력 다시 켜주기!)
    }

}