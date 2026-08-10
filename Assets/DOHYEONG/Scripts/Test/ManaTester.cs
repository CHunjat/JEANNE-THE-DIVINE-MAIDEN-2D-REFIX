using UnityEngine;

public class ManaTester : MonoBehaviour
{
    [Header("플레이어 스탯")]
    [SerializeField] private PlayerStats playerStats;

    [Header("마나 증가 테스트")]
    [SerializeField] private KeyCode increaseKey = KeyCode.Q;
    [SerializeField] private float increaseAmount = 25f;

    [Header("마나 감소 테스트")]
    [SerializeField] private KeyCode decreaseKey = KeyCode.E;
    [SerializeField] private float decreaseAmount = 25f;

    [Header("마나 즉시 설정")]
    [SerializeField] private KeyCode zeroKey = KeyCode.Alpha0;
    [SerializeField] private KeyCode fullKey = KeyCode.Alpha5;

    private void Start()
    {
        if (playerStats == null)
        {
            playerStats = FindFirstObjectByType<PlayerStats>();
        }

        if (playerStats == null)
        {
            Debug.LogError("ManaTester : PlayerStats를 찾을 수 없습니다.");
        }
    }

    private void Update()
    {
        if (playerStats == null)
            return;

        // 마나 증가
        if (Input.GetKeyDown(increaseKey))
        {
            AddMana(increaseAmount);
        }

        // 마나 감소
        if (Input.GetKeyDown(decreaseKey))
        {
            RemoveMana(decreaseAmount);
        }

        // 0으로 초기화
        if (Input.GetKeyDown(zeroKey))
        {
            SetMana(0f);
        }

        // 최대치로 설정
        if (Input.GetKeyDown(fullKey))
        {
            SetMana(playerStats.GetMaxMp());
        }
    }

    private void AddMana(float amount)
    {
        playerStats.currentMp = Mathf.Clamp(
            playerStats.currentMp + amount,
            0f,
            playerStats.GetMaxMp()
        );

        Debug.Log($"MP 증가 : {playerStats.currentMp}");
    }

    private void RemoveMana(float amount)
    {
        playerStats.currentMp = Mathf.Clamp(
            playerStats.currentMp - amount,
            0f,
            playerStats.GetMaxMp()
        );

        Debug.Log($"MP 감소 : {playerStats.currentMp}");
    }

    private void SetMana(float amount)
    {
        playerStats.currentMp = Mathf.Clamp(
            amount,
            0f,
            playerStats.GetMaxMp()
        );

        Debug.Log($"MP 설정 : {playerStats.currentMp}");
    }
}