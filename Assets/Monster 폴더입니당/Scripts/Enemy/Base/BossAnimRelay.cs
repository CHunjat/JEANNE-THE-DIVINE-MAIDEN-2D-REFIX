using UnityEngine;


// =====================================================

// BossAnimRelay.cs (거짓말 탐지기 탑재 완료)

// 애니메이션 프레임에 꽂힌 이벤트를 부모(보스 본체)로 전달하는 중계기임.

// 반드시 Animator가 있는 Visual 오브젝트에 붙여야 함.

// =====================================================

public class BossAnimRelay : MonoBehaviour

{

    // 애니메이션 창에서 이벤트를 꽂고, 파라미터(string)에 실행할 함수 이름을 적으면 됨.

    public void RelayEvent(string methodName)

    {

        // [매복 1] 애니메이션 핀을 실제로 밟고 출발했는지 체크하는 로그임!

        Debug.Log($"<color=yellow>[릴레이 1단계]</color> 애니메이션 핀 밟음! 배달할 이름: {methodName}");


        // 부모 오브젝트에 있는 해당 이름의 함수를 찾아서 실행하라는 명령임.

        SendMessageUpwards(methodName, SendMessageOptions.DontRequireReceiver);

    }

}