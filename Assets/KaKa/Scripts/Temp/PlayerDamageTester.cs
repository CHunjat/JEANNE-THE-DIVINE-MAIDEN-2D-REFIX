using UnityEngine;

public class PlayerDamageTester : MonoBehaviour
{
    [Header("플레이어 스탯 참조")]
    [SerializeField] private PlayerStats playerStats;

    [Header("테스트 설정")]
    [SerializeField] private float damageAmount = 20f;      // 한 번 누를 때 깎일 데미지 양
    [SerializeField] private KeyCode damageKey = KeyCode.Q;  // 데미지를 유발할 키 (Q키)

    void Start()
    {
        // 인스펙터에서 플레이어를 깜빡하고 연결 안 했다면 게임 시작 시 자동으로 검색
        if (playerStats == null)
        {
            playerStats = FindFirstObjectByType<PlayerStats>();
        }
    }

    void Update()
    {
        if (playerStats == null) return;

        // 지정한 키(Q)를 누르면 PlayerStats의 TakeDamage를 호출합니다.
        if (Input.GetKeyDown(damageKey))
        {
            Debug.Log($"[테스트] {damageKey}키 입력 - 플레이어에게 {damageAmount} 데미지를 입힙니다.");
            playerStats.TakeDamage(damageAmount);
        }
    }
}