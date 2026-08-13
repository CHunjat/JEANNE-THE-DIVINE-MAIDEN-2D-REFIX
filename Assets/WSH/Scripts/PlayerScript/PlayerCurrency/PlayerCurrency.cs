using UnityEngine;

public class PlayerCurrency : MonoBehaviour
{
    [Header("재화 보유량")]
    public int anima = 0; // 아니마 (스킬 성장 재화)
    public int gold = 0;  // 골드 (일반 재화)

    public void AddAnima(int amount)
    {
        anima += amount;
        Debug.Log($"아니마 획득! (+{amount}) 현재 아니마: {anima}");
    }

    public void AddGold(int amount)
    {
        gold += amount;
        Debug.Log($"골드 획득! (+{amount}) 현재 골드: {gold}");
    }

    public bool TryConsumeGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            return true;
        }
        return false;
    }

    public bool TryUpgradeSkill(SkillData targetSkill)
    {
        if (targetSkill == null || targetSkill.growthData == null) return false;

        if (targetSkill.currentLevel >= targetSkill.growthData.growthTable.Length)
        {
            Debug.Log($"{targetSkill.skillName} 스킬은 이미 최고 레벨입니다.");
            return false;
        }

        int cost = targetSkill.growthData.growthTable[targetSkill.currentLevel - 1].requiredAnima;

        if (anima >= cost)
        {
            anima -= cost;
            targetSkill.currentLevel++;
            Debug.Log($"{targetSkill.skillName} 레벨업 성공! (현재 Lv.{targetSkill.currentLevel} / 남은 아니마: {anima})");
            return true;
        }

        Debug.Log($"아니마가 부족합니다! (필요: {cost} / 보유: {anima})");
        return false;
    }
}