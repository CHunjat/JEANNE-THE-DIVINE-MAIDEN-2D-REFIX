
using UnityEngine;

public class MidBossWebProjectile : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private Transform visual;
    [SerializeField] private bool visualFacesRight = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugLog = true;

    private float speed;
    private float maxRange;
    private float bindDuration;

    private Vector2 startPos;
    private Vector2 currentDir;

    private Animator anim;
    private SpriteRenderer visualRenderer;

    private Vector3 originalVisualScale;
    private bool isHit;
    private bool facingLeft;

    private void Awake()
    {
        // Visual 오브젝트 자동 검색
        if (visual == null)
        {
            Transform[] children =
                GetComponentsInChildren<Transform>(true);

            foreach (Transform child in children)
            {
                if (child.name == "Visual")
                {
                    visual = child;
                    break;
                }
            }
        }

        if (visual != null)
        {
            originalVisualScale = visual.localScale;

            visualRenderer =
                visual.GetComponent<SpriteRenderer>();

            anim = visual.GetComponent<Animator>();
        }
        else
        {
            Debug.LogError(
                "[MidBossWebProjectile] Visual을 찾지 못했습니다.",
                this
            );
        }
    }

    public void Initialize(
        Vector2 dir,
        float spd,
        float range,
        float bind)
    {
        speed = spd;
        maxRange = range;
        bindDuration = bind;

        startPos = transform.position;
        currentDir = dir.normalized;

        isHit = false;
        facingLeft = currentDir.x < 0f;

        // Visual만 방향 반전
        ApplyVisualDirection();

        if (showDebugLog)
        {
            Debug.Log(
                "[MidBossWebProjectile] 발사 방향: " +
                (facingLeft ? "왼쪽" : "오른쪽") +
                " / 이동 벡터: " + currentDir +
                " / 부모 회전: " + transform.eulerAngles,
                this
            );
        }
    }

    private void ApplyVisualDirection()
    {
        if (visual == null)
            return;

        // 기본 스프라이트 방향에 따라 반전 여부 결정
        bool shouldFlip =
            visualFacesRight ? facingLeft : !facingLeft;

        Vector3 newScale = originalVisualScale;

        newScale.x =
            Mathf.Abs(originalVisualScale.x) *
            (shouldFlip ? -1f : 1f);

        visual.localScale = newScale;

        // 이전 방식과 중복 반전 방지
        if (visualRenderer != null)
        {
            visualRenderer.flipX = false;
        }
    }

    private void LateUpdate()
    {
        // Animator가 반전 값을 변경하는 경우 대비
        if (visual != null)
        {
            ApplyVisualDirection();
        }
    }

    private void Update()
    {
        if (isHit)
            return;

        transform.Translate(
            currentDir * speed * Time.deltaTime,
            Space.World
        );

        float traveledDistance =
            Vector2.Distance(startPos, transform.position);

        if (traveledDistance >= maxRange)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isHit)
            return;

        if (other.gameObject.layer !=
            LayerMask.NameToLayer("Player"))
        {
            return;
        }

        isHit = true;
        speed = 0f;

        if (showDebugLog)
        {
            Debug.Log(
                "[MidBossWebProjectile] 플레이어 충돌 감지",
                this
            );
        }

        if (anim != null && HasParameter(anim, "doSpread"))
        {
            anim.SetTrigger("doSpread");
        }

        Destroy(gameObject, 0.4f);
    }

    private bool HasParameter(
        Animator animator,
        string parameterName)
    {
        foreach (
            AnimatorControllerParameter parameter
            in animator.parameters)
        {
            if (parameter.name == parameterName)
            {
                return true;
            }
        }

        return false;
    }
}
