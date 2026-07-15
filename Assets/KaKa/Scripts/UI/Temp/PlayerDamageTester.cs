using UnityEngine;

public class PlayerDamageTester : MonoBehaviour
{
    [Header("플레이어 스탯 참조")]
    [SerializeField] private PlayerStats playerStats;

    [Header("MP 차감 테스트 설정")]
    [SerializeField] private float mpDecreaseAmount = 100f;  // 한 번 누를 때 깎일 MP 양 (100 단위)
    [SerializeField] private KeyCode mpKey = KeyCode.W;      // MP를 깎을 키 (W키)

    [Header("MP 회복 테스트 설정")]
    [SerializeField] private float mpIncreaseAmount = 50f;   // 한 번 누를 때 찰 MP 양 (50 단위)
    [SerializeField] private KeyCode mpIncreaseKey = KeyCode.E; // MP를 채울 키 (E키)

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

        // 1. 지정한 키(W)를 누르면 MP 차감
        if (Input.GetKeyDown(mpKey))
        {
            // MP가 0 이하로 떨어지지 않도록 방어 처리하며 차감
            playerStats.currentMp = Mathf.Max(playerStats.currentMp - mpDecreaseAmount, 0f);

            Debug.Log($"[테스트] {mpKey}키 입력 - 플레이어의 MP를 {mpDecreaseAmount}만큼 깎습니다. (현재 MP: {playerStats.currentMp})");
        }

        // 2. 지정한 키(E)를 누르면 MP 회복
        if (Input.GetKeyDown(mpIncreaseKey))
        {
            // MP가 PlayerStats에 정해진 MaxMp(500)를 넘지 않도록 방어 처리하며 차감
            playerStats.currentMp = Mathf.Min(playerStats.currentMp + mpIncreaseAmount, playerStats.baseMaxMp);

            Debug.Log($"[테스트] {mpIncreaseKey}키 입력 - 플레이어의 MP를 {mpIncreaseAmount}만큼 채웁니다. (현재 MP: {playerStats.currentMp})");
        }
    }
}