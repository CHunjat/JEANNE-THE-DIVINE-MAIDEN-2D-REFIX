using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 때려서 파괴할 수 있는 오브젝트. 파괴된 상태는 GameOverManager.lastRespawnPosition과
/// 완전히 같은 원리(static 필드)로 씬 리로드에도 유지됩니다.
///
/// 주의: static이므로 "플레이 모드 정지" 또는 "스크립트 재컴파일" 시에는 초기화됩니다.
/// (GameOverManager의 static 변수들과 똑같은 특성이라 게임 세션 내내는 문제 없이 동작합니다.)
/// </summary>
public class DestructibleObject : MonoBehaviour
{
    [Header("고유 ID (비워두면 이름+위치 기반으로 자동 생성)")]
    [Tooltip("씬에 같은 프리팹을 여러 개 배치한다면, 반드시 서로 다른 값을 직접 입력해주세요.")]
    [SerializeField] private string objectId;

    [Header("체력")]
    public float maxHp = 30f;
    private float currentHp;

    [Header("파괴 연출")]
    public SpriteRenderer spriteRenderer;   // 상자의 SpriteRenderer (비워두면 자동으로 GetComponent 시도)
    public Sprite brokenSprite;             // 파괴됐을 때 바꿔낄 스프라이트 (예: Obj_chest_7)
    public GameObject breakEffectPrefab;    // 파괴되는 "순간"에 한 번 재생할 파티클 등 (없으면 비워둬도 무방)
    public bool disableColliderOnBreak = true; // 파괴 후 더 이상 맞지 않도록 콜라이더 끄기

    // ★ static이므로 SceneManager.LoadScene()으로 씬이 통째로 리로드되어도 값이 유지됩니다.
    private static readonly HashSet<string> destroyedObjectIds = new HashSet<string>();

    private bool isBroken = false;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (string.IsNullOrEmpty(objectId))
        {
            objectId = $"{gameObject.name}_{transform.position.x:F1}_{transform.position.y:F1}";
        }

        // 이전 세션(리로드 전)에 이미 파괴됐던 오브젝트라면, 처음부터 파괴된 모습으로 표시
        if (destroyedObjectIds.Contains(objectId))
        {
            ApplyBrokenVisual(); // 오브젝트는 그대로 살아있고, 스프라이트만 파괴 상태로
            return;
        }

        currentHp = maxHp;
    }

    // 플레이어 공격 판정 스크립트에서 이 함수를 호출해주세요
    public void TakeDamage(float amount)
    {
        if (isBroken) return; // 이미 파괴됐으면 중복 처리 방지

        currentHp -= amount;
        if (currentHp <= 0f)
        {
            BreakObject();
        }
    }

    private void BreakObject()
    {
        destroyedObjectIds.Add(objectId); // ★ 핵심: 파괴됐다는 사실을 static 저장소에 기록

        if (breakEffectPrefab != null)
        {
            Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);
        }

        ApplyBrokenVisual();
    }

    // 오브젝트를 끄거나 없애지 않고, 스프라이트만 파괴된 모습으로 바꾸는 함수
    private void ApplyBrokenVisual()
    {
        isBroken = true;
        currentHp = 0f;

        if (spriteRenderer != null && brokenSprite != null)
        {
            spriteRenderer.sprite = brokenSprite;
        }

        if (disableColliderOnBreak)
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }
    }
}