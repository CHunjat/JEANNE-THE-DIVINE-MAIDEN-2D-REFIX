using UnityEngine;
using System.Collections;

// MidBossPattern9.cs
// 더블 어택 2연타 신규 스크립트
public class MidBossPattern9 : BossPatternBase
{
    [SerializeField] private float hitDuration1 = 0.2f;
    [SerializeField] private float hitDuration2 = 0.2f;

    private GameObject hitBox1;
    private GameObject hitBox2;

    protected override void Awake()
    {
        base.Awake();
        MidBoss parent = GetComponent<MidBoss>();

        if (parent != null)
        {
            // 더블 어택에 사용할 히트박스를 연결해줍니다. 필요에 따라 변경 가능합니다.
            hitBox1 = parent.hitBox_Slash;
            hitBox2 = parent.hitBox_BackKick;
        }

        if (hitBox1 != null) hitBox1.SetActive(false);
        if (hitBox2 != null) hitBox2.SetActive(false);

        cooldown = 5f;
        priority = 4;
        distanceType = DistanceType.Mid;
    }

    protected override void OnExecute()
    {
        // 유니티 애니메이터에 연결된 더블 어택 트리거 이름에 맞게 변경해주세요.
        if (visualAnimator != null) visualAnimator.SetTrigger("doDoubleAttack");
    }

    // 애니메이션 첫 번째 타격 프레임에 꽂을 핀
    public void AnimEvent_DoubleHit1()
    {
        if (hitBox1 != null)
        {
            StartCoroutine(SmartHitboxRoutine(hitBox1, hitDuration1));
        }
    }

    // 애니메이션 두 번째 타격 프레임에 꽂을 핀
    public void AnimEvent_DoubleHit2()
    {
        if (hitBox2 != null)
        {
            StartCoroutine(SmartHitboxRoutine(hitBox2, hitDuration2));
        }
    }

    private IEnumerator SmartHitboxRoutine(GameObject hitbox, float duration)
    {
        EnemyHitbox hitComp = hitbox.GetComponent<EnemyHitbox>();

        if (hitbox.activeSelf && hitComp != null)
        {
            hitComp.ResetHitRecord();
        }
        else
        {
            hitbox.SetActive(true);
        }

        yield return new WaitForSeconds(duration);
        hitbox.SetActive(false);
    }
}