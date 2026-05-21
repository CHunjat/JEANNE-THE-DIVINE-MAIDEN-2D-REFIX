using UnityEngine;

public class PlayerAirAttack1State : PlayerState
{
    private bool isExitingState; // 중복 탈출 방지용

    public PlayerAirAttack1State(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();
        isExitingState = false;

        // 1. 공격 횟수 1개 차감
        player.currentAirActionCount++;

        // 2. 🛑 2D 중력 끄기 (체공)
        player.rb.gravityScale = 0f;

        // 3. 🦅 허공답보: 진행하던 X축 관성은 절반으로 줄이고, Y축은 살짝 튕겨 올림
        float currentX = player.rb.linearVelocity.x;
        player.SetVelocity(currentX * 0.5f, player.airAttackBounceForce);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        float nTime = GetNormalizedTime();
        // 현재 애니메이션 실제 길이 가져오기
        float animLength = player.animator.GetCurrentAnimatorStateInfo(0).length;

        // ⚔️ 2타 콤보 예약: 카운트가 남았고, 모션이 50% 이상 진행됐을 때 공격 키를 누르면
        if (player.inputReader.AttackPressed && player.currentAirActionCount < player.maxAirActions && nTime >= 0.5f)
        {
            isExitingState = true;
            stateMachine.ChangeState(player.AirAttack2State);
            return;
        }

        // 🚪 애니메이션 다 끝나면 다시 떨어지기 시작
        if (stateTimer >= animLength && !isExitingState)
        {
            isExitingState = true;
            stateMachine.ChangeState(player.AirState); // Fall 모션으로 돌아감
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        if (player.OnSlope())
        {
            player.rb.gravityScale = 0f; // 중력 끄기
            player.SetVelocity(0f, 0f);   // 속도 완전 고정 (이동 공격이 아닐 경우)
        }
        else
        {
            // 평지라면 기존 중력/마찰력 로직 유지
            player.SetVelocity(0f, player.rb.linearVelocity.y);
        }

        // 🔥 공중 브레이크: '스르륵'을 '통!'으로 바꿔주는 마법
        float currentX = player.rb.linearVelocity.x;
        float currentY = player.rb.linearVelocity.y;

        // Lerp를 이용해 현재 속도를 0으로 빠르게 깎아내립니다.
        // 뒤의 숫자(5f, 15f)가 클수록 브레이크가 걸립니다.
        float newX = Mathf.Lerp(currentX, 0f, Time.deltaTime * 5f);  // X축(앞으로 가는 힘) 제동
        float newY = Mathf.Lerp(currentY, 0f, Time.deltaTime * 10f); // ⭐️ Y축(위로 뜨는 힘) 급제동!

        player.SetVelocity(newX, newY);
    }

    public override void Exit()
    {
        base.Exit();
        // 상태를 나갈 때 무조건 중력 다시 켜주기! (안 그러면 우주로 날아감)
        player.rb.gravityScale = 1f;
    }
}