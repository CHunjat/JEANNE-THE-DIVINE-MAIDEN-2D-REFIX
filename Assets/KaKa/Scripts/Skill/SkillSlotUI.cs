using UnityEngine;
using UnityEngine.UI;


public class SkillSlotUI : MonoBehaviour
{
    public Image skillImage;          // 분홍 원 이미지 컴포넌트
 

    // 이 슬롯에 새로운 스킬 데이터를 주입하고 UI를 갱신하는 함수
    public void UpdateSlot(SkillData data)
    {
        if (data != null)
        {
            skillImage.sprite = data.skillIcon;
        }
    }
}
