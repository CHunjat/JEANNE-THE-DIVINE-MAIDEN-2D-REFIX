using UnityEngine;
using UnityEngine.UI; // UI Image 컴포넌트 제어를 위해 추가

public class SkillUIManager : MonoBehaviour
{
    [Header("스킬 마스크 부모 오브젝트 (Skill_MaskGroup)")]
    [SerializeField] private Transform skillGroupTransform;

    [Header("플레이어 스탯 참조")]
    [SerializeField] private PlayerStats playerStats;

    // 각 스킬 슬롯의 Skill_Mask Image 컴포넌트를 순서대로 담아둘 배열
    private Image[] maskImages = new Image[5];

    // 각 스킬 슬롯의 Skill_Mask180 GameObject를 순서대로 담아둘 배열 (새로 추가)
    private GameObject[] mask180Objects = new GameObject[5];

    void Start()
    {
        // Skill_MaskGroup 방어 코드
        if (skillGroupTransform == null)
        {
            GameObject groupObj = GameObject.Find("Skill_MaskGroup");
            if (groupObj == null) groupObj = GameObject.Find("SkillGroup");

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

        // Skill_MaskGroup 하위 자식들(Skill1~5)을 탐색하여 내부 오브젝트들을 캐싱
        if (skillGroupTransform != null)
        {
            for (int i = 0; i < skillGroupTransform.childCount; i++)
            {
                Transform child = skillGroupTransform.GetChild(i);

                // 자식 오브젝트(Skill1~5) 아래에서 각각 자식 찾기
                Transform maskChild = child.Find("Skill_Mask");
                Transform mask180Child = child.Find("Skill_Mask180");

                Image targetImage = (maskChild != null) ? maskChild.GetComponent<Image>() : null;
                GameObject target180Obj = (mask180Child != null) ? mask180Child.gameObject : null;

                // 이름에 맞춰 배열의 적절한 인덱스(0~4)에 매칭
                int index = -1;
                if (child.name.Contains("Skill1")) index = 0;
                else if (child.name.Contains("Skill2")) index = 1;
                else if (child.name.Contains("Skill3")) index = 2;
                else if (child.name.Contains("Skill4")) index = 3;
                else if (child.name.Contains("Skill5")) index = 4;

                if (index != -1)
                {
                    maskImages[index] = targetImage;
                    mask180Objects[index] = target180Obj;
                }
            }
        }

        UpdateSkillMasks();
    }

    void Update()
    {
        // 매 프레임 실시간으로 체크하여 MP 변화에 즉각 대응
        UpdateSkillMasks();
    }

    private void UpdateSkillMasks()
    {
        if (playerStats == null) return;

        // PlayerStats에서 현재 MP 값 실시간 동기화
        float currentMp = playerStats.currentMp;

        for (int i = 0; i < 5; i++)
        {
            if (maskImages[i] == null) continue;

            // 각 마스크 슬롯의 MP 범위 계산 (0~100, 100~200, ...)
            float minMp = i * 100f;
            float maxMp = (i + 1) * 100f;

            // 조건 1: MP가 이 슬롯의 최대 요구치를 채웠을 때 (스킬 활성화 상태)
            if (currentMp >= maxMp)
            {
                maskImages[i].fillAmount = 0f; // 게이지 완전히 지움

                // 아래에 있는 Skill_Mask180 오브젝트를 비활성화 (꺼줌)
                if (mask180Objects[i] != null && mask180Objects[i].activeSelf)
                {
                    mask180Objects[i].SetActive(false);
                }
            }
            // 조건 2: MP가 부족해졌을 때 (스킬 비활성화 혹은 차오르는 중인 상태)
            else
            {
                // MP가 줄어들었으므로 꺼져있던 Skill_Mask180 오브젝트를 다시 활성화 (켜줌)
                if (mask180Objects[i] != null && !mask180Objects[i].activeSelf)
                {
                    mask180Objects[i].SetActive(true);
                }

                // 현재 MP 상태에 따라 Fill Amount 값 복구 및 계산
                if (currentMp <= minMp)
                {
                    // MP가 아예 도달하지 못한 완전히 잠긴 상태 -> 마스크 가득 채움 (1.0)
                    maskImages[i].fillAmount = 1f;
                }
                else
                {
                    // MP가 다시 줄어들거나 차오르는 도중인 상태 -> 실시간 비율 계산
                    float progress = (currentMp - minMp) / 100f;
                    maskImages[i].fillAmount = 1f - progress;
                }
            }
        }
    }
}