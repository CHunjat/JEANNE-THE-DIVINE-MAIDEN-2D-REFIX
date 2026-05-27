using UnityEngine;
using DG.Tweening; // ◀ DOTween을 쓰기 위해 반드시 추가!

public class SkillRotationManager : MonoBehaviour
{
    [Header("Skill Data (Logic)")]
    [Tooltip("기획자 아저씨가 만든 스킬 데이터 SO 3개를 넣는 곳")]
    public SkillData[] skills = new SkillData[3];

    [Header("Skill Slots (UI)")]
    [Tooltip("하이라키의 CurSkill, PreSkill, NextSkill 오브젝트들을 넣는 곳")]
    public SkillSlotUI[] skillSlots = new SkillSlotUI[3];

    [Header("Input Reader Connection")]
    [Tooltip("방금 업데이트한 UIInputReader 에셋을 연결하는 곳")]
    public UIInputReader uiInputReader;

    // 에디터에 배치된 초기 UI 원들의 좌표와 크기를 기억해둘 비밀 주머니
    private Vector3[] slotPositions = new Vector3[3];
    private Vector3[] slotScales = new Vector3[3];

    private void OnEnable()
    {
        // 🔔 인풋 리더의 R키 알림벨에 내 회전 함수(OnRotateInputPressed)를 연결!
        if (uiInputReader != null)
            uiInputReader.OnRotateSkillPressed += OnRotateInputPressed;
    }

    private void OnDisable()
    {
        // 🔕 오브젝트가 꺼질 때는 리크(오류) 방지를 위해 연결을 끊어줍니다.
        if (uiInputReader != null)
            uiInputReader.OnRotateSkillPressed -= OnRotateInputPressed;
    }

    private void Start()
    {
        // 게임이 시작되면 하이라키에 이쁘게 배치해 둔 UI 원들의 초기 '위치'와 '크기'를 먼저 기억합니다.
        for (int i = 0; i < skillSlots.Length; i++)
        {
            slotPositions[i] = skillSlots[i].transform.localPosition;
            slotScales[i] = skillSlots[i].transform.localScale;
        }

        // 시작하자마자 데이터에 맞게 이미지와 코스트 글자를 먼저 채워줍니다.
        UpdateAllSlotsUI();
    }

    // R키 알림벨이 울리면 실행되는 함수
    private void OnRotateInputPressed()
    {
        RotateSkillData();    // 1. 순환 알고리즘으로 알맹이 데이터 교체
        PlayRotationVisual(); // 2. DOTween으로 껍데기(원 오브젝트) 회전 연출
    }

    // 🔄 [순환 알고리즘] 1번은 2번으로, 2번은 3번으로, 3번은 1번으로!
    private void RotateSkillData()
    {
        // 0번(현재 메인) 데이터를 잠시 임시 주머니에 대피시킵니다.
        SkillData temp = skills[0];

        // 데이터를 한 칸씩 앞으로 밀어줍니다.
        skills[0] = skills[1]; // PreSkill 자리에 있던 데이터가 CurSkill로!
        skills[1] = skills[2]; // NextSkill 자리에 있던 데이터가 PreSkill로!
        skills[2] = temp;      // 대피시켰던 원래 CurSkill 데이터가 NextSkill로!
    }

    // ✨ DOTween 회전 연출 함수
    private void PlayRotationVisual()
    {
        for (int i = 0; i < skillSlots.Length; i++)
        {
            // R키를 연타했을 때 트윈 연출이 꼬이지 않도록 이전에 돌던 연출을 즉시 종료시킵니다.
            skillSlots[i].transform.DOKill();

            // 기억해 둔 타겟 좌표로 0.25초 동안 부드럽게 미끄러지듯 이동시킵니다. (OutQuad 곡선 적용)
            skillSlots[i].transform.DOLocalMove(slotPositions[i], 0.25f).SetEase(Ease.OutQuad);

            // 메인 위치로 이동하는 슬롯은 크기가 커지고, 서브 위치로 가는 슬롯은 작아지도록 크기도 트윈해 줍니다.
            skillSlots[i].transform.DOScale(slotScales[i], 0.25f).SetEase(Ease.OutQuad);
        }

        // ⏱️ 원들이 자리를 찾아 이동하는 시간(0.25초)이 끝나는 타이밍에 맞춰
        // 알맹이 글자와 이미지를 새 데이터로 싹 동기화(새로고침) 해줍니다!
        DOVirtual.DelayedCall(0.25f, UpdateAllSlotsUI);
    }

    // 모든 슬롯의 UI 그래픽을 현재 데이터 배열 상태로 새로고침하는 함수
    private void UpdateAllSlotsUI()
    {
        for (int i = 0; i < skillSlots.Length; i++)
        {
            skillSlots[i].UpdateSlot(skills[i]);
        }
    }
}
