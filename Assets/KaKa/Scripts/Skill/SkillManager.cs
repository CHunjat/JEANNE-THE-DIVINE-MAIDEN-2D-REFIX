using UnityEngine;
using UnityEngine.UI;

public class SkillManager : MonoBehaviour
{
    [Header("--- 아니마(스킬 재화) 및 성장 시스템 ---")]
    public int totalAnima = 0;
    public SkillData[] allSkillDatas;

    [Header("스킬 마스크 부모 오브젝트 (Skill_MaskGroup)")]
    [SerializeField] private Transform skillGroupTransform;

    [Header("플레이어 스탯 참조")]
    [SerializeField] private PlayerStats playerStats;

    [Header("중앙 정보 표시 UI")]
    [SerializeField] private Text centerTopText;
    [SerializeField] private Text centerCostText;

    private Image[] maskImages = new Image[5];
    private GameObject[] mask180Objects = new GameObject[5];

    void Start()
    {
        if (skillGroupTransform == null)
        {
            GameObject groupObj = GameObject.Find("Skill_MaskGroup") ?? GameObject.Find("SkillGroup");
            if (groupObj != null) skillGroupTransform = groupObj.transform;
        }

        if (playerStats == null)
        {
            playerStats = Object.FindFirstObjectByType<PlayerStats>();
        }

        if (skillGroupTransform != null)
        {
            for (int i = 0; i < skillGroupTransform.childCount; i++)
            {
                Transform child = skillGroupTransform.GetChild(i);
                Transform maskChild = child.Find("Skill_Mask");
                Transform mask180Child = child.Find("Skill_Mask180");

                Image targetImage = (maskChild != null) ? maskChild.GetComponent<Image>() : null;
                GameObject target180Obj = (mask180Child != null) ? mask180Child.gameObject : null;

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
        UpdateSkillMasks();
    }

    private void UpdateSkillMasks()
    {
        if (playerStats == null) return;
        float currentMp = playerStats.currentMp;

        if (skillGroupTransform != null)
        {
            bool shouldBeActive = currentMp < 500f;
            if (skillGroupTransform.gameObject.activeSelf != shouldBeActive)
            {
                skillGroupTransform.gameObject.SetActive(shouldBeActive);
            }
        }

        if (skillGroupTransform != null && !skillGroupTransform.gameObject.activeSelf) return;

        for (int i = 0; i < 5; i++)
        {
            if (maskImages[i] == null) continue;

            float minMp = i * 100f;
            float maxMp = (i + 1) * 100f;

            if (currentMp >= maxMp)
            {
                maskImages[i].fillAmount = 0f;
                if (mask180Objects[i] != null && mask180Objects[i].activeSelf)
                {
                    mask180Objects[i].SetActive(false);
                }
            }
            else
            {
                if (mask180Objects[i] != null && !mask180Objects[i].activeSelf)
                {
                    mask180Objects[i].SetActive(true);
                }

                if (currentMp <= minMp)
                {
                    maskImages[i].fillAmount = 1f;
                }
                else
                {
                    float progress = (currentMp - minMp) / 100f;
                    maskImages[i].fillAmount = 1f - progress;
                }
            }
        }
    }
}