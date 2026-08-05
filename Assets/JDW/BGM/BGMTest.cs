using UnityEngine;

/// <summary>
/// BGM 전투 전환 테스트용 임시 스크립트.
/// 실제 보스/컷씬 로직 연결 전에 BGMManager 동작만 확인하는 용도.
/// 테스트 끝나면 오브젝트에서 제거하거나 비활성화하면 됨.
/// </summary>
public class BGMTest : MonoBehaviour
{
    [Header("테스트 키 안내")]
    [TextArea]
    [SerializeField]
    private string info =
        "1: 전투 시작(Phase 1)\n" +
        "2: Phase 2 전환\n" +
        "3: Phase 3 전환\n" +
        "0: 전투 종료";

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("<color=cyan>[BGMTest] 전투 시작 (Phase 1)</color>");
            BGMManager.Instance?.EnterBattle(0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("<color=cyan>[BGMTest] Phase 2 전환</color>");
            BGMManager.Instance?.SetBattlePhase(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("<color=cyan>[BGMTest] Phase 3 전환</color>");
            BGMManager.Instance?.SetBattlePhase(2);
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            Debug.Log("<color=cyan>[BGMTest] 전투 종료</color>");
            BGMManager.Instance?.ExitBattle();
        }
    }
}