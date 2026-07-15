using UnityEngine;
using UnityEngine.UI;

public class SkillSlot : MonoBehaviour
{
    public Image skillImage;          // 분홍 원 안의 스킬 아이콘 이미지

    // 이 슬롯에 새로운 스킬 데이터를 주입하고 UI를 갱신하는 함수
    public void UpdateSlot(SkillData data)
    {
        if (skillImage == null) return;

        if (data != null && data.skillIcon != null)
        {
            // 🔥 스킬이 등록되면 아이콘을 넣고 투명도를 255(1f)로 변경
            skillImage.sprite = data.skillIcon;

            Color c = skillImage.color;
            c.a = 1f; // 유니티 Color 코딩에서는 1f가 투명도 255를 뜻합니다.
            skillImage.color = c;
        }
        else
        {
            // 🔥 스킬이 없으면 이미지를 비우고 투명도를 0으로 만들어 흰색 사각형 방지
            skillImage.sprite = null;

            Color c = skillImage.color;
            c.a = 0f; // 투명도 0
            skillImage.color = c;
        }
    }
}