using UnityEngine;
using DG.Tweening; // ◀ DOTween을 쓰기 위해 반드시 추가!

public class SkillRotationManager : MonoBehaviour
{
    [Header("Skill Data (Logic)")]
    public SkillData[] skills = new SkillData[3];

    [Header("Skill Slots (UI)")]
    public SkillSlotUI[] skillSlots = new SkillSlotUI[3];

    [Header("UI Positions for Rotation")]
    // 에디터에서 분홍 원들의 초기 '위치 좌표' 세 군데를 기억해둘 변수입니다.
    private Vector3[] slotPositions = new Vector3[3];
    private Vector3[] slotScales = new Vector3[3];

    [Header("Input Reader Connection")]
    public UIInputReader uiInputReader;

    private void OnEnable()
    {
        if (uiInputReader != null) uiInputReader.OnPausePressed += OnRotateInputPressed;
    }

    private void OnDisable()
    {
        if (uiInputReader != null) uiInputReader.OnPausePressed -= OnRotateInputPressed;
    }

    private void Start()
    {
        // 1. 게임이 시작되면, 현재 배치된 UI 슬롯들의 기본 위치와 크기(스케일)를 기억해둡니다.
        for (int i = 0; i < skillSlots.Length; i++)
        {
            slotPositions[i] = skillSlots[i].transform.localPosition;
            slotScales[i] = skillSlots[i].transform.localScale;
        }

        UpdateAllSlotsUI();
    }

    private void OnRotateInputPressed()
    {
        RotateSkillData();    // 1. 데이터 스왑 (1->2->3->1)
        PlayRotationVisual(); // 2. DOTween으로 부드러운 위치 회전 연출!
    }

    private void RotateSkillData()
    {
        SkillData temp = skills[0];
        skills[0] = skills[1];
        skills[1] = skills[2];
        skills[2] = temp;
    }

    // ✨ DOTween으로 원들이 살아 움직이게 만드는 연출 함수
    private void PlayRotationVisual()
    {
        // 모든 슬롯의 움직임 연산을 초기화하고 연출을 시작합니다.
        for (int i = 0; i < skillSlots.Length; i++)
        {
            // 혹시 이전 연출이 끝나기 전에 R을 연타했을 때 연출이 꼬이지 않도록 방지합니다.
            skillSlots[i].transform.DOKill();

            // 데이터 스왑에 의해 이미 슬롯들이 가져야할 인덱스 위치가 정해졌습니다.
            // DOMoveLocal을 사용하여 0.25초 동안 부드럽게 지정된 좌표로 이동시킵니다!
            skillSlots[i].transform.DOLocalMove(slotPositions[i], 0.25f).SetEase(Ease.OutQuad);

            // 보너스 연출: 메인 스킬 위치(0번)로 오는 원은 크기를 키우고, 서브 위치는 작게 만듭니다.
            skillSlots[i].transform.DOScale(slotScales[i], 0.25f).SetEase(Ease.OutQuad);
        }

        // 이동 애니메이션이 끝나는 시점인 0.25초 뒤에 
        // 바뀐 이미지(스프라이트)와 코스트 텍스트를 한 번에 동기화해 줍니다.
        DOVirtual.DelayedCall(0.25f, UpdateAllSlotsUI);
    }

    private void UpdateAllSlotsUI()
    {
        for (int i = 0; i < skillSlots.Length; i++)
        {
            skillSlots[i].UpdateSlot(skills[i]);
        }
    }
}
