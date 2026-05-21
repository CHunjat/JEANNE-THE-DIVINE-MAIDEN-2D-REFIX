using UnityEngine;

public class PlayerThrustAttackState : PlayerAttackState
{
    // 🔥 3타의 stepSpeed(4f)를 참고하여 찌르기 맛에 맞게 상수화
    private float thrustSpeed = 6f;
    // 🔥 너무 멀리 가는 걸 방지하기 위해 전진 시간을 3타(0.15f)와 비슷하게 조절
    private float activeThrustTime = 0.2f;

    public PlayerThrustAttackState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();
        player.animator.Play(player.anim_ThrustAtk, 0, 0f);

        // 3타와 동일: 진입 시 중력 끄고 정지 쐐기
        player.rb.gravityScale = 0f; // 2D gravityScale 사용
        player.SetVelocity(0f, 0f);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        // 애니메이션이 완전히 끝날 때까지 상태를 유지해야 PhysicsUpdate의 쐐기가 작동함
        if (GetNormalizedTime() >= 1.0f)
        {
            stateMachine.ChangeState(player.IdleState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        float facingDir = player.isFacingRight ? 1f : -1f;

        // 🔥 [3타 핵심 로직 1] 독립적 비탈길 앵커
        // PhysicsUpdate 시작 시 매 프레임 속도를 리셋하여 내리막 가속도를 삭제합니다.
        if (player.OnSlope())
        {
            player.rb.gravityScale = 0f; // 2D gravityScale 사용
            player.SetVelocity(0f, 0f);
        }

        // 🔥 [3타 핵심 로직 2] 상수 기반 이동 구간
        // 커브를 쓰지 않고 정해진 시간(activeThrustTime) 동안만 상수로 밀어줍니다.
        if (stateTimer < activeThrustTime)
        {
            if (player.OnSlope())
            {
                Vector2 moveVec = new Vector2(facingDir, 0f); // Vector2로 전환
                Vector2 slopeVec = player.GetSlopeMoveDirection(moveVec);

                // 3타에서 검증된 자석 수치 (2.0f, -2f)
                float downwardStickiness = slopeVec.y < 0 ? 2.0f : 1.0f;
                player.SetVelocity(
                    slopeVec.x * thrustSpeed,
                    (slopeVec.y * thrustSpeed * downwardStickiness) - (slopeVec.y < 0 ? 2f : 0f)
                );
            }
            else
            {
                // 평지 상속 전진 (중력은 끄고 수평 속도만 깔끔하게 주입)
                player.SetVelocity(facingDir * thrustSpeed, 0f);
            }
        }
        else
        {
            // 🔥 [3타 핵심 로직 3] 전진 종료 후 0,0 쐐기
            // 시간이 다 되면 애니메이션이 끝날 때까지 캐릭터를 땅에 박아버립니다.
            if (player.OnSlope()) player.SetVelocity(0f, 0f);
            else player.SetVelocity(0f, 0f); // 평지도 관성 없이 0,0 고정
        }
    }

    public override void Exit()
    {
        base.Exit();
        // 상태 탈출 시 물리 엔진에 남아있을지 모를 모든 속도 제거
        player.rb.linearVelocity = Vector2.zero; // Vector2로 전환
        player.rb.gravityScale = 1f; // 2D gravityScale 복구
    }
}