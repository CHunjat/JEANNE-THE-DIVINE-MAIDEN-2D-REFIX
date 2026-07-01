using UnityEngine;

public class DummyEnemy : EnemyFSM
{
    [Header("허수아비 애니메이션 설정")]
    [SerializeField] private string hitAnimName = "Hit";   // 대소문자 주의!
    [SerializeField] private string idleAnimName = "Idle"; // 대소문자 주의!

    private bool isHitAnimationStarted = false;

    // 💥 플레이어한테 맞았을 때 무조건 실행되는 곳
    public override void TakeDamage(float amount)
    {
        if (GetCurrentState() == EnemyState.Dead) return;

        base.TakeDamage(amount); // 체력 깎기

        // 살아있다면 맞았을 때 무조건 Hit 상태로 강제 변환!
        if (currentHp > 0)
        {
            isHitAnimationStarted = false;
            ChangeState(EnemyState.Hit);
        }
    }
     
    protected override void OnIdle()
    {
        isHitAnimationStarted = false;

        // 1장짜리 Idle 애니메이션 틀어주기
        if (animator != null && !animator.GetCurrentAnimatorStateInfo(0).IsName(idleAnimName))
        {
            animator.Play(idleAnimName, 0, 0f);
        }
    }

    protected override void OnHit()
    {
        if (animator == null)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        // 1. 처음 Hit 상태가 되면 피격 애니메이션 0프레임부터 강제 재생
        if (!isHitAnimationStarted)
        {
            isHitAnimationStarted = true;
            animator.Play(hitAnimName, 0, 0f);
            return;
        }

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // 2. 유니티 딜레이 억까 방지 (Hit 모션으로 바뀔 때까지 대기)
        if (!stateInfo.IsName(hitAnimName)) return;

        // 3. Hit 모션이 100% 끝나면 다시 Idle 상태로 복귀
        if (stateInfo.normalizedTime >= 1.0f)
        {
            ChangeState(EnemyState.Idle);
        }
    }

    protected override void OnChase() { }
    protected override void OnAttack() { }
    protected override void OnDead() { }
}