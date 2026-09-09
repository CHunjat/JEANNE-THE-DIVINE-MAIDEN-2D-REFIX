using UnityEngine;

public class PlayerHeavyChargeState : PlayerState
{
    private bool isFullyCharged;
    private bool isLoopAnimStarted;

    public PlayerHeavyChargeState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();
        player.SetVelocity(0f, player.rb.linearVelocity.y);

        isFullyCharged = false;
        isLoopAnimStarted = false;

        // ★ 진입 시 무조건 1단계로 세팅
        player.currentChargeLevel = 1;

        // 시작 시 준비 애니메이션 강제 재생
        player.animator.Play("ToCharge", 0, 0f);
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        if (player.OnSlope())
        {
            player.rb.gravityScale = 0f; // 2D gravityScale 사용 (중력 끄기)
            player.SetVelocity(0f, 0f);  // 속도 완전 고정 (이동 공격이 아닐 경우)
        }
        else
        {
            // 평지라면 기존 중력/마찰력 로직 유지
            player.SetVelocity(0f, player.rb.linearVelocity.y);
        }
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

        // 2. 풀차지 도달 체크
        // 지정한 시간이 지나면 2단계로 승급시킴
        if (stateTimer >= player.maxChargeTime && !isFullyCharged)
        {
            isFullyCharged = true;
            player.currentChargeLevel = 2; // ★ 풀차지 달성 시 2단계로 변환

            Debug.Log("풀차지 완료! 번쩍! (2단계)");
            // TODO: 여기서 시각적 이펙트(반짝임)나 효과음을 넣어주면 아주 좋습니다.
        }

        // 3. 입력 판정: 유저가 손을 떼는 순간 공격 상태로 전환
        // (0.5초 전에 떼면 1단계인 상태로 넘어가고, 0.5초 후에 떼면 2단계로 넘어감)
        if (!player.inputReader.HeavyAttackHeld)
        {
            stateMachine.ChangeState(player.HeavyAttackState);
            return; // 상태가 바뀌었으므로 아래 로직 실행 방지
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.rb.gravityScale = 1f; // 2D gravityScale 사용 (상태 나갈 때 중력 원복 필수!)
    }
}