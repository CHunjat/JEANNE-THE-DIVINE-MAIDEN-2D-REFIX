using System.Collections;
using UnityEngine;

public class PlayerDropState : PlayerState
{
    public PlayerDropState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();

        Collider2D dropCol = player.GetDropThroughCollider();

        if (dropCol != null)
        {
            player.ignoredDropCollider = dropCol;

            // ⚡ [핵심 수정] 0.15f는 너무 깊어 가까운 계단을 건너뛰게 만듭니다.
            // 딱 땅에서 발만 떨어지도록 0.02f만 내립니다.
            player.transform.position -= new Vector3(0f, 0.1f, 0f);

            // 약간의 하강 속도를 주어 중력이 부드럽게 바로 먹히도록 합니다.
            player.SetVelocity(player.rb.linearVelocity.x, -2f);

            player.StartCoroutine(DisableCollisionRoutine(dropCol));
        }

        // 즉시 AirState로 넘겨야 PlayerController의 계단 안착(isStairUnder) 로직이
        // 바로 다음 프레임부터 가까운 계단을 스캔하고 딱 잡아줍니다!
        stateMachine.ChangeState(player.AirState);
    }

    private IEnumerator DisableCollisionRoutine(Collider2D platformCol)
    {
        Collider2D playerCol = player.cd;

        // 1. 내가 방금 뚫고 내려가려는 '그 발판 하나'만 확실하게 충돌을 끕니다.
        Physics2D.IgnoreCollision(playerCol, platformCol, true);

        // 2. 타이머(WaitForSeconds) 삭제! 
        // 캐릭터가 그 발판에서 완전히 빠져나올 때까지 대기 (위로 튕김 완벽 방지)
        while (playerCol != null && platformCol != null && Physics2D.Distance(playerCol, platformCol).isOverlapped)
        {
            yield return null;
        }

        // 3. 완전히 빠져나왔을 때 복구
        if (playerCol != null && platformCol != null)
        {
            Physics2D.IgnoreCollision(playerCol, platformCol, false);
        }

        // 4. 완벽하게 빠져나온 후 널 초기화
        if (player.ignoredDropCollider == platformCol)
        {
            player.ignoredDropCollider = null;
        }
    }
}