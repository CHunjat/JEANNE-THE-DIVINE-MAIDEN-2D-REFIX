using UnityEngine;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    [Header("체력 (Health)")]
    public float maxHp = 100f;
    public float currentHp;
    public float MaxMp = 100f;
    public float currentMp;

    [Header("기본 스탯 (Base Stats)")]
    public float baseAttackPower = 0f; // 기본 공격력
    public float defense = 0f;          // 방어력

    [Header("상태 이상 및 무적 (Status)")]
    public bool isInvincible = false;   // 무적 상태 여부
    public float invincibilityDuration = 1.0f; // 피격 시 무적 시간

    private void Awake()
    {
        // 게임 시작 시 체력을 최대치로 초기화
        currentHp = maxHp;
        currentMp = MaxMp;
    }

    //파트너 호출용 몬스터가 플레이어를 때릴 때 사용할 함수
    public void TakeDamage(float amount)
    {
        // 무적 상태이거나 이미 죽었다면 데미지 무시
        if (isInvincible || currentHp <= 0) return;

        // 방어력 연산 (최소 1의 데미지는 무조건 받도록 처리)
        float finalDamage = Mathf.Max(amount - defense, 1f);
        currentHp -= finalDamage;

        Debug.Log($"플레이어 피격! 받은 데미지: {finalDamage} / 남은 체력: {currentHp}");

        if (currentHp <= 0)
        {
            currentHp = 0;
            Die();
        }
        else
        {
            // 살아있다면 피격 무적 시간 발동
            StartCoroutine(InvincibilityRoutine());
        }
    }

    // 💚 힐 스킬이나 물약을 먹었을 때 사용할 함수
    public void Heal(float amount, float mpCost)
    {
        if (currentHp <= 0) return; // 죽었을 땐 힐 불가

        if (currentMp < mpCost)
        {
            Debug.Log("MP가 부족하여 힐을 사용할 수 없습니다!");
            return;
        }

        // 3. MP 소모 및 체력 회복
        currentMp -= mpCost;
        currentHp = Mathf.Min(currentHp + amount, maxHp);

        Debug.Log($"힐 사용! MP 소모: {mpCost} / 현재 체력: {currentHp} / 남은 MP: {currentMp}");
    }

    // 피격 시 잠시 무적이 되는 코루틴
    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        // TODO: 여기서 플레이어 스프라이트를 깜빡거리게 하는 등 피격 시각 효과 추가 가능
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

    // 사망 처리
    private void Die()
    {
        Debug.Log("플레이어 사망!");
        // TODO: 사망 애니메이션 트리거, 입력 차단, 게임오버 UI 호출 등
    }
}