//using UnityEngine;

//public class PlayerAirDownAttackState : PlayerState
//{
//    private bool isExitingState;

//    public PlayerAirDownAttackState(PlayerController player, PlayerStateMachine stateMachine, string animName)
//        : base(player, stateMachine, animName) { }

//    public override void Enter()
//    {
//        base.Enter();
//        isExitingState = false;
//        player.currentAirActionCount++;

//        player.rb.useGravity = false;

//        // 아랫방향 공격은 일반 공중 평타와 똑같이 위로 살짝 튀어오름
//        float currentX = player.rb.linearVelocity.x;
//        player.SetVelocity(currentX * 0.5f, player.airAttackBounceForce);
//    }

//    public override void LogicUpdate()
//    {
//        base.LogicUpdate();

//        float animLength = player.animator.GetCurrentAnimatorStateInfo(0).length;

//        if (stateTimer >= animLength && !isExitingState)
//        {
//            isExitingState = true;
//            stateMachine.ChangeState(player.AirState);
//        }
//    }

//    public override void PhysicsUpdate()
//    {
//        base.PhysicsUpdate();

//        // 찰진 타격감을 위한 공중 브레이크
//        float currentX = player.rb.linearVelocity.x;
//        float currentY = player.rb.linearVelocity.y;

//        float newX = Mathf.Lerp(currentX, 0f, Time.deltaTime * 5f);
//        float newY = Mathf.Lerp(currentY, 0f, Time.deltaTime * 15f);

//        player.SetVelocity(newX, newY);
//    }

//    public override void Exit()
//    {
//        base.Exit();
//        player.rb.useGravity = true;
//    }
//}