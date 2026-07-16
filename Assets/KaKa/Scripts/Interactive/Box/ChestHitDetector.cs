using UnityEngine;

[RequireComponent(typeof(DestructibleObject))]
public class ChestHitDetector : MonoBehaviour
{
    [Header("설정")]
    [Tooltip("플레이어나 플레이어의 공격 히트박스에 설정된 태그 이름")]
    public string playerTag = "Player";

    [Tooltip("한 번 부딪힐 때 상자가 입을 고정 피해량")]
    public float damageToTake = 10f;

    private DestructibleObject destructible;

    private void Awake()
    {
        destructible = GetComponent<DestructibleObject>();
    }

    // 1. 플레이어 공격(또는 몸)이 Trigger 콜라이더일 경우
    private void OnTriggerEnter2D(Collider2D collision)
    {
        CheckAndTakeDamage(collision.gameObject);
    }

    // 2. 플레이어 공격(또는 몸)이 일반 물리 콜라이더일 경우
    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckAndTakeDamage(collision.gameObject);
    }

    private void CheckAndTakeDamage(GameObject hitObject)
    {
        // 닿은 오브젝트의 태그가 지정한 태그(Player 등)와 일치한다면 데미지 적용
        if (hitObject.CompareTag(playerTag))
        {
            destructible.TakeDamage(damageToTake);
            Debug.Log($"[ChestHitDetector] {hitObject.name}에 닿아 상자가 {damageToTake}의 데미지를 입었습니다!");
        }
    }
}