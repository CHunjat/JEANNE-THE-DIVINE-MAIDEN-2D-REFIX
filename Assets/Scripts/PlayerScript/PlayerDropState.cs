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
            player.ignoreSlopeDetection = true; //비탈길 자석 끄기! (허공 툭 걸림 방지)
            // ⚡ [핵심  0.15f는 너무 깊어 가까운 계단을 건너뛰게 만듭니다.
            // 딱 땅에서 발만 떨어지도록 0.02f만 내립니다.
            player.transform.position -= new Vector3(0f, 0.1f, 0f);

            // 약간의 하강 속도를 주어 중력이 부드럽게 바로 먹히도록 합니다.
            player.SetVelocity(player.rb.linearVelocity.x, -2f);

            player.StartCoroutine(DisableSlopeAndCollision(dropCol));
        }

        // 즉시 AirState로 넘겨야 PlayerController의 계단 안착(isStairUnder) 로직이
        // 바로 다음 프레임부터 가까운 계단을 스캔하고 딱 잡아줍니다!
        stateMachine.ChangeState(player.AirState);
    }

    private IEnumerator DisableSlopeAndCollision(Collider2D platformCol)
    {
        Collider2D playerCol = player.cd;
        Physics2D.IgnoreCollision(playerCol, platformCol, true);

        // 0.2초 정도는 밑점프 상태이므로 비탈길 자석을 끕니다.
        // 0.2초면 캐릭터가 충분히 발판을 뚫고 내려가서 툭 걸릴 일이 없습니다.
        yield return new WaitForSeconds(0.2f);

        player.ignoreSlopeDetection = false;

        // 발판 빠져나올 때까지 기다림
        while (playerCol != null && platformCol != null && Physics2D.Distance(playerCol, platformCol).isOverlapped)
        {
            yield return null;
        }

        Physics2D.IgnoreCollision(playerCol, platformCol, false);
        if (player.ignoredDropCollider == platformCol) player.ignoredDropCollider = null;
    }
}