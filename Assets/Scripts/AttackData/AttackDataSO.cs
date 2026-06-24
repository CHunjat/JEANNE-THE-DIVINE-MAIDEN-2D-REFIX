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
    public float knockbackForce;   // 넉백(밀쳐내기) 강도
    public float hitStopDuration;  // 역경직(멈춤) 시간 (예: 0.05초)
    public float cameraShakeIntensity; // 카메라 흔들림 강도 (0이면 안 흔들림)

    [Header("시각 & 청각 효과")]
    public GameObject attackEffectPrefab; // 타격 성공 시 터질 파티클(VFX)
    public AudioClip attackSFX;           // 타격 성공 시 재생될 사운드(SFX)
    [Range(0f, 1f)]
    public float sfxVolume = 1f;
}