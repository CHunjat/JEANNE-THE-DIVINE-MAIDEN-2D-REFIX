using UnityEngine;

// Visual 오브젝트에 부착하여 애니메이션 이벤트 신호를 본체 및 패턴에 전달함.
public class EliteAnimRelay : MonoBehaviour
{
    private EliteMonster brain;

    [Header("패턴 스크립트 연결")]
    public ElitePatternBase pattern1;
    public ElitePatternBase pattern2;
    public ElitePatternBase pattern3;

    private void Awake()
    {
        brain = GetComponentInParent<EliteMonster>();
    }

    // 타격 판정이 시작되는 순간 호출할 애니메이션 이벤트 함수임.
    public void OnHitboxEnable(int patternIndex)
    {
        if (patternIndex == 1 && pattern1 != null) pattern1.EnableHitbox();
        else if (patternIndex == 2 && pattern2 != null) pattern2.EnableHitbox();
        else if (patternIndex == 3 && pattern3 != null) pattern3.EnableHitbox();
    }

    // 타격 판정이 끝나는 순간 호출할 애니메이션 이벤트 함수임.
    public void OnHitboxDisable(int patternIndex)
    {
        if (patternIndex == 1 && pattern1 != null) pattern1.DisableHitbox();
        else if (patternIndex == 2 && pattern2 != null) pattern2.DisableHitbox();
        else if (patternIndex == 3 && pattern3 != null) pattern3.DisableHitbox();
    }

    // 공격 모션이 완전히 종료되었을 때 호출할 애니메이션 이벤트 함수임.
    public void OnAttackFinish()
    {
        if (brain != null)
        {
            brain.EndAttack();
        }
    }
}