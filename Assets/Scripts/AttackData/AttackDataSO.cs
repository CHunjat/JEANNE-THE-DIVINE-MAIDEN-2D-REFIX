using UnityEngine;

[CreateAssetMenu(fileName = "NewAttackData", menuName = "Attack/AttackData")]
public class AttackDataSO : ScriptableObject
{

    [Header("기본 세팅")]
    public string attackName;      // 예: 평타 1타
    public Vector2 size;           // 타격 범위 크기
    public Vector2 offset;         // 공격 중심점 오프셋
    public float damage;           // 데미지 수치

    [Header("타격감 (Game Feel) 요소")]
   
    public float hitStopDuration;  // 역경직(멈춤) 시간 (예: 0.05초)


    [Header("차지(기 모으기) 설정")]
    public bool canCharge = false;        // 이 공격이 기 모으기가 가능한가? (찌르기 SO에서만 체크!)
    public float chargeMultiplier = 1.5f; // 풀 차지 시 데미지 몇 배? (예: 1.5배)

}