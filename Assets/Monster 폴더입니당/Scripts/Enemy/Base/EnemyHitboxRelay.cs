using UnityEngine;

// =====================================================
// EnemyHitboxRelay.cs
// 대형 몬스터의 자식 피격 박스(Hurt Box)에 붙여서,
// 플레이어 공격 판정을 부모 본체(진짜 EnemyFSM)로 전달하는 중계기!
// =====================================================
public class EnemyHitboxRelay : EnemyFSM
{
    private EnemyFSM parentFSM;

    protected override void Awake()
    {
        // 부모 오브젝트에 있는 진짜 본체(MidBoss 등) 스크립트를 찾아 연결!
        if (transform.parent != null)
        {
            parentFSM = transform.parent.GetComponent<EnemyFSM>();
        }
    }

    // 플레이어 칼질이 닿아서 TakeDamage()를 호출할 때 실행됨
    public override void TakeDamage(float amount)
    {
        if (parentFSM != null)
        {
            // 진짜 거미 본체한테 데미지를 그대로 토스해서 체력을 깎음!
            parentFSM.TakeDamage(amount);
        }
    }

    // --- 아래는 EnemyFSM이 추상(abstract) 클래스라 에러 방지용으로 비워두는 필수 함수들 ---
    // (얘는 AI 행동이나 피격 애니메이션을 직접 실행하지 않는 단순 과녁이니까 비워두는 게 정답!)
    protected override void OnIdle() { }
    protected override void OnChase() { }
    protected override void OnAttack() { }
    protected override void OnHit() { }
    protected override void OnDead() { }
}