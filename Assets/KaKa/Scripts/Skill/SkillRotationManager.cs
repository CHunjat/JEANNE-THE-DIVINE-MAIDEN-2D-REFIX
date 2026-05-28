using UnityEngine;
using DG.Tweening;

public class SkillRotationManager : MonoBehaviour
{
    [Header("Skill Data (Logic)")]
    public SkillData[] skills = new SkillData[3];

    [Header("Skill Slots (UI)")]
    public SkillSlotUI[] skillSlots = new SkillSlotUI[3];

    [Header("Input Reader Connection")]
    public UIInputReader uiInputReader;

    [Header("Curve Settings")]
    [Tooltip("곡선 연출의 크기 (값이 클수록 더 둥글게 바깥으로 휨)")]
    public float curveOffset = 80f;

    // 📌 하이라키에 배치해 둔 예쁜 초기 좌표와 크기를 기억할 배열
    private Vector2[] targetAnchorPositions = new Vector2[3];
    private Vector3[] targetScales = new Vector3[3];
    private bool isRotating = false;

    private void OnEnable()
    {
        if (uiInputReader != null)
            uiInputReader.OnRotateSkillPressed += OnRotateInputPressed;
    }

    private void OnDisable()
    {
        if (uiInputReader != null)
            uiInputReader.OnRotateSkillPressed -= OnRotateInputPressed;
    }

    private void Start()
    {
        // 1. 에디터 씬 뷰에서 이쁘게 배치한 [0]Cur, [1]Next, [2]Pre의 원래 좌표를 기억
        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (skillSlots[i] != null)
            {
                RectTransform rect = skillSlots[i].GetComponent<RectTransform>();
                if (rect != null)
                {
                    targetAnchorPositions[i] = rect.anchoredPosition;
                }
                targetScales[i] = skillSlots[i].transform.localScale;
            }
        }

        // 2. 초기 UI 새로고침
        UpdateAllSlotsUI();
    }

    private void OnRotateInputPressed()
    {
        // 데이터가 비어있거나 트윈 중이면 작동 방지 (Null 에러 차단)
        if (isRotating || skills[0] == null || skills[1] == null || skills[2] == null) return;

        isRotating = true;

        // 1. 데이터 알맹이와 슬롯 오브젝트 순서 스왑
        RotateSkillDataAndSlots();

        // 2. 둥근 곡선 트윈 연출 시작!
        PlayCurveRotationVisual();
    }

    private void RotateSkillDataAndSlots()
    {
        // [A] 데이터 순환
        SkillData tempData = skills[0];
        skills[0] = skills[1];
        skills[1] = skills[2];
        skills[2] = tempData;

        // [B] UI 오브젝트 순환
        SkillSlotUI tempSlot = skillSlots[0];
        skillSlots[0] = skillSlots[1];
        skillSlots[1] = skillSlots[2];
        skillSlots[2] = tempSlot;
    }

    private void PlayCurveRotationVisual()
    {
        float duration = 0.35f;

        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (skillSlots[i] == null) continue;

            RectTransform rect = skillSlots[i].GetComponent<RectTransform>();
            if (rect == null) continue;

            skillSlots[i].transform.DOKill();

            // 🎯 삼각함수 하이브리드 방식: 
            // 시작점(현재 위치)에서 목적지(targetAnchorPositions[i])로 직선 이동하되,
            // 중간 경로에 Sin 함수를 섞어 바깥쪽으로 불룩하게 호(Arc)를 그리게 만듭니다.
            Vector2 startPos = rect.anchoredPosition;
            Vector2 endPos = targetAnchorPositions[i];

            DOTween.To(() => 0f, t => {
                // t는 0에서 1까지 흐르는 시간 비율
                // 직선 보간 위치 계산
                Vector2 currentLinearPos = Vector2.Lerp(startPos, endPos, t);

                // 🔥 [삼각함수 구간] 호를 그리기 위한 Sin 오프셋 계산 (0도 ~ 180도)
                // 이동하는 도중 정중앙(t=0.5)일 때 가장 바깥쪽으로 불룩해집니다.
                float sinOffset = Mathf.Sin(t * Mathf.PI) * curveOffset;

                // 왼쪽 방향으로 볼록한 반원을 그리도록 X축에 오프셋 가산
                rect.anchoredPosition = new Vector2(currentLinearPos.x - sinOffset, currentLinearPos.y);

            }, 1f, duration).SetEase(Ease.OutQuad);

            // 크기도 자연스럽게 타겟 크기로 조절
            skillSlots[i].transform.DOScale(targetScales[i], duration).SetEase(Ease.OutQuad);
        }

        // 연출 완료 후 데이터 동기화 및 락 해제
        DOVirtual.DelayedCall(duration, () => {
            UpdateAllSlotsUI();
            isRotating = false;
        });
    }

    private void UpdateAllSlotsUI()
    {
        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (skillSlots[i] != null && skills[i] != null)
            {
                skillSlots[i].UpdateSlot(skills[i]);
            }
        }
    }
}