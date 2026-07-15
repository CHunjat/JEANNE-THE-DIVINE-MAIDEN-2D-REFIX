using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 때려서 파괴할 수 있는 오브젝트. 파괴된 상태는 GameOverManager.lastRespawnPosition과
/// 완전히 같은 원리(static 필드)로 씬 리로드에도 유지됩니다.
/// </summary>
public class DestructibleObject : MonoBehaviour
{
    [Header("고유 ID (비워두면 이름+위치 기반으로 자동 생성)")]
    [Tooltip("씬에 같은 프리팹을 여러 개 배치한다면, 반드시 서로 다른 값을 직접 입력해주세요.")]
    [SerializeField] private string objectId;

    [Header("체력")]
    public float maxHp = 30f;
    private float currentHp;

    [Header("파괴 연출 (선택)")]
    public GameObject breakEffectPrefab; // 파괴될 때 생성할 파티클 등 (없으면 비워둬도 무방)
    public bool destroyGameObjectOnBreak = false; // true면 Destroy, false면 SetActive(false)로만 처리

    [Header("파괴 시 추가 연동 (선택)")]
    [Tooltip("이 오브젝트가 파괴될 때 함께 비활성화(SetActive(false))할 오브젝트를 여기에 넣어주세요.")]
    public GameObject objectToDisable; // ★ 여기에 bossroom 오브젝트를 넣어주시면 됩니다!

    // ★ static이므로 SceneManager.LoadScene()으로 씬이 통째로 리로드되어도 값이 유지됩니다.
    private static readonly HashSet<string> destroyedObjectIds = new HashSet<string>();

    private void Awake()
    {
        if (string.IsNullOrEmpty(objectId))
        {
            objectId = $"{gameObject.name}_{transform.position.x:F1}_{transform.position.y:F1}";
        }

        // 이전 세션(리로드 전)에 이미 파괴됐던 오브젝트라면, 다시 나타나지 않도록 즉시 비활성화
        if (destroyedObjectIds.Contains(objectId))
        {
            gameObject.SetActive(false);

            // ★ [연동] 이미 파괴된 상태로 씬이 리로드되었다면, 연동된 오브젝트도 다시 꺼줍니다.
            if (objectToDisable != null)
            {
                objectToDisable.SetActive(false);
            }
            return;
        }

        currentHp = maxHp;
    }

    // 플레이어 공격 판정 스크립트에서 이 함수를 호출해주세요
    public void TakeDamage(float amount)
    {
        if (currentHp <= 0f) return; // 이미 파괴됐으면 중복 처리 방지

        currentHp -= amount;
        if (currentHp <= 0f)
        {
            BreakObject();
        }
    }

    private void BreakObject()
    {
        destroyedObjectIds.Add(objectId); // ★ 파괴 상태 기록

        // ★ [연동] 파괴되는 순간에 등록된 오브젝트를 비활성화합니다.
        if (objectToDisable != null)
        {
            objectToDisable.SetActive(false);
            Debug.Log($"[DestructibleObject] {gameObject.name} 파괴 완료! -> 연동된 {objectToDisable.name}을 비활성화했습니다.");
        }

        // 파괴 이펙트 생성
        if (breakEffectPrefab != null)
        {
            Instantiate(breakEffectPrefab, transform.position, transform.rotation);
        }

        if (destroyGameObjectOnBreak)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}