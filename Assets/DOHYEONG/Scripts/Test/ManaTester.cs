using UnityEngine;

public class ManaTester : MonoBehaviour
{
    [Header("�÷��̾� ����")]
    [SerializeField] private PlayerStats playerStats;

    [Header("���� ���� �׽�Ʈ")]
    [SerializeField] private KeyCode increaseKey = KeyCode.Q;
    [SerializeField] private float increaseAmount = 25f;

    [Header("���� ���� �׽�Ʈ")]
    [SerializeField] private KeyCode decreaseKey = KeyCode.E;
    [SerializeField] private float decreaseAmount = 25f;

    [Header("���� ��� ����")]
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
            Debug.LogError("ManaTester : PlayerStats�� ã�� �� �����ϴ�.");
        }
    }

    private void Update()
    {
        if (playerStats == null)
            return;

        // ���� ����
        if (Input.GetKeyDown(increaseKey))
        {
            AddMana(increaseAmount);
        }

        // ���� ����
        if (Input.GetKeyDown(decreaseKey))
        {
            RemoveMana(decreaseAmount);
        }

        // 0���� �ʱ�ȭ
        if (Input.GetKeyDown(zeroKey))
        {
            SetMana(0f);
        }

        // �ִ�ġ�� ����
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

        Debug.Log($"MP ���� : {playerStats.currentMp}");
    }

    private void RemoveMana(float amount)
    {
        playerStats.currentMp = Mathf.Clamp(
            playerStats.currentMp - amount,
            0f,
            playerStats.GetMaxMp()
        );

        Debug.Log($"MP ���� : {playerStats.currentMp}");
    }

    private void SetMana(float amount)
    {
        playerStats.currentMp = Mathf.Clamp(
            amount,
            0f,
            playerStats.GetMaxMp()
        );

        Debug.Log($"MP ���� : {playerStats.currentMp}");
    }
}