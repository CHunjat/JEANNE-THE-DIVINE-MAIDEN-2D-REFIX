using System.Collections;
using UnityEngine;

public class PlayerDropState : PlayerState
{
    private float dropTimer;

    public PlayerDropState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();
        player.lastGroundedWasSlope = false;
        player.ignoreSlopeDetection = true; // 비탈길 자석 강제 종료
        player.slopeHit = default;
        dropTimer = 0.15f; // 유령 상태 유지 시간

        // 1. 기존의 안전장치(좁은 공간 밑점프 금지 등)를 살리기 위해 순정 함수 그대로 호출
        Collider2D mainDropCol = player.GetDropThroughCollider();

        if (mainDropCol != null)
        {
            player.ignoredDropCollider = mainDropCol; // 에러 없는 기존 변수 사용

            // 이 State 안에서만 자체적으로 발밑에 겹친 '모든 바닥(평지+비탈길)'을 찾아냅니다.
            Vector2 footPos = new Vector2(player.cd.bounds.center.x, player.cd.bounds.min.y);
            Vector2 checkSize = new Vector2(player.cd.bounds.size.x * 1.2f, 0.2f);
            Collider2D[] piercingCols = Physics2D.OverlapBoxAll(footPos, checkSize, 0f, player.groundLayer | player.stairsLayer);

           
            // 스프린트 속도 때문에 앞으로 날아가서 비탈길에 처박히는 걸 막기 위해 X 속도를 60%로 줄이고,
            // 억지로 내리꽂지 않는 스무스한 하강 속도(-5f)를 줍니다.
            float dropSpeedX = player.rb.linearVelocity.x * 0.4f;
            player.SetVelocity(dropSpeedX, -4f);

            if (player.isSprinting)
            {
                player.animator.Play("Sprint-jump-falling");
            }


            // 겹쳐있는 바닥들을 한 방에 모조리 뚫어버리는 코루틴 발사
            player.StartCoroutine(PierceThroughRoutine(piercingCols));
        }
        else
        {
            // 좁은 공간이라 차단된 경우 바로 공중 상태로
            stateMachine.ChangeState(player.AirState);
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        dropTimer -= Time.deltaTime;

        // 시간이 지나면 자연스럽게 공중 상태로 전환 (착지 레이더 가동 시작)
        if (dropTimer <= 0f)
        {
            stateMachine.ChangeState(player.AirState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        // 비탈길 자석 기능 정상 복구
        player.ignoreSlopeDetection = false;
    }

    private IEnumerator PierceThroughRoutine(Collider2D[] cols)
    {
        Collider2D playerCol = player.cd;

        // 1. 발밑에 있던 모든 겹친 바닥들의 물리 충돌을 일시 정지 (관통 시작)
        foreach (var col in cols)
        {
            if (col != null && col != playerCol)
                Physics2D.IgnoreCollision(playerCol, col, true);
        }

        // 충분히 통과할 시간 대기
        yield return new WaitForSeconds(0.2f);

        // 2. 바닥을 완전히 빠져나왔는지 하나씩 검사 후 복구
        bool allCleared = false;
        while (!allCleared)
        {
            allCleared = true;
            foreach (var col in cols)
            {
                if (col != null && playerCol != null)
                {
                    // 아직 캐릭터 몸에 닿아있는 바닥이 있다면 더 기다림
                    if (Physics2D.Distance(playerCol, col).isOverlapped)
                    {
                        allCleared = false;
                    }
                    else
                    {
                        // 몸을 완전히 빠져나온 바닥은 즉시 충돌 복구
                        Physics2D.IgnoreCollision(playerCol, col, false);
                    }
                }
            }
            if (!allCleared) yield return null;
        }

        // 혹시 모를 기존 변수 초기화
        if (cols != null)
        {
            foreach (var col in cols)
            {
                if (player.ignoredDropCollider == col) player.ignoredDropCollider = null;
            }
        }
    }
}