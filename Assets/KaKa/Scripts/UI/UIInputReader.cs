using UnityEngine;
using UnityEngine.InputSystem;
using System; // Action을 쓰기 위해 필요합니다!

[CreateAssetMenu(fileName = "UIInputReader", menuName = "Game/UI Input Reader")]
public class UIInputReader : ScriptableObject, UIControls.IUIActions
{
    public event Action OnRotateSkillPressed;
    private UIControls controls;

    // "ESC 키가 눌렸을 때" 사방에 알려줄 이벤트 방송국을 세웁니다.
    public event Action OnPausePressed;

    private void OnEnable()
    {
        if (controls == null)
        {
            controls = new UIControls();
            controls.UI.SetCallbacks(this);
        }
        controls.UI.Enable();
    }

    private void OnDisable()
    {
        if (controls != null)
        {
            controls.UI.Disable();
        }
    }

    public void OnRotateSkill(InputAction.CallbackContext context)
    {
        // 키를 꾹 누르고 있을 때 여러 번 실행되지 않도록, 
        // "딱 누르는 순간(Started)"에만 알림벨을 한 번 울립니다.
        if (context.started)
        {
            OnRotateSkillPressed?.Invoke();
        }
    }

    // 인풋 시스템이 ESC 입력을 감지하면 자동으로 실행되는 곳
    public void OnPause(InputAction.CallbackContext context)
    {
        // 키를 딱 누른 순간(started)에만 방송을 내보냅니다.
        if (context.started)
        {
            // 이 이벤트를 구독하고 있는 대상을 향해 신호를 쏩니다.
            OnPausePressed?.Invoke();
        }
    }
}