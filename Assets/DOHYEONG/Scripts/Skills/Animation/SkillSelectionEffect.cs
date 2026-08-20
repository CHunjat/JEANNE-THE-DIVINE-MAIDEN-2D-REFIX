using DG.Tweening;
using UnityEngine;

public class SkillSelectionEffect : MonoBehaviour
{
    [Header("Border")]
    [SerializeField] private RectTransform top;
    [SerializeField] private RectTransform right;
    [SerializeField] private RectTransform bottom;
    [SerializeField] private RectTransform left;

    [Header("Moving Line")]
    [SerializeField] private RectTransform runner;

    [Header("Settings")]
    [SerializeField] private float drawDuration = 0.08f;
    [SerializeField] private float runnerLoopDuration = 2.2f;
    [SerializeField] private float runnerLength = 40f;
    [SerializeField] private float cornerRadius = 12f;

    private RectTransform rectTransform;

    private Sequence drawSequence;
    private Tween runnerTween;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Show()
    {
        gameObject.SetActive(true);

        drawSequence?.Kill();
        runnerTween?.Kill();

        ResetBorder();

        PlayDrawAnimation();
    }

    public void Hide()
    {
        drawSequence?.Kill();
        runnerTween?.Kill();

        gameObject.SetActive(false);
    }

    private void ResetBorder()
    {
        if (top != null)
            top.localScale = new Vector3(0f, 1f, 1f);

        if (right != null)
            right.localScale = new Vector3(1f, 0f, 1f);

        if (bottom != null)
            bottom.localScale = new Vector3(0f, 1f, 1f);

        if (left != null)
            left.localScale = new Vector3(1f, 0f, 1f);

        if (runner != null)
            runner.gameObject.SetActive(false);
    }

    private void PlayDrawAnimation()
    {
        drawSequence = DOTween.Sequence()
            .SetUpdate(true);

        // 위
        if (top != null)
        {
            drawSequence.Append(
                top.DOScaleX(1f, drawDuration)
                    .SetEase(Ease.Linear)
            );
        }

        // 오른쪽
        if (right != null)
        {
            drawSequence.Append(
                right.DOScaleY(1f, drawDuration)
                    .SetEase(Ease.Linear)
            );
        }

        // 아래
        if (bottom != null)
        {
            drawSequence.Append(
                bottom.DOScaleX(1f, drawDuration)
                    .SetEase(Ease.Linear)
            );
        }

        // 왼쪽
        if (left != null)
        {
            drawSequence.Append(
                left.DOScaleY(1f, drawDuration)
                    .SetEase(Ease.Linear)
            );
        }

        drawSequence.OnComplete(StartRunnerLoop);
    }

    private void StartRunnerLoop()
    {
        if (runner == null || rectTransform == null)
            return;

        runner.gameObject.SetActive(true);

        runnerTween?.Kill();

        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;

        // 너무 큰 Radius 방지
        float radius = Mathf.Clamp(
            cornerRadius,
            0f,
            Mathf.Min(width, height) * 0.5f
        );

        // Runner 자체는 계속 가로선 상태 유지
        runner.sizeDelta = new Vector2(runnerLength, 2f);

        float progress = 0f;

        runnerTween = DOTween.To(
            () => progress,
            value =>
            {
                progress = value;

                EvaluateRoundedRect(
                    progress,
                    width,
                    height,
                    radius,
                    out Vector2 position,
                    out float angle
                );

                runner.anchoredPosition = position;

                // 진행 방향을 따라 자연스럽게 회전
                runner.localRotation =
                    Quaternion.Euler(0f, 0f, angle);
            },
            1f,
            runnerLoopDuration
        )
        .SetEase(Ease.Linear)
        .SetLoops(-1, LoopType.Restart)
        .SetUpdate(true);
    }

    private void EvaluateRoundedRect(
    float t,
    float width,
    float height,
    float radius,
    out Vector2 position,
    out float angle)
    {
        float halfW = width * 0.5f;
        float halfH = height * 0.5f;

        float horizontalLength =
            Mathf.Max(0f, width - radius * 2f);

        float verticalLength =
            Mathf.Max(0f, height - radius * 2f);

        // 90도 원호 길이
        float arcLength =
            Mathf.PI * radius * 0.5f;

        float totalLength =
            horizontalLength * 2f +
            verticalLength * 2f +
            arcLength * 4f;

        float distance =
            Mathf.Repeat(t, 1f) * totalLength;


        // =================================
        // 1. 위쪽 직선
        // =================================

        if (distance < horizontalLength)
        {
            float p = distance / horizontalLength;

            position = new Vector2(
                Mathf.Lerp(
                    -halfW + radius,
                    halfW - radius,
                    p
                ),
                halfH
            );

            angle = 0f;
            return;
        }

        distance -= horizontalLength;


        // =================================
        // 2. 오른쪽 위 코너
        // =================================

        if (distance < arcLength)
        {
            float p = distance / arcLength;

            float theta =
                Mathf.Lerp(90f, 0f, p) *
                Mathf.Deg2Rad;

            Vector2 center =
                new Vector2(
                    halfW - radius,
                    halfH - radius
                );

            position =
                center +
                new Vector2(
                    Mathf.Cos(theta),
                    Mathf.Sin(theta)
                ) * radius;

            angle = Mathf.Lerp(0f, -90f, p);
            return;
        }

        distance -= arcLength;


        // =================================
        // 3. 오른쪽 직선
        // =================================

        if (distance < verticalLength)
        {
            float p = distance / verticalLength;

            position = new Vector2(
                halfW,
                Mathf.Lerp(
                    halfH - radius,
                    -halfH + radius,
                    p
                )
            );

            angle = -90f;
            return;
        }

        distance -= verticalLength;


        // =================================
        // 4. 오른쪽 아래 코너
        // =================================

        if (distance < arcLength)
        {
            float p = distance / arcLength;

            float theta =
                Mathf.Lerp(0f, -90f, p) *
                Mathf.Deg2Rad;

            Vector2 center =
                new Vector2(
                    halfW - radius,
                    -halfH + radius
                );

            position =
                center +
                new Vector2(
                    Mathf.Cos(theta),
                    Mathf.Sin(theta)
                ) * radius;

            angle = Mathf.Lerp(-90f, -180f, p);
            return;
        }

        distance -= arcLength;


        // =================================
        // 5. 아래쪽 직선
        // =================================

        if (distance < horizontalLength)
        {
            float p = distance / horizontalLength;

            position = new Vector2(
                Mathf.Lerp(
                    halfW - radius,
                    -halfW + radius,
                    p
                ),
                -halfH
            );

            angle = -180f;
            return;
        }

        distance -= horizontalLength;


        // =================================
        // 6. 왼쪽 아래 코너
        // =================================

        if (distance < arcLength)
        {
            float p = distance / arcLength;

            float theta =
                Mathf.Lerp(-90f, -180f, p) *
                Mathf.Deg2Rad;

            Vector2 center =
                new Vector2(
                    -halfW + radius,
                    -halfH + radius
                );

            position =
                center +
                new Vector2(
                    Mathf.Cos(theta),
                    Mathf.Sin(theta)
                ) * radius;

            angle = Mathf.Lerp(-180f, -270f, p);
            return;
        }

        distance -= arcLength;


        // =================================
        // 7. 왼쪽 직선
        // =================================

        if (distance < verticalLength)
        {
            float p = distance / verticalLength;

            position = new Vector2(
                -halfW,
                Mathf.Lerp(
                    -halfH + radius,
                    halfH - radius,
                    p
                )
            );

            angle = -270f;
            return;
        }

        distance -= verticalLength;


        // =================================
        // 8. 왼쪽 위 코너
        // =================================

        {
            float p = distance / arcLength;

            float theta =
                Mathf.Lerp(180f, 90f, p) *
                Mathf.Deg2Rad;

            Vector2 center =
                new Vector2(
                    -halfW + radius,
                    halfH - radius
                );

            position =
                center +
                new Vector2(
                    Mathf.Cos(theta),
                    Mathf.Sin(theta)
                ) * radius;

            angle = Mathf.Lerp(-270f, -360f, p);
        }
    }
    private void OnDisable()
    {
        drawSequence?.Kill();
        runnerTween?.Kill();
    }
}