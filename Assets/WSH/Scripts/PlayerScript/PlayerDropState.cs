using System.Collections;
using System.Collections.Generic;
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
        player.ignoreSlopeDetection = true;
        player.slopeHit = default;
        dropTimer = 0.15f;

        Collider2D mainDropCol = player.GetDropThroughCollider();
        if (mainDropCol != null)
        {
            player.ignoredDropCollider = mainDropCol;

            Vector2 footPos = new Vector2(player.cd.bounds.center.x, player.cd.bounds.min.y);
            Vector2 checkSize = new Vector2(player.cd.bounds.size.x * 1.2f, 0.2f);
            Collider2D[] piercingCols = Physics2D.OverlapBoxAll(footPos, checkSize, 0f, player.groundLayer | player.stairsLayer);

            bool dropFromStairs = ((1 << mainDropCol.gameObject.layer) & player.stairsLayer) != 0;
            var pierceList = new List<Collider2D>();
            foreach (var col in piercingCols)
            {
                if (col == null || col == player.cd) continue;
                bool colIsStairs = ((1 << col.gameObject.layer) & player.stairsLayer) != 0;
                if (!dropFromStairs && colIsStairs) continue;
                pierceList.Add(col);
            }

            float dropSpeedX = player.rb.linearVelocity.x * 0.4f;
            player.SetVelocity(dropSpeedX, -4f);
            if (player.isSprinting)
            {
                player.animator.Play("Sprint-jump-falling");
            }
            player.StartCoroutine(PierceThroughRoutine(pierceList.ToArray()));
        }
        else
        {
            stateMachine.ChangeState(player.AirState);
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        dropTimer -= Time.deltaTime;
        if (dropTimer <= 0f)
        {
            stateMachine.ChangeState(player.AirState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.ignoreSlopeDetection = false;
    }

    private IEnumerator PierceThroughRoutine(Collider2D[] cols)
    {
        Collider2D playerCol = player.cd;

        foreach (var col in cols)
        {
            if (col != null && col != playerCol)
                Physics2D.IgnoreCollision(playerCol, col, true);
        }

        yield return new WaitForSeconds(0.2f);

        bool allCleared = false;
        while (!allCleared)
        {
            allCleared = true;
            foreach (var col in cols)
            {
                if (col != null && playerCol != null)
                {
                    if (Physics2D.Distance(playerCol, col).isOverlapped)
                    {
                        allCleared = false;
                    }
                    else
                    {
                        Physics2D.IgnoreCollision(playerCol, col, false);
                    }
                }
            }
            if (!allCleared) yield return null;
        }

        if (cols != null)
        {
            foreach (var col in cols)
            {
                if (player.ignoredDropCollider == col) player.ignoredDropCollider = null;
            }
        }
    }
}