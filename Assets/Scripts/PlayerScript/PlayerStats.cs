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
    public int statSpirit = 0;     // 4. 정신력 (스킬 에너지 회복력)
    public int statFaith = 0;      // 5. 신앙심 (스킬 해금, 스킬 위력)
    public int statEndurance = 0;  // 6. 지구력 (이동 속도, 대시 쿨타임)

    [Header("체력 (Health)")]
    public float baseMaxHp = 100f;
    public float currentHp;
    public float baseMaxMp = 100f;
    public float currentMp;

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
    public float baseDefense = 0f;          // 방어력
    public float baseGroggyPower = 10f; // 캐릭터의 기본 그로기 파괴력

    [Header("상태 이상 및 무적 (Status)")]
    public bool isInvincible = false;
    public float invincibilityDuration = 0.5f;

    private PlayerController playerController;

    // =========================================================
    // [핵심 추가] 기획서 패시브 스탯 공식 적용기
    // =========================================================

    // 1. 최종 공격력 = 기본공격력 + 기량(70%) + 근력(30%)
    public float GetTotalAttackPower()
    {
        float atk100 = statBalance.baseAttackPerStat;

        // [수정] 스탯 레벨에서 1을 뺀 값으로 계산 (최소 0 보장)
        float dexBonus = Mathf.Max(0, statDex - 1) * (atk100 * 0.7f);
        float strBonus = Mathf.Max(0, statStr - 1) * (atk100 * 0.3f);

        return baseAttackPower + dexBonus + strBonus;
    }

    // 2. 방어력 = 체력(30%) + 근력(70%)
    public float GetTotalDefense()
    {
        float def100 = statBalance.baseDefensePerStat;

        // [수정] 스탯 레벨에서 1을 뺀 값으로 계산
        float hpBonus = Mathf.Max(0, statHp - 1) * (def100 * 0.3f);
        float strBonus = Mathf.Max(0, statStr - 1) * (def100 * 0.7f);

        return baseDefense + hpBonus + strBonus;
    }

    // 3. 최대 HP = 체력(100%)
    public float GetMaxHp()
    {
        float hp100 = statBalance.baseHpPerStat;

        // [수정] 스탯 레벨에서 1을 뺀 값으로 계산
        float hpBonus = Mathf.Max(0, statHp - 1) * (hp100 * 1.0f);

        return baseMaxHp + hpBonus;
    }

    // 4. 최대 MP = 정신력(100%)
    public float GetMaxMp()
    {
        float mp100 = statBalance.baseMpPerStat;

        // [수정] 스탯 레벨에서 1을 뺀 값으로 계산
        float mpBonus = Mathf.Max(0, statSpirit - 1) * (mp100 * 1.0f);

        return baseMaxMp + mpBonus;
    }

    public float GetFinalGroggyPower()
    {
        return baseGroggyPower;
    }

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        // 게임 시작 시, 공식이 적용된 최종 체력과 MP로 셋팅
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

        // [수정됨] 방어력 계산 시 기존 defense 대신 GetTotalDefense() 사용
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

    public void Heal(float amount, float mpCost)
    {
        if (currentHp <= 0) return;

        if (currentMp < mpCost)
        {
            Debug.Log("MP가 부족하여 힐을 사용할 수 없습니다!");
            return;
        }

        currentMp -= mpCost;
        // 힐 할 때도 최대 체력을 GetMaxHp()로 검사
        currentHp = Mathf.Min(currentHp + amount, GetMaxHp());

        Debug.Log($"힐 사용! MP 소모: {mpCost} / 현재 체력: {currentHp} / 남은 MP: {currentMp}");
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }
}