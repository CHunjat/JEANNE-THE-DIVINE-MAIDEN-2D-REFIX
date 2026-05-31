using UnityEngine;

// =====================================================
// MidBossWebProjectile.cs
// 거미 보스 거미줄 발사체 스크립트임.
// MidBossPattern3에서 Instantiate할 때 자동으로 설정됨.
//
// [구속 상태이상 처리]
// 플레이어에 닿으면 PlayerHealth의 ApplyBind()를 호출함.
// Player 담당자가 ApplyBind(float duration) 함수를 구현해야 함.
// → Player 담당자에게 요청할 것: "public void ApplyBind(float duration)"
//
// [이 스크립트가 붙은 오브젝트 구성]
// - CircleCollider2D (Is Trigger 체크)
// - EnemyHitbox 스크립트 (데미지 처리)
// - MidBossWebProjectile 스크립트 (이것 - 이동 + 구속 처리)
// =====================================================
public class MidBossWebProjectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private float maxRange;
    private float bindDuration;
    private Vector2 startPos;

    public void Initialize(Vector2 dir, float spd, float range, float bind)
    {
        direction = dir;
        speed = spd;
        maxRange = range;
        bindDuration = bind;
        startPos = transform.position;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);

        if (Vector2.Distance(startPos, transform.position) >= maxRange)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

        // 구속 상태이상 적용 - Player 담당자의 ApplyBind 함수 호출
        // Player 담당자에게 요청: public void ApplyBind(float duration) 구현 필요
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            // playerHealth.ApplyBind(bindDuration);  ← Player 담당자가 ApplyBind 만들면 주석 해제
            Debug.Log($"[MidBossWebProjectile] 플레이어 구속 상태이상 적용! 지속시간: {bindDuration}초");
            Debug.Log("[MidBossWebProjectile] Player 담당자에게 ApplyBind(float duration) 구현 요청할 것.");
        }

        Destroy(gameObject);  // 거미줄은 맞으면 사라짐
    }
}