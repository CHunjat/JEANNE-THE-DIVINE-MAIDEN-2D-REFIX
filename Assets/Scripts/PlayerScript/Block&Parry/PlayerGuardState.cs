using UnityEngine;

public class PlayerGuardState : PlayerState
{
    public bool isParrying = false;
    private float knockbackTimer = 0f; // 넉백 보호 타이머
    private float parryStartTime = 0f;

    private float originalGravity;
    private RigidbodyConstraints2D originalConstraints; // [추가] 원래 제약조건 백업용

    public PlayerGuardState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();
        player.rb.linearVelocity = new Vector2(0f, player.rb.linearVelocity.y);
        originalGravity = player.rb.gravityScale; //원ㄹㅐ 중력값 담아두기

        originalConstraints = player.rb.constraints; // [추가] 기본 제약조건(회전방지 등) 담아두기

        // 가드 진입 시 강제 초기화
        player.animator.CrossFade(player.anim_GuardNormal, 0f, 0);
        player.guardStartTime = Time.time;
        isParrying = false;
        knockbackTimer = 0f;
        parryStartTime = 0f;
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
        AnimatorStateInfo stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);

        // ==========================================
        // [1. 패리 구역] 무조건 여기부터 검사해야 함!
        // ==========================================
        if (isParrying)
        {
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

            if (Time.time - parryStartTime > 0.1f)
            {
                if (stateInfo.IsName(player.anim_GuardParry) && stateInfo.normalizedTime >= 0.95f)
                {
                    isParrying = false;

                    if (player.inputReader.GuardHeld)
                    {
                        player.animator.Play(player.anim_GuardNormal);
                    }
                    else
                    {
                        stateMachine.ChangeState(player.GuardOffState);
                    }
                }
            }

            // 패리 중일 때는 여기서 강제로 끝냄. 
            return;
        }

        // 패리가 아닐 때만 밑코드

        // 일반 가드일 때만 S키 떼면 가드 해제!
        if (!player.inputReader.GuardHeld)
        {
            stateMachine.ChangeState(player.GuardOffState);
            return;
        }

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
            player.rb.constraints = originalConstraints; // [추가] 넉백될 땐 락 풀기
            return;
        }

        if (player.OnSlope())
        {
            // [비탈길] 속도 0 + 중력 0 = 본드 칠한 듯이 절대 안 미끄러짐!
            player.SetVelocity(0f, 0f);
            player.rb.gravityScale = 0f;
            // [추가] 물리엔진 멱살 잡고 위치 락 걸어버리기
            player.rb.constraints = originalConstraints | RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;
        }
        else
        {
            // [평지] 중력 복구시키고 부드럽게 감속
            player.rb.gravityScale = originalGravity;
            player.rb.constraints = originalConstraints; // [추가] 평지면 락 풀기

            float slideDecay = Mathf.Lerp(player.rb.linearVelocity.x, 0f, Time.fixedDeltaTime * 10f);
            player.SetVelocity(slideDecay, player.rb.linearVelocity.y);
        }
    }

    public override void Exit()
    {
        base.Exit();
        // [중요!] 가드를 풀고 나갈 때는 무조건 중력을 원래대로 돌려놔야 공중에 안 뜹니다!
        player.rb.gravityScale = originalGravity;
        player.rb.constraints = originalConstraints; // [추가] 나갈 때 락 풀기
    }
}