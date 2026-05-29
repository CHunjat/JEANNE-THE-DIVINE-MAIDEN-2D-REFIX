using System.Collections;
using UnityEngine;

public class PlayerDropState : PlayerState
{
    public PlayerDropState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();

        // 1. 발판 찾기
        Collider2D dropCol = player.GetDropThroughCollider();

        // 2. [핵심] 내 머리 위에 닿아있는 콜라이더 찾기 (이게 윗천장)
        // OverlapBox로 내 캐릭터 범위 바로 위를 체크해서 Ground 레이어인 놈을 찾음
        Collider2D ceilingCol = Physics2D.OverlapBox(player.cd.bounds.center + Vector3.up * 0.2f, player.cd.bounds.size * 0.8f, 0f, LayerMask.GetMask("Ground"));

        if (dropCol != null)
        {
            player.ignoredDropCollider = dropCol;
            player.transform.position -= new Vector3(0f, 0.5f, 0f);

            // 3. 발판과 윗천장을 동시에 무시하는 코루틴 호출
            player.StartCoroutine(DisableCollisionRoutine(dropCol, ceilingCol));
        }
        stateMachine.ChangeState(player.AirState);
    }

    private IEnumerator DisableCollisionRoutine(Collider2D platformCol, Collider2D ceilingCol)
    {
        // 발판 무시
        Physics2D.IgnoreCollision(player.cd, platformCol, true);

        // [중요] 머리에 닿은 윗천장(Ground)도 같이 무시!
        if (ceilingCol != null) Physics2D.IgnoreCollision(player.cd, ceilingCol, true);

        // 0.3초간 무조건 통과 유지
        yield return new WaitForSeconds(0.4f);

        // 완전히 빠져나올 때까지 대기
        while (player.cd != null && platformCol != null && platformCol.IsTouching(player.cd))
        {
            yield return null;
        }

        // 복구 (둘 다)
        Physics2D.IgnoreCollision(player.cd, platformCol, false);
        if (ceilingCol != null) Physics2D.IgnoreCollision(player.cd, ceilingCol, false);

        player.ignoredDropCollider = null;
    }

    // 발판과의 충돌을 껐다가, "완전히 빠져나왔을 때만" 다시 켜주는 코루틴
    private IEnumerator DisableCollisionRoutine(Collider2D platformCol)
    {
        Collider2D playerCol = player.GetComponent<Collider2D>();

        // 1. 충돌 끄기
        Physics2D.IgnoreCollision(playerCol, platformCol, true);

        // 2. 떨어지기 시작하도록 아주 잠깐(0.1초) 대기
        yield return new WaitForSeconds(0.3f);

       
        // 두 콜라이더 사이의 실제 기하학적 거리를 계산하여, 
        // 머리나 몸통이 단 1픽셀이라도 겹쳐(Overlapped) 있다면 영원히 대기합니다.
        while (playerCol != null && platformCol != null && Physics2D.Distance(playerCol, platformCol).isOverlapped)
        {
            yield return null; // 다음 프레임까지 대기
        }

        // 4. 머리가 계단 밖으로 '완벽하게' 빠져나온 그 순간에만 충돌 복구! 
        // (가만히 서 있으면 여기서 계속 멈춰있기 때문에 절대 위로 안 튕겨 올라갑니다)
        if (playerCol != null && platformCol != null)
        {
            Physics2D.IgnoreCollision(playerCol, platformCol, false);
        }

        // 5. 투시 모드 해제
        player.ignoredDropCollider = null;
    }
}