using UnityEngine;

[CreateAssetMenu(fileName = "HealDataSO", menuName = "Skill System/Heal Data")]
public class HealDataSO : ScriptableObject
{
    public string healName;
    public float healMpCost = 100f;

    [Header("레벨별 회복량 (0번칸 = 1레벨)")]
    public float[] healAmountByLevel;

    // 현재 레벨에 맞는 회복량 반환 함수
    public float GetHealAmount(int currentLevel)
    {
        if (healAmountByLevel == null || healAmountByLevel.Length == 0) return 0f;
        int index = Mathf.Clamp(currentLevel - 1, 0, healAmountByLevel.Length - 1);
        return healAmountByLevel[index];
    }
}