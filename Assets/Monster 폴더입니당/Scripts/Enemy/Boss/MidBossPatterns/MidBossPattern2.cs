using UnityEngine;

// =====================================================
// MidBossPattern2.cs (제자리 근접 슬래시 전용으로 수정 완료)
// 장풍(Projectile) 로직 및 프리팹 빈칸 싹 다 날리고 애니메이션만 실행.
// =====================================================
public class MidBossPattern2 : BossPatternBase
{
    // 자식 오브젝트(Visual)에 있는 애니메이터를 조종하기 위한 변수.
    private Animator visualAnimator;

    private void Awake()
    {
        // 시작할 때 Visual 오브젝트에 붙어있는 Animator 컴포넌트를 찾아서 넣어둠.
        visualAnimator = GetComponentInChildren<Animator>();
    }

    // MidBoss.cs에서 이 패턴을 뽑아서 실행할 때 발동되는 함수임.
    protected override void OnExecute()
    {
        // 애니메이션 트리거("doSlashPhase2")만 딱 켜줌. 
        // 실제 히트박스가 켜지는 건 애니메이션 창에서 부른 MidBoss.cs의 AnimEvent_Slash1()이 알아서 함.
        if (visualAnimator != null)
        {
            visualAnimator.SetTrigger("doSlashPhase2");
        }
    }
}