using UnityEngine;

// =====================================================
// SlashProjectile.cs
// 앞발 휘두르기 이펙트(검기)의 이동을 처리하는 스크립트임.
// MidBossPattern2에서 Instantiate할 때 자동으로 설정됨.
//
// [이 스크립트가 붙은 오브젝트 구성]
// - CircleCollider2D (Is Trigger 체크)
// - EnemyHitbox 스크립트
// - SlashProjectile 스크립트 (이것)
// - SpriteRenderer (나중에 스프라이트 받으면 추가)
// =====================================================
public class SlashProjectile : MonoBehaviour
{
    private Vector2 direction;    // 이동 방향
    private float speed;          // 이동 속도
    private float maxRange;       // 최대 사거리
    private Vector2 startPos;     // 시작 위치

    // MidBossPattern2에서 발사 시 호출됨
    public void Initialize(Vector2 dir, float spd, float range)
    {
        direction = dir;
        speed = spd;
        maxRange = range;
        startPos = transform.position;

        // 이동 방향으로 오브젝트 회전 (스프라이트가 있을 때 방향 맞춤)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void Update()
    {
        // 매 프레임 방향으로 이동
        transform.Translate(Vector2.right * speed * Time.deltaTime);

        // 최대 사거리 도달 시 삭제
        if (Vector2.Distance(startPos, transform.position) >= maxRange)
        {
            Debug.Log("[SlashProjectile] 최대 사거리 도달. 삭제됨.");
            Destroy(gameObject);
        }
    }
}