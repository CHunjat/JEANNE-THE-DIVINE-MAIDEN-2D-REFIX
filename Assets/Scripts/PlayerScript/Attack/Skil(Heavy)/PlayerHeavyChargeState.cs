using UnityEngine;

public class PlayerHeavyChargeState : PlayerState
{
    private bool isFullyCharged;
    private float fullChargeTime = 1.2f;
    private bool isLoopAnimStarted;

    public PlayerHeavyChargeState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();
        player.SetVelocity(0f, player.rb.linearVelocity.y);

        isFullyCharged = false;
        isLoopAnimStarted = false;

        // 시작 시 준비 애니메이션 강제 재생

        player.animator.Play("ToCharge", 0, 0f);
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
        player.SetVelocity(0f, player.rb.linearVelocity.y);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 1. 애니메이션 전환: ToCharge -> Charging (루프)
        // GetNormalizedTime이 1.0(끝)에 도달하면 루프 애니메이션으로 교체
        if (!isLoopAnimStarted && GetNormalizedTime() >= 1.0f)
        {
            isLoopAnimStarted = true;
            player.animator.Play("Charging");
        }

        // 2. 입력 판정: 유저가 손을 떼는 순간 공격 상태로 전환
        if (!player.inputReader.HeavyAttackHeld)
        {
            if (isFullyCharged)
            {
                stateMachine.ChangeState(player.HeavyAttackState);
            }
            else
            {
                stateMachine.ChangeState(player.Attack2State);
                Debug.Log("차지 실패: 일반 강공격 나감");
            }
            return; // 상태가 바뀌었으므로 아래 로직 실행 방지
        }

        // 3. 풀차지 도달 체크
        if (stateTimer >= fullChargeTime && !isFullyCharged)
        {
            isFullyCharged = true;
            Debug.Log("풀차지 완료! 번쩍!");
            // 여기서 시각적 이펙트(반짝임)를 넣어주면 아주 좋습니다.
        }
    }
    public override void Exit()
    {
        base.Exit();
        player.rb.gravityScale = 1f; // 2D gravityScale 사용 (상태 나갈 때 중력 원복 필수!)
    }
}