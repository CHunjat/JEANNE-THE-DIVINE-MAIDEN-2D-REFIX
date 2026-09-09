using UnityEngine;

// 엘리트 몬스터의 모든 패턴이 공통으로 상속받는 뼈대 클래스임.
public abstract class ElitePatternBase : MonoBehaviour
{
    protected EliteMonster brain;

    protected virtual void Awake()
    {
        brain = GetComponentInParent<EliteMonster>();
        if (brain == null)
        {
            Debug.LogError(gameObject.name + " 부모에서 EliteMonster를 찾을 수 없음.");
        }
    }

    // 애니메이션 이벤트가 타격 판정을 켤 때 실행할 함수임.
    public virtual void EnableHitbox()
    {
    }

    // 애니메이션 이벤트가 타격 판정을 끌 때 실행할 함수임.
    public virtual void DisableHitbox()
    {
    }
}