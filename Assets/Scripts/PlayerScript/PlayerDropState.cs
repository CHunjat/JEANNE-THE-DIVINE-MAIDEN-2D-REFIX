using System.Collections;
using UnityEngine;

public class PlayerDropState : PlayerState
{
    public PlayerDropState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();

        // 1. 통과할 발판 콜라이더 가져오기
        Collider2D dropCol = player.GetDropThroughCollider();

        if (dropCol != null)
        {
            // 2. 약간의 위치 보정 (나인솔즈 특유의 '쑥' 빠지는 느낌)
            player.transform.position -= new Vector3(0f, 0.3f, 0f);

            // 3. 바닥 센서 투시용 콜라이더 등록
            player.ignoredDropCollider = dropCol;

            // 4. 충돌 무시 코루틴 실행
            player.StartCoroutine(DisableCollisionRoutine(dropCol));
        }

        // 볼일 끝났으니 바로 공중 상태(AirState)로 전환해서 낙하 물리 적용!
        stateMachine.ChangeState(player.AirState);
    }

    // 발판과의 충돌을 껐다가, "완전히 빠져나왔을 때만" 다시 켜주는 코루틴
    private IEnumerator DisableCollisionRoutine(Collider2D platformCol)
    {
        Collider2D playerCol = player.GetComponent<Collider2D>();

        // 1. 충돌 끄기
        Physics2D.IgnoreCollision(playerCol, platformCol, true);

        // 2. 떨어지기 시작하도록 아주 잠깐(0.1초) 대기
        yield return new WaitForSeconds(0.1f);

        // 3. ★ [밀려남 방지 핵심] 타임아웃 완전 삭제! (무한 존버)
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