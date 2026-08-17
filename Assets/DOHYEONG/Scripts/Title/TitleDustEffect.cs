using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TitleDustEffect : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private RectTransform dustArea;
    [SerializeField] private GameObject dustPrefab;

    [Header("먼지 개수")]
    [SerializeField] private int particleCount = 18;

    [Header("크기")]
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 1.3f;

    [Header("밝기")]
    [SerializeField] private float minAlpha = 0.08f;
    [SerializeField] private float maxAlpha = 0.28f;

    [Header("이동 거리")]
    [SerializeField] private float minMoveY = 30f;
    [SerializeField] private float maxMoveY = 90f;
    [SerializeField] private float moveX = 25f;

    [Header("이동 시간")]
    [SerializeField] private float minDuration = 5f;
    [SerializeField] private float maxDuration = 9f;

    [Header("처음 랜덤 대기")]
    [SerializeField] private float maxStartDelay = 4f;

    [Header("반복 사이 대기")]
    [SerializeField] private float minRepeatDelay = 0.2f;
    [SerializeField] private float maxRepeatDelay = 1.5f;

    private readonly List<RectTransform> particleRects
        = new List<RectTransform>();

    private readonly List<CanvasGroup> particleGroups
        = new List<CanvasGroup>();

    // ★ 각 먼지가 사용 중인 Sequence를 직접 보관
    private readonly List<Sequence> particleSequences
        = new List<Sequence>();

    private bool initialized;

    private void OnEnable()
    {
        if (!initialized)
        {
            CreateParticles();
            initialized = true;
        }

        StartParticles();
    }

    private void CreateParticles()
    {
        if (dustArea == null || dustPrefab == null)
        {
            Debug.LogWarning("[TitleDustEffect] DustArea 또는 DustPrefab이 없습니다.");
            return;
        }

        for (int i = 0; i < particleCount; i++)
        {
            GameObject obj = Instantiate(
                dustPrefab,
                dustArea
            );

            RectTransform rect =
                obj.GetComponent<RectTransform>();

            CanvasGroup group =
                obj.GetComponent<CanvasGroup>();

            if (group == null)
                group = obj.AddComponent<CanvasGroup>();

            group.alpha = 0f;

            particleRects.Add(rect);
            particleGroups.Add(group);
        }
    }

    private void StartParticles()
    {
        // 혹시 기존 Sequence가 남아 있다면 먼저 제거
        KillAllSequences();

        if (dustArea == null)
            return;

        for (int i = 0; i < particleRects.Count; i++)
        {
            CreateParticleSequence(i);
        }
    }

    private void CreateParticleSequence(int index)
    {
        if (dustArea == null)
            return;

        if (index < 0 || index >= particleRects.Count)
            return;

        RectTransform rect = particleRects[index];
        CanvasGroup group = particleGroups[index];

        if (rect == null || group == null)
            return;

        Rect areaRect = dustArea.rect;

        // =========================
        // 랜덤 시작 위치
        // =========================

        Vector2 startPos = new Vector2(
            Random.Range(areaRect.xMin, areaRect.xMax),
            Random.Range(areaRect.yMin, areaRect.yMax)
        );

        float targetAlpha = Random.Range(
            minAlpha,
            maxAlpha
        );

        float scale = Random.Range(
            minScale,
            maxScale
        );

        float startDelay = Random.Range(
            0f,
            maxStartDelay
        );

        // 먼지 하나의 전체 이동 시간
        float totalDuration = Random.Range(
            minDuration,
            maxDuration
        );

        // =========================
        // 자유로운 중간 이동 지점
        // =========================

        Vector2 point1 = GetRandomNearbyPoint(
            startPos,
            areaRect,
            50f,
            35f
        );

        Vector2 point2 = GetRandomNearbyPoint(
            point1,
            areaRect,
            65f,
            45f
        );

        Vector2 point3 = GetRandomNearbyPoint(
            point2,
            areaRect,
            55f,
            40f
        );

        Vector2 point4 = GetRandomNearbyPoint(
            point3,
            areaRect,
            70f,
            50f
        );

        // =========================
        // 초기 상태
        // =========================

        rect.anchoredPosition = startPos;
        rect.localScale = Vector3.one * scale;

        group.alpha = 0f;

        // 이동 시간을 4구간으로 나눔
        float segmentDuration = totalDuration / 4f;

        Sequence sequence = DOTween.Sequence();

        sequence.AppendInterval(startDelay);

        // =========================
        // 천천히 등장
        // =========================

        sequence.Append(
            group
                .DOFade(targetAlpha, 1.2f)
                .SetEase(Ease.InOutSine)
        );

        // =========================
        // 자유롭게 부유
        // =========================

        sequence.Append(
            rect
                .DOAnchorPos(point1, segmentDuration)
                .SetEase(Ease.InOutSine)
        );

        sequence.Append(
            rect
                .DOAnchorPos(point2, segmentDuration)
                .SetEase(Ease.InOutSine)
        );

        sequence.Append(
            rect
                .DOAnchorPos(point3, segmentDuration)
                .SetEase(Ease.InOutSine)
        );

        sequence.Append(
            rect
                .DOAnchorPos(point4, segmentDuration)
                .SetEase(Ease.InOutSine)
        );

        // =========================
        // 천천히 사라짐
        // =========================

        sequence.Append(
            group
                .DOFade(0f, 1.2f)
                .SetEase(Ease.InOutSine)
        );

        // =========================
        // 반복
        // =========================

        sequence.SetLoops(
            -1,
            LoopType.Restart
        );

        sequence.SetUpdate(true);

        sequence.SetLink(
            gameObject,
            LinkBehaviour.KillOnDisable
        );

        particleSequences.Add(sequence);
    }

    private Vector2 GetRandomNearbyPoint(
    Vector2 currentPosition,
    Rect areaRect,
    float maxMoveX,
    float maxMoveY)
    {
        Vector2 newPosition =
            currentPosition +
            new Vector2(
                Random.Range(-maxMoveX, maxMoveX),
                Random.Range(-maxMoveY, maxMoveY)
            );

        // DustArea 밖으로 너무 많이 나가지 않도록 제한
        newPosition.x = Mathf.Clamp(
            newPosition.x,
            areaRect.xMin,
            areaRect.xMax
        );

        newPosition.y = Mathf.Clamp(
            newPosition.y,
            areaRect.yMin,
            areaRect.yMax
        );

        return newPosition;
    }

    private void KillAllSequences()
    {
        for (int i = 0; i < particleSequences.Count; i++)
        {
            Sequence sequence = particleSequences[i];

            if (sequence != null && sequence.IsActive())
            {
                sequence.Kill(false);
            }
        }

        particleSequences.Clear();
    }

    private void OnDisable()
    {
        // ★ 핵심
        // MainScreen이 꺼지는 순간 모든 Sequence 완전히 제거
        KillAllSequences();

        // 혹시 남은 Tween까지 방어적으로 제거
        for (int i = 0; i < particleRects.Count; i++)
        {
            if (particleRects[i] != null)
                particleRects[i].DOKill(false);

            if (particleGroups[i] != null)
                particleGroups[i].DOKill(false);
        }
    }

    private void OnDestroy()
    {
        // ★ 씬 재로드/오브젝트 파괴 시 한 번 더 확실하게 제거
        KillAllSequences();
    }
}