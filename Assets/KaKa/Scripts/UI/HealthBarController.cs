using UnityEngine;
using UnityEngine.UI; // UI 컴포넌트 제어를 위해 필수

public class HealthBarController : MonoBehaviour
{
    [SerializeField] private Image healthBarFill; // HealthBar_Fill 이미지를 드래그 앤 드롭

    public float maxHealth = 100f;
    private float currentHealth;
    void Start()
    {
        currentHealth = maxHealth;
    }

    // 체력을 깎는 함수 (예시)
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // 0 이하로 내려가지 않게 방지

        // UI 업데이트: 0~1 사이의 비율로 계산
        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        healthBarFill.fillAmount = currentHealth / maxHealth;
    }
}
