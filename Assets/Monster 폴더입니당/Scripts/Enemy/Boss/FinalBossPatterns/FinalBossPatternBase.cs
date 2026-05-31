using UnityEngine;

// =====================================================
// FinalBossPatternBase.cs
// 데몬 누나(FinalBoss) 패턴 전용 기반 클래스임.
// BossPatternBase를 상속받고, 페이즈 구분 기능이 추가됨.
//
// [사용 방법]
// FinalBossPattern1~5 스크립트가 이 클래스를 상속받음.
// 인스펙터에서 "Is Phase2 Pattern" 체크박스로 1/2페이즈를 구분함.
// =====================================================
public abstract class FinalBossPatternBase : BossPatternBase
{
    [Header("페이즈 구분 - 2페이즈 전용 패턴이면 체크할 것")]
    [SerializeField] private bool isPhase2Pattern = false;

    // FinalBoss에서 페이즈 구분 시 사용
    public bool IsPhase2Pattern => isPhase2Pattern;
}