using UnityEngine;

public class MainMenuBGMNotifier : MonoBehaviour
{
    private void OnEnable()
    {
        TryNotify(true);
    }

    private void OnDisable()
    {
        TryNotify(false);
    }

    // Start()는 모든 오브젝트의 Awake()가 끝난 뒤 호출되는 게 보장되므로,
    // 씬 시작 시 이미 활성화된 상태라 OnEnable이 BGMManager.Awake()보다
    // 먼저 실행돼버린 경우를 여기서 다시 잡아준다.
    private void Start()
    {
        if (gameObject.activeInHierarchy)
        {
            TryNotify(true);
        }
    }

    private void TryNotify(bool isOpen)
    {
        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.SetMenuOpen(isOpen);
        }
    }
}