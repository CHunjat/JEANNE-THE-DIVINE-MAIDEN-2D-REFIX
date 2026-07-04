using UnityEngine;

public class PlayerGuardState : PlayerState
{
    public bool isParrying = false;
    private float knockbackTimer = 0f; // 넉백 보호 타이머

    public PlayerGuardState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();
        player.rb.linearVelocity = new Vector2(0f, player.rb.linearVelocity.y);

        // 가드 진입 시 강제 초기화
        player.animator.CrossFade(player.anim_GuardNormal, 0f, 0);
        player.guardStartTime = Time.time;
        isParrying = false;
        knockbackTimer = 0f;
    }

    // [중요] 외부(EvaluateAttack)에서 넉백 발생 시 호출
    public void SetKnockbackLock(float duration)
    {
        knockbackTimer = duration;
    }

    public void TriggerParryAnimation()
    {
        isParrying = true;
        player.animator.Play(player.anim_GuardParry, 0, 0f);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 1. 가드 키 해제 시
        if (!player.inputReader.GuardHeld)
        {
            stateMachine.ChangeState(player.GuardOffState);
            return;
        }

        AnimatorStateInfo stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);

        // 2. 패리 중 처리
        if (isParrying)
        {
            // 카운터 입력
            if (player.inputReader.AttackPressed)
            {
                player.inputReader.AttackPressed = false;
                stateMachine.ChangeState(player.ParryLightCounterState);
                return;
            }
            if (player.inputReader.ThrustAttackPressed)
            {
                player.inputReader.ThrustAttackPressed = false;
                stateMachine.ChangeState(player.ParryHeavyCounterState);
                return;
            }

            // 패리 종료 시 자동 복귀
            if (stateInfo.IsName(player.anim_GuardParry) && stateInfo.normalizedTime >= 0.95f)
            {
                isParrying = false;
                player.animator.Play(player.anim_GuardNormal);
            }
            return;
        }

        // 3. 일반 가드 피격 후 복귀
        if (stateInfo.IsName(player.anim_BlockHit) && stateInfo.normalizedTime >= 1.0f)
        {
            player.animator.Play(player.anim_GuardNormal);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        // 넉백 보호 타이머가 작동 중이면 슬라이딩 감속 로직을 건너뜀 (넉백 보존)
        if (knockbackTimer > 0)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            return;
        }

        // 넉백 타이머가 끝났을 때만 스르륵 멈추는 로직 적용
        float slideDecay = Mathf.Lerp(player.rb.linearVelocity.x, 0f, Time.fixedDeltaTime * 10f);
        player.SetVelocity(slideDecay, player.rb.linearVelocity.y);
    }
}