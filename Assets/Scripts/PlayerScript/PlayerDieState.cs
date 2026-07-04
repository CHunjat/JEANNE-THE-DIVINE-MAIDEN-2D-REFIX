using UnityEngine;

public class PlayerDieState : PlayerState
{
    private bool isGameOverTriggered;
    private string groundAnimName;
    private string airAnimName;
    private bool diedInAir;

    public PlayerDieState(PlayerController player, PlayerStateMachine stateMachine, string groundAnimName, string airAnimName)
        : base(player, stateMachine, groundAnimName)
    {
        this.groundAnimName = groundAnimName;
        this.airAnimName = airAnimName;
    }

    public override void Enter()
    {
        stateTimer = 0f;
        isGameOverTriggered = false;

        player.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        diedInAir = !player.IsGrounded() && !player.OnSlope();

        // 디버깅용: 이름이 제대로 들어오는지 확인
        Debug.Log($"사망 상태 진입 - 지상: {groundAnimName}, 공중: {airAnimName}");

        if (diedInAir)
        {
            player.SetVelocity(0f, player.rb.linearVelocity.y);
            // 문자열 그대로 호출
            player.animator.Play(airAnimName, 0, 0f);
        }
        else
        {
            player.SetVelocity(0f, 0f);
            player.rb.gravityScale = 0f;
            // 문자열 그대로 호출
            player.animator.Play(groundAnimName, 0, 0f);

        }

        player.animator.Update(0f);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (diedInAir)
        {
            float nTime = player.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

            // 1. 0~4프레임(약 0.3) 도달 안 했으면? 
            // 그냥 냅둬야 함. (가만히 있으면 자연스럽게 0->4로 진행됨)
            if (nTime < 0.3f) return;

            // 2. 5~7프레임(0.3~0.45) 구간 & 땅에 안 닿았을 때
            // -> 이 구간에서만 루프 반복
            if (!player.IsGrounded())
            {
                if (nTime >= 0.46f) 
                {
                    player.animator.Play(airAnimName, 0, 0.31f); // 5프레임(0.3)으로 복귀
                }
            }
            // 3. 땅에 닿으면?
            else if (player.IsGrounded() && player.rb.linearVelocity.y <= 0.1f)
            {
                diedInAir = false; // 이제 루프 로직 종료
                player.SetVelocity(0f, 0f);
                player.rb.gravityScale = 0f;
            }
        }

        if (stateTimer >= 2.5f && !isGameOverTriggered)
        {
            isGameOverTriggered = true;
            Debug.Log("<color=red>[사망] 게임 오버 로직 호출</color>");
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.rb.gravityScale = 1f;
        player.gameObject.layer = LayerMask.NameToLayer("Player");
    }
}