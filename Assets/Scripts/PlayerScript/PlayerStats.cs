using UnityEngine;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    [Header("밸런스 데이터")]
    public StatBalanceSO statBalance;

    [Header("패시브 스탯 레벨 (기획 입력)")]
    public int statHp = 0;         // 1. 체력 (HP, 방어력)
    public int statDex = 0;        // 2. 기량 (공격력)
    public int statStr = 0;        // 3. 근력 (공격력, 방어력)
    public int statSpirit = 0;     // 4. 정신력 (타격 시 스킬 에너지 회복력)
    public int statFaith = 0;      // 5. 신앙심 (스킬 해금, 스킬 위력)
    public int statEndurance = 0;  // 6. 지구력 (이동 속도, 대시 쿨타임)

    [Header("체력 (Health)")]
    public float baseMaxHp = 100f;
    public float currentHp;

    [Header("마나 (MP) 및 타격 회복 시스템")]
    public float baseMaxMp = 500f;
    public float currentMp;
    public float spiritBonusPercent = 0.01f; // 정신력 1렙당 타격 마나 회복 증가율 (기획서의 0.n% 역할. 0.01 = 1%)

    [Header("가드 리게인 (내상 HP) 시스템")]
    public float currentRecoverableHp = 0f;
    public float recoverableRatio = 0.5f;
    public float lifestealRatio = 0.2f;

    [Header("내상 소멸 세팅")]
    public float internalHpDuration = 6f;
    private float internalHpTimer = 0f;
    public bool loseInternalHpOnHit = true;

    [Header("기본 스탯 (Base Stats)")]
    public float baseAttackPower = 0f;  // 기본 공격력
    public float baseDefense = 0f;      // 방어력
    public float baseGroggyPower = 10f; // 캐릭터의 기본 그로기 파괴력

    [Header("상태 이상 및 무적 (Status)")]
    public bool isInvincible = false;
    public float invincibilityDuration = 0.5f;

    private PlayerController playerController;

    // 1. 최종 공격력 = 기본공격력 + 기량(70%) + 근력(30%)
    public float GetTotalAttackPower()
    {
        float atk100 = statBalance.baseAttackPerStat;
        float dexBonus = Mathf.Max(0, statDex - 1) * (atk100 * 0.7f);
        float strBonus = Mathf.Max(0, statStr - 1) * (atk100 * 0.3f);

        return baseAttackPower + dexBonus + strBonus;
    }

    // 2. 방어력 = 체력(30%) + 근력(70%)
    public float GetTotalDefense()
    {
        float def100 = statBalance.baseDefensePerStat;
        float hpBonus = Mathf.Max(0, statHp - 1) * (def100 * 0.3f);
        float strBonus = Mathf.Max(0, statStr - 1) * (def100 * 0.7f);

        return baseDefense + hpBonus + strBonus;
    }

    // 3. 최대 HP = 체력(100%)
    public float GetMaxHp()
    {
        float hp100 = statBalance.baseHpPerStat;
        float hpBonus = Mathf.Max(0, statHp - 1) * (hp100 * 1.0f);

        return baseMaxHp + hpBonus;
    }

    // 4. 최대 MP (정신력은 최대치를 늘려주지 않으므로 기본값 고정)
    public float GetMaxMp()
    {
        return baseMaxMp;
    }

    // 🔥 4-1. 정신력 = 데미지 기반 타격 마나 회복량 증가 공식!
    public float GetMpRecoveryByDamage(float totalDamage, float baseRatio)
    {
        // 1. 데미지에 따른 순수 기본 회복량 (예: 100데미지 * 0.1 = 10 마나)
        float baseRecovery = totalDamage * baseRatio;

        // 2. 정신력 스탯에 따른 뻥튀기 비율 산출
        float bonusRatio = Mathf.Max(0, statSpirit - 1) * spiritBonusPercent;

        // 3. 최종 회복량 = 기본 회복량 * (100% + 보너스%)
        return baseRecovery * (1f + bonusRatio);
    }

    public float GetFinalGroggyPower()
    {
        return baseGroggyPower;
    }

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        currentHp = GetMaxHp();
        currentMp = GetMaxMp();
    }

    private void Update()
    {
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

        float finalDamage = Mathf.Max(amount - GetTotalDefense(), 1f);
        currentHp = Mathf.Max(0, currentHp - finalDamage);

        Debug.Log($"플레이어 피격! 받은 데미지: {finalDamage} / 남은 체력: {currentHp}");

        if (currentHp == 0)
        {
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

    public bool TryConsumeMp(float cost)
    {
        if (currentMp >= cost)
        {
            currentMp -= cost;
            Debug.Log($"<color=cyan>스킬 마나 결제 성공! 소모 MP: {cost} (남은 MP: {currentMp})</color>");
            return true;
        }

        Debug.Log("<color=red>마나 부족! 스킬 발동 실패</color>");
        return false;
    }

    // 🔥 데미지에 비례한 타격 마나 회복 실행 함수
    public void RestoreMpByDamage(float totalDamage, float baseRatio)
    {
        if (currentMp >= GetMaxMp()) return; // 이미 풀마나면 패스

        float finalRecovery = GetMpRecoveryByDamage(totalDamage, baseRatio);
        currentMp += finalRecovery;

        if (currentMp > GetMaxMp()) currentMp = GetMaxMp();

        Debug.Log($"<color=blue>적 타격! 데미지({totalDamage}) 비례 MP 회복: {finalRecovery} (현재 MP: {currentMp})</color>");
    }

    public void Heal(float amount)
    {
        if (currentHp <= 0) return;
        currentHp = Mathf.Min(currentHp + amount, GetMaxHp());
        Debug.Log($"힐 발동 완료! 현재 체력: {currentHp}");
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }
}