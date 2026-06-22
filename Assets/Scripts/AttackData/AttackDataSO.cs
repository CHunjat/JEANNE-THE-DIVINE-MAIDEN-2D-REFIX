using UnityEngine;

[CreateAssetMenu(fileName = "NewAttackData", menuName = "Attack/AttackData")]
public class AttackDataSO : ScriptableObject
{
    public string attackName;      // 예: 평타 1타
    public Vector2 size;           // 타격 범위 크기
    public Vector2 offset;         // 공격 중심점 오프셋
    public float damage;           // 데미지 수치
}