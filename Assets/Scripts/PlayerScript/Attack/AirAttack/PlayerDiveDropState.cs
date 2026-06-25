using UnityEngine;
using System.Collections.Generic; // 💡 HashSet을 쓰기 위해 추가

public class PlayerDiveDropState : PlayerState
{
    // 💡 추가된 변수들: 타격 데이터와 맞은 적을 기억할 명부
    private AttackDataSO attackData;
    private HashSet<Collider2D> alreadyHitEnemies = new HashSet<Collider2D>();

    // 💡 생성자: PlayerController에서 상태를 만들 때 SO 데이터를 넘겨받도록 수정
    public PlayerDiveDropState(PlayerController player, PlayerStateMachine stateMachine, string animName, AttackDataSO data)
        : base(player, stateMachine, animName)
    {
        this.attackData = data;
    }

    public override void Enter()
    {
        base.Enter();

        // 💡 0. 낙하 시작! 명부 초기화 (이전에 맞았던 기록 삭제)
        alreadyHitEnemies.Clear();

        // 1. 중력 무시
        player.rb.gravityScale = 0f;

        // 2. 수직으로 최대 속도 꽂기
        player.SetVelocity(0f, -player.diveDropSpeed);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 바닥에 닿는 순간, 착지 타격 모션으로 
        if (player.IsGrounded())
        {
            stateMachine.ChangeState(player.DiveLandState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        // 제자리에서 안정적으로 떨어지도록 X축 고정
        player.SetVelocity(0f, player.rb.linearVelocity.y);

        // ========================================================
        // 추가된 부분: 낙하 중 타격 판정 로직 (매 물리 프레임마다 체크)
        // ========================================================
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            player.transform.position + (Vector3)attackData.offset,
            attackData.size,
            0f,
            player.enemyLayer
        );

        foreach (Collider2D hit in hits)
        {
            // 명부에 있으면(이미 맞았으면) 패스
            if (alreadyHitEnemies.Contains(hit)) continue;

            // hit.GetComponent<EnemyFSM>().TakeDamage(attackData.damage); 
            Debug.Log($"<color=orange>[타격]</color> <b>{attackData.attackName}</b> -> {hit.name}적중 ( 위치: {hit.transform.position})");
            // 명부에 이름 적기
            alreadyHitEnemies.Add(hit);

        }
    }

    public override void Exit()
    {
        base.Exit();
        player.rb.gravityScale = 1f; // 중력 복구
    }
}