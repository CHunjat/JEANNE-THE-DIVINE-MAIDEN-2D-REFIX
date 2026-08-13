using UnityEngine;

public class PlayerThrustReadyState : PlayerState
{
    //fx효과및 차지타임
    private float maxChargeTime = 0.6f;
    private GameObject currentChargeFX;


    public PlayerThrustReadyState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();
        // 3타 진입처럼 아예 (0,0)으로 못 박고 시작
        player.rb.gravityScale = 0f; // 2D gravityScale 사용
        player.SetVelocity(0f, 0f);
        if (player.thrustChargeFxPrefab != null && player.thrustChargeFxSpawnPoint != null)
        {
            
            currentChargeFX = Object.Instantiate(
                player.thrustChargeFxPrefab,
                player.thrustChargeFxSpawnPoint.position, // 지정해둔 오른팔 위치
                Quaternion.identity,
                player.transform // 플레이어를 부모로 설정해서 따라다니게 함
            );

            // 플레이어가 왼쪽을 보면 FX도 좌우 반전
            if (!player.isFacingRight)
            {
                currentChargeFX.transform.localScale = new Vector3(-1, 1, 1);
            }
        }



    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 업데이트에서 y값을 건드리면 비탈길 쐐기(0,0)가 풀립니다. 삭제하세요.

        if (stateTimer >= maxChargeTime)
        {
            player.isThrustCharged = true; // 🌟 풀 차지 깃발 ON!
            stateMachine.ChangeState(player.ThrustAttackState);
            return;
        }

        // 키를 떼면 바로 공격!
        if (!player.inputReader.ThrustAttackHeld)
        {
            player.isThrustCharged = false;
            stateMachine.ChangeState(player.ThrustAttackState);
            return;
        }

        if (!player.IsGrounded())
        {
            stateMachine.ChangeState(player.AirState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        if (player.OnSlope())
        {
            player.rb.gravityScale = 0f; // 2D gravityScale 사용
            player.SetVelocity(0f, 0f); // 여기서만 물리적으로 꽉 잡습니다.
        }
        else
        {
            player.rb.gravityScale = 1f; // 2D gravityScale 사용
            player.SetVelocity(0f, player.rb.linearVelocity.y);
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.rb.gravityScale = 1f; // 2D gravityScale 사용

        if (currentChargeFX != null)
        {
            Object.Destroy(currentChargeFX);
        }
    }
}