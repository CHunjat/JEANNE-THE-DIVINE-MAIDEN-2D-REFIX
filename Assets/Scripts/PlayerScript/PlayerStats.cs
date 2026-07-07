using UnityEngine;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    [Header("체력 (Health)")]
    public float maxHp = 100f;
    public float currentHp;
    public float MaxMp = 100f;
    public float currentMp;

    [Header("가드 리게인 (내상 HP) 시스템")]
    public float currentRecoverableHp = 0f; // 현재 쌓여있는 내상 HP
    public float recoverableRatio = 0.5f;   // 가드 데미지의 내상 전환율 (예: 50%)
    public float lifestealRatio = 0.2f;     // 가한 데미지의 피흡 비율 (예: 20%)

    [Header("내상 소멸 세팅")]
    public float internalHpDuration = 6f;   // 유지 시간 (6초)
    private float internalHpTimer = 0f;     // 유지 시간 타이머
    public bool loseInternalHpOnHit = true; // 피격 시 내상 즉시 소멸 여부

    [Header("기본 스탯 (Base Stats)")]
    public float baseAttackPower = 0f;  // 기본 공격력
    public float defense = 0f;          // 방어력
    public float baseGroggyPower = 10f; // [추가] 캐릭터의 기본 그로기 파괴력

    [Header("상태 이상 및 무적 (Status)")]
    public bool isInvincible = false;   // 무적 상태 여부
    public float invincibilityDuration = 0.5f; // 피격 시 무적 시간

    private PlayerController playerController;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        currentHp = maxHp;
        currentMp = MaxMp;
    }

    private void Update()
    {
        // 룰: 내상 HP 6초 유지 후 즉시 소멸
        if (currentRecoverableHp > 0)
        {
            internalHpTimer -= Time.deltaTime;
            if (internalHpTimer <= 0)
            {
                currentRecoverableHp = 0f;
                Debug.Log("<color=gray>6초 경과: 내상 HP 즉시 소멸!</color>");
            }
        }
    }

    public void SetInternalHp(float amount)
    {
        currentRecoverableHp = amount;
        internalHpTimer = internalHpDuration;
    }

    public void TakeDamage(float amount, bool isGuard = false)
    {
        if (isInvincible || currentHp <= 0) return;

        float finalDamage = Mathf.Max(amount - defense, 1f);
        currentHp -= finalDamage;

        Debug.Log($"플레이어 피격! 받은 데미지: {finalDamage} / 남은 체력: {currentHp}");

        if (currentHp <= 0)
        {
            currentHp = 0;
            if (playerController.StateMachine.CurrentState != playerController.DieState)
            {
                playerController.StateMachine.ChangeState(playerController.DieState);
            }
        }
        else
        {
            if (!isGuard)
            {
                StartCoroutine(InvincibilityRoutine());
            }
        }
    }

    public void Heal(float amount, float mpCost)
    {
        if (currentHp <= 0) return;

        if (currentMp < mpCost)
        {
            Debug.Log("MP가 부족하여 힐을 사용할 수 없습니다!");
            return;
        }

        currentMp -= mpCost;
        currentHp = Mathf.Min(currentHp + amount, maxHp);

        Debug.Log($"힐 사용! MP 소모: {mpCost} / 현재 체력: {currentHp} / 남은 MP: {currentMp}");
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

    // =========================================================
    // [추가] 컨트롤러에서 호출할 '최종 그로기 힘' 계산기
    // =========================================================
    public float GetFinalGroggyPower()
    {
        return baseGroggyPower;
    }
}