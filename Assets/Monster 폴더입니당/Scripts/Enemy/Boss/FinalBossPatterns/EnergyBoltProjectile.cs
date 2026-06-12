using UnityEngine;

// =====================================================
// EnergyBoltProjectile.cs
// 데몬 누나 에너지 볼트 발사체 이동 스크립트임.
// FinalBossPattern2에서 Instantiate할 때 자동으로 설정됨.
//
// [이 스크립트가 붙은 오브젝트 구성]
// - CircleCollider2D (Is Trigger 체크)
// - EnemyHitbox 스크립트 (Destroy On Hit 체크)
// - EnergyBoltProjectile 스크립트 (이것)
// - SpriteRenderer (나중에 스프라이트 받으면 추가)
// =====================================================
public class EnergyBoltProjectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private float maxRange;
    private Vector2 startPos;

    public void Initialize(Vector2 dir, float spd, float range)
    {
        direction = dir;
        speed = spd;
        maxRange = range;
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
}