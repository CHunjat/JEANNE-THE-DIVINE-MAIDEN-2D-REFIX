using UnityEngine;

// =====================================================
// BossAnimRelay.cs
// 애니메이션 프레임에 꽂힌 이벤트를 부모(보스 본체)로 전달하는 중계기임.
// 반드시 Animator가 있는 Visual 오브젝트에 붙여야 함.
// =====================================================
public class BossAnimRelay : MonoBehaviour
{
    // 애니메이션 창에서 이벤트를 꽂고, 파라미터(string)에 실행할 함수 이름을 적으면 됨.
    public void RelayEvent(string methodName)
    {
        // 부모 오브젝트에 있는 해당 이름의 함수를 찾아서 실행하라는 명령임.
        SendMessageUpwards(methodName, SendMessageOptions.DontRequireReceiver);
    }
}