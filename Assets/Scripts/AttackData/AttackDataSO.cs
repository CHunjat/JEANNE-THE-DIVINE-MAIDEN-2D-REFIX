using UnityEngine;

// 기획자가 인스펙터에서 공격 종류를 선택할 수 있게 해주는 카테고리
public enum AttackCategory
{
    Light,              // 일반 약공격
    Heavy,              // 일반 강공격
    JumpLight,          // 점프 약공격
    JumpHeavy,          // 점프 강공격
    ParryCounterLight,  // 패리 카운터 약공격
    ParryCounterHeavy   // 패리 카운터 강공격
}

[CreateAssetMenu(fileName = "NewAttackData", menuName = "Attack/AttackData")]
public class AttackDataSO : ScriptableObject
{
    [Header("기본 세팅")]
    public string attackName;             // 예: 평타 1타
    public AttackCategory attackCategory; // [핵심] 이 공격이 어떤 타입인지 인스펙터에서 선택!
    public Vector2 size;                  // 타격 범위 크기
    public Vector2 offset;                // 공격 중심점 오프셋
    public float damageMultiplier;                  // 기본 데미지 수치

    [Header("타격감 (Game Feel) 요소")]
    public float hitStopDuration;         // 역경직(멈춤) 시간 (예: 0.05초)


    // =========================================================
    // 그로기 및 패리 카운터 시스템 (기획자 조절 영역)
    // =========================================================

    [Header("지누게가 설정할 그로기 수치비율, 카테고리 선택후 그에 맞는 수치만 입력하면됨")]
    [Header("일반 및 점프 공격 그로기 반영 비율")]
    [Tooltip("일반 약공격 그로기 공격력 반영비율")]
    public float lightAttackGroggyRatio = 1.0f;

    [Tooltip("일반 강공격 그로기 공격력 반영비율")]
    public float heavyAttackGroggyRatio = 1.5f;

    [Tooltip("일반 강공차지 그로기 공격력 반영비율")]
    public float heavyChargeGroggyRatio = 2.0f;

    [Tooltip("점프 약공격 그로기 반영비율")]
    public float jumpLightGroggyRatio = 1.2f;

    [Tooltip("점프 강공격 그로기 반영비율")]
    public float jumpHeavyGroggyRatio = 1.8f;

    [Header("패리 카운터 특수 설정")]
    [Tooltip("패리 카운터 발동 시 공격속도 증가율 (예: 1.2 입력 시 20% 빨라짐)")]
    public float parryCounterSpeedRatio = 1.2f;

    [Tooltip("패리 카운터 약공격 그로기 반영비율")]
    public float parryCounterLightGroggyRatio = 2.5f;

    [Tooltip("패리 카운터 강공격 그로기 반영 비율")]
    public float parryCounterHeavyGroggyRatio = 3.5f;
}