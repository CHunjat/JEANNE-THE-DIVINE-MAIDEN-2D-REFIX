using UnityEngine;

// =====================================================
// EnemyHitboxRelay.cs
// 대형 몬스터의 자식 피격 박스(Hurt Box)에 붙여서,
// 플레이어 공격 판정을 부모 본체(진짜 EnemyFSM)로 전달하는 중계기
// =====================================================
public class EnemyHitboxRelay : EnemyFSM
{
    private EnemyFSM parentFSM;

    protected override void Awake()
    {
        // 부모 오브젝트에 있는 진짜 본체(MidBoss 등) 스크립트를 찾아 연결
        if (transform.parent != null)
        {
            parentFSM = transform.parent.GetComponent<EnemyFSM>();
        }
    }

    // [수정됨] 여기도 똑같이 인수 2개로 부모한테 토스하게 수정
    public override void TakeDamage(float amount, float groggyDamage = 0f)
    {
        if (parentFSM != null)
        {
            // 진짜 거미 본체한테 체력 데미지랑 그로기 데미지 둘 다 토스
            parentFSM.TakeDamage(amount, groggyDamage);
        }
    }

    // --- 아래는 EnemyFSM이 추상(abstract) 클래스라 에러 방지용으로 비워두는 필수 함수들 ---
    protected override void OnIdle() { }
    protected override void OnChase() { }
    protected override void OnAttack() { }
    protected override void OnHit() { }
    protected override void OnDead() { }
}