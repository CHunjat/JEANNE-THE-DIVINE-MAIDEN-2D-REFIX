using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    [Header("플레이어 스탯 참조 (자동 검색)")]
    private PlayerStats playerStats; // 이제 인스펙터에서 안 넣어도 됩니다!

    [Header("관리할 체력바 UI 요소들")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Image healthBarRecover;

    [Header("내상 체력(Recover) 연출 설정")]
    [SerializeField] private float recoverBarChaseSpeed = 4f; // 회색 바가 목표치를 따라가는 부드러운 속도

    void Start()
    {
        // 씬 전체에서 PlayerStats 스크립트를 가진 오브젝트를 자동으로 찾아 연결합니다.
        playerStats = FindFirstObjectByType<PlayerStats>();

        if (playerStats == null)
        {
            Debug.LogError("HPBarManager: 씬에서 PlayerStats 스크립트를 찾을 수 없습니다! 플레이어 오브젝트에 스크립트가 잘 붙어있는지 확인하세요.");
        }
    }

    void Update()
    {
        if (playerStats == null) return;
        if (playerStats.baseMaxHp <= 0) return;

        float maxHp = playerStats.GetMaxHp();
        if (maxHp <= 0f) maxHp = playerStats.baseMaxHp;

        // 실제 체력 바 = 즉시 반영 (내상 체력은 별도 바가 담당)
        float currentFill = playerStats.currentHp / maxHp;
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = currentFill;
        }

        // 회색(내상) 바 = "현재 체력 + 회복 가능한 내상 체력"을 목표로 부드럽게 추적
        if (healthBarRecover != null)
        {
            float recoverTargetFill = (playerStats.currentHp + playerStats.currentRecoverableHp) / maxHp;
            healthBarRecover.fillAmount = Mathf.MoveTowards(
                healthBarRecover.fillAmount,
                recoverTargetFill,
                Time.deltaTime * recoverBarChaseSpeed);
        }
    }
}