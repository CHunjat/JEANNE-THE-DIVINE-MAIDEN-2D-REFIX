using UnityEngine;

public class PlayerCurrency : MonoBehaviour
{
    public const int MAX_CURRENCY = 999_999_999;

    [Header("재화 보유량")]
    public int anima = 0;
    public int gold = 0;

    public void AddAnima(int amount)
    {
        if (amount <= 0)
            return;

        anima = Mathf.Clamp(anima + amount,0,MAX_CURRENCY);
        Debug.Log(
            $"아니마 획득! (+{amount}) 현재 아니마: {anima:N0}"
        );
    }

    public void AddGold(int amount)
    {
        if (amount <= 0)
            return;

        gold = Mathf.Clamp(gold + amount, 0, MAX_CURRENCY);

        Debug.Log(
            $"골드 획득! (+{amount}) 현재 골드: {gold:N0}"
        );
    }

    public bool TryConsumeGold(int amount)
    {
        if (amount <= 0)
            return false;

        if (gold >= amount)
        {
            gold -= amount;
            return true;
        }

        return false;
    }

    public bool TryUpgradeSkill(SkillData targetSkill)
    {
        if (targetSkill == null || targetSkill.growthData == null)
            return false;

        if (targetSkill.currentLevel >=targetSkill.growthData.growthTable.Length)
        {
            Debug.Log($"{targetSkill.skillName} 스킬은 이미 최고 레벨입니다.");

            return false;
        }

        int cost =targetSkill.growthData .growthTable[targetSkill.currentLevel - 1].requiredAnima;

        if (anima >= cost)
        {
            anima -= cost;
            targetSkill.currentLevel++;

            Debug.Log( $"{targetSkill.skillName} 레벨업 성공! " + $"(현재 Lv.{targetSkill.currentLevel}/" +$"남은 아니마: {anima:N0})");

            return true;
        }

        Debug.Log( $"아니마가 부족합니다! " +$"(필요: {cost:N0} / 보유: {anima:N0})");
        return false;
    }

#if UNITY_EDITOR //인스펙터에서 최대수량보다 많이 적더라도 최대수량으로 고정
    private void OnValidate()
    {
        anima = Mathf.Clamp(
            anima,
            0,
            MAX_CURRENCY
        );

        gold = Mathf.Clamp(
            gold,
            0,
            MAX_CURRENCY
        );
    }
#endif
}