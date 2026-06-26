using UnityEngine;

public class DummyEnemy : EnemyFSM
{
    // EnemyFSM이 추상 클래스이므로 아래 5개 함수를 필수로 선언해야 에러가 안 납니다.
    // 허수아비이므로 안에는 아무것도 안 적어도 됩니다.
    protected override void OnIdle() { }
    protected override void OnChase() { }
    protected override void OnAttack() { }
    protected override void OnHit() { }
    protected override void OnDead() { }
}