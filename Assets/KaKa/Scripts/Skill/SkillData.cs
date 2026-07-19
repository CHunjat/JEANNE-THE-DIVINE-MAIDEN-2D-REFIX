using UnityEngine;

public enum SkillType { Heavy, Lightning, Heal }

[CreateAssetMenu(fileName = "SkillData", menuName = "Scriptable Objects/SkillData")]
public class SkillData : ScriptableObject
{
    public string skillName;
    public Sprite skillIcon;
    public string cost;          // 화면 표시용 (그대로 유지, "1~3" 같은 텍스트 가능)
    public string usedslot;

    public SkillType skilltype;

    [TextArea]
    public string description;

    [Header("실제 발동 데이터")]
    public AttackDataSO attackData;
    public AttackDataSO attackDataCharged;
    public HealDataSO healData;

    [Header("MP 소모 (실제 차감용, 숫자)")]
    public float mpCost;   // ★ 추가 — Heavy/Lightning/Heal 공통으로 사용
    public float mpCostChargedExtra;  // 풀차지 도달 시 추가로 더 나가는 비용 (Heavy 기준: 200)
}