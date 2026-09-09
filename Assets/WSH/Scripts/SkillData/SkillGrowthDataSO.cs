using UnityEngine;

[System.Serializable]
public struct SkillLevelData
{
    public int level;
    public int requiredAnima;    // 다음 레벨까지 필요한 비용
    public int cumulativeAnima;  // 1레벨부터 누적 (자동 계산됨)
}

[CreateAssetMenu(fileName = "NewSkillGrowth", menuName = "Skill System/Skill Growth Data")]
public class SkillGrowthDataSO : ScriptableObject
{
    [Header("기획자 입력칸: 레벨별 필요 아니마 최대레벨 정해서 너가 +눌러서 추가하셈")] 
    public SkillLevelData[] growthTable;

    // 기획자가 숫자를 적을 때마다 누적값을 자동 계산해 주는 기능
    private void OnValidate()
    {
        if (growthTable == null || growthTable.Length == 0) return;
        int currentCumulative = 0;
        for (int i = 0; i < growthTable.Length; i++)
        {
            growthTable[i].level = i + 1;
            currentCumulative += growthTable[i].requiredAnima;
            growthTable[i].cumulativeAnima = currentCumulative;
        }
    }
}