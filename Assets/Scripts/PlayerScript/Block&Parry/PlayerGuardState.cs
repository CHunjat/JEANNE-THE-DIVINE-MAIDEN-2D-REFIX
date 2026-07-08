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

        // 1. 패리 상태 탈출을 위한 타이머 강제 리셋
        parryStartTime = Time.time;

        // 2. [진짜 핵심] 1타 패링 성공 시, 패링 유효 시간을 갱신해 줍니다!
        // 이걸 해줘야 2타, 3타가 연속으로 들어와도 쿨하게 연속 패링(챙- 챙- 챙-)이 터집니다.
        player.guardStartTime = Time.time;

        // 3. 애니메이션 강제 재시작 (같은 상태라도 무조건 0프레임부터)
        // -1을 넣으면 현재 레이어 전체를 덮어씌우고 0프레임부터 강제로 틀어버립니다.
        player.animator.Play(player.anim_GuardParry, -1, 0f);

        // 4. 다음 프레임까지 안 기다리고 지금 당장! 즉시 갱신
        player.animator.Update(0f);
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