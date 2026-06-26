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
    [SerializeField] private float recoverSpeed = 2f;
    [SerializeField] private float delayBeforeRecover = 0.5f;

    private float currentTargetFill = 1f;
    private float lastDamageTime;

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

        if (playerStats.maxHp > 0)
        {
            float targetFill = playerStats.currentHp / playerStats.maxHp;

            if (targetFill < currentTargetFill)
            {
                lastDamageTime = Time.time;
                currentTargetFill = targetFill;
            }
            else if (targetFill > currentTargetFill)
            {
                currentTargetFill = targetFill;
            }

            if (healthBarFill != null)
            {
                healthBarFill.fillAmount = targetFill;
            }

            if (healthBarRecover != null)
            {
                if (Time.time - lastDamageTime >= delayBeforeRecover || targetFill >= healthBarRecover.fillAmount)
                {
                    healthBarRecover.fillAmount = Mathf.Lerp(healthBarRecover.fillAmount, targetFill, Time.deltaTime * recoverSpeed);
                }
            }
        }
    }
}