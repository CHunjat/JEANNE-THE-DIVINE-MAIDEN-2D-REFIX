using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "Scriptable Objects/SkillData")]
public class SkillData : ScriptableObject
{
    public string skillName;     // 스킬 이름
    public Sprite skillIcon;     // 분홍 원에 들어갈 스킬 이미지
    public int cost;             // 빨간 사각형에 들어갈 코스트 비용
}
