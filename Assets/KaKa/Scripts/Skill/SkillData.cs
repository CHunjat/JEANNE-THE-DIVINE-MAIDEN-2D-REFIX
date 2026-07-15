using UnityEngine;

public enum SkillType { Heavy, Lightning, Heal }

[CreateAssetMenu(fileName = "SkillData", menuName = "Scriptable Objects/SkillData")]
public class SkillData : ScriptableObject
{
    public string skillName;
    public Sprite skillIcon;
    public string cost;
    public string usedslot;

    public SkillType skilltype;

    [TextArea]
    public string description;

    [Header("실제 발동 데이터")]
    public AttackDataSO attackData;         // Heavy 1단계 / Lightning 용
    public AttackDataSO attackDataCharged;  // Heavy 풀차지(2단계) 전용 - 없으면 attackData로 대체
    public HealDataSO healData;             // Heal 전용
}