using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillSlotUI : MonoBehaviour
{
    public Image skillImage;          // 분홍 원 이미지 컴포넌트
    public TextMeshProUGUI costText;  // 빨간 사각형 안의 코스트 텍스트

    // 이 슬롯에 새로운 스킬 데이터를 주입하고 UI를 갱신하는 함수
    public void UpdateSlot(SkillData data)
    {
        if (data != null)
        {
            skillImage.sprite = data.skillIcon;
            costText.text = data.cost.ToString();
        }
    }
}
