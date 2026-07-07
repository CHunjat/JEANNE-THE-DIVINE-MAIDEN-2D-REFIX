using UnityEngine;

[CreateAssetMenu(fileName = "NewStatBalance", menuName = "Stats/StatBalanceData")]
public class StatBalanceSO : ScriptableObject
{
    [Header("100% 기준 수치 (기획자가 조절할 값)")]

    [Tooltip("공격력 100% 기준치 (예: 15를 넣으면 100%가 15로 계산됨)")]
    public float baseAttackPerStat = 15f;

    [Tooltip("방어력 100% 기준치 (예: 10)")]
    public float baseDefensePerStat = 10f;

    [Tooltip("최대 체력 100% 기준치 (예: 20)")]
    public float baseHpPerStat = 20f;

    [Tooltip("스킬 에너지(MP) 100% 기준치 (예: 10)")]
    public float baseMpPerStat = 10f;
}