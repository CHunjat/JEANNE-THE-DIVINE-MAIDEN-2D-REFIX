using UnityEngine;

public class SkillUIManager : MonoBehaviour
{
    [Header("스킬 부모 오브젝트 (SkillGroup)")]
    [SerializeField] private Transform skillGroupTransform;

    [Header("플레이어 스탯 참조")]
    [SerializeField] private PlayerStats playerStats;

    // 각 스킬 오브젝트들을 순서대로 담아둘 배열 (Skill1 ~ Skill5)
    private GameObject[] skillObjects = new GameObject[5];

    void Start()
    {
        // SkillGroup 방어 코드
        if (skillGroupTransform == null)
        {
            GameObject groupObj = GameObject.Find("SkillGroup");
            if (groupObj != null)
            {
                skillGroupTransform = groupObj.transform;
            }
        }

        // 플레이어 스탯 자동 검색
        if (playerStats == null)
        {
            playerStats = Object.FindFirstObjectByType<PlayerStats>();
        }

        // SkillGroup의 자식들을 탐색해서 Skill1~5 오브젝트를 배열에 순서대로 캐싱
        if (skillGroupTransform != null)
        {
            for (int i = 0; i < skillGroupTransform.childCount; i++)
            {
                Transform child = skillGroupTransform.GetChild(i);
                if (child.name.Contains("Skill1")) skillObjects[0] = child.gameObject;
                else if (child.name.Contains("Skill2")) skillObjects[1] = child.gameObject;
                else if (child.name.Contains("Skill3")) skillObjects[2] = child.gameObject;
                else if (child.name.Contains("Skill4")) skillObjects[3] = child.gameObject;
                else if (child.name.Contains("Skill5")) skillObjects[4] = child.gameObject;
            }
        }

        UpdateSkillSlots();
    }

    void Update()
    {
        // 매 프레임 MP를 체크하여 실시간으로 조건에 맞는 스킬 활성화
        UpdateSkillSlots();
    }

    private void UpdateSkillSlots()
    {
        if (playerStats == null) return;

        float currentMp = playerStats.currentMp;

        // 1. 모든 스킬을 우선 비활성화 상태(false)로 셋팅할 변수들 준비
        bool s1 = false;
        bool s2 = false;
        bool s3 = false;
        bool s4 = false;
        bool s5 = false;

        // 2. MP 조건에 따른 활성화 여부 판정 (요청하신 조건 적용)
        if (currentMp == 0)
        {
            s1 = true;
        }

        if (currentMp <= 100f)
        {
            s2 = true;
        }

        if (currentMp <= 200f)
        {
            s3 = true;
        }

        if (currentMp <= 300f)
        {
            s4 = true;
        }

        if (currentMp <= 400f)
        {
            s5 = true;
        }

        // 3. 실제 오브젝트들의 SetActive 상태를 판정 결과에 맞게 일괄 변경
        SetSkillActive(0, s1); // Skill1
        SetSkillActive(1, s2); // Skill2
        SetSkillActive(2, s3); // Skill3
        SetSkillActive(3, s4); // Skill4
        SetSkillActive(4, s5); // Skill5
    }

    // 오브젝트가 존재할 때만 안전하게 SetActive를 켜고 끄는 함수
    private void SetSkillActive(int index, bool isActive)
    {
        if (skillObjects[index] != null)
        {
            // 매 프레임 중복 셋팅 방지
            if (skillObjects[index].activeSelf != isActive)
            {
                skillObjects[index].SetActive(isActive);
            }
        }
    }
}