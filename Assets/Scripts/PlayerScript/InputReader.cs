using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputReader", menuName = "Game/Input Reader")]
public class InputReader : ScriptableObject, PlayerControls.IPlayerActions
{
    private PlayerControls controls;

    // 외부에서 읽어갈 프로퍼티들
    public Vector2 MoveValue { get; private set; }
    public bool DashPressed { get; set; }
    private bool _jumpPressed;
    public bool JumpPressed //예약입력 제한  좋은데?
    {
        get
        {
            bool value = _jumpPressed;
            _jumpPressed = false; // [핵심] 읽어가자마자 바로 꺼버림
            return value;
        }
        set => _jumpPressed = value;
        //이렇게 하면 JumpState에 진입하기 위해 한 번 읽는 순간 값이 사라지므로,
        //중복 실행이 물리적으로 불가능
    }


    public bool HAttackPressed
    {
        get
        {
            bool value = _HAttackPressed;
            _HAttackPressed = false;
            return value;

        }
        set => _HAttackPressed = value;
    }

    //기모으기 누르고있는지 확인하는 bool 변수
    public bool HeavyAttackHeld;

    public bool ThrustAttackHeld;

    private bool _HAttackPressed;


    private bool _AttackPressed;


    public bool AttackPressed
    {
        get
        {
            bool value = _AttackPressed;
            _AttackPressed = false;
            return value;
        }
        set => _AttackPressed = value;
        
    }

    private bool _ThrustAttackPressed; //
    public bool ThrustAttackPressed
    {
        get
        {
            bool value = _ThrustAttackPressed;
            _ThrustAttackPressed = false;
            return value;
        }
        set => _ThrustAttackPressed = value;
        
    }



    private void OnEnable()
    {
        if (controls == null)
        {
            controls = new PlayerControls();
            controls.Player.SetCallbacks(this); // 인터페이스 연결
        }
        controls.Player.Enable();
    }

    private void OnDisable() => controls.Player.Disable();

    // 인터페이스 구현 (자동 호출됨)
    public void OnMove(InputAction.CallbackContext context)
    {
        MoveValue = context.ReadValue<Vector2>();
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed) DashPressed = true;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        Debug.Log("점프누름");
        if (context.started) JumpPressed = true;
    }

    public void OnATK(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            AttackPressed = true;
        }
        // 2. 버튼에서 손을 떼면 혹시 모를 찌꺼기 입력을 지우기 위해 false를 줍니다.
        else if (context.canceled)
        {
            AttackPressed = false;
        }
    }

    //public void OnHATK(InputAction.CallbackContext context)
    //{
    //    if (context.started)
    //    {
    //        HAttackPressed = true;   // 분배기(Controller)를 뚫고 ReadyState
    //        HeavyAttackHeld = true;  // 지금부터 계속 누르고 있다는 스위치 
    //    }
    //    // E키에서 손가락을 떼는 순간
    //    else if (context.canceled)
    //    {
    //        HeavyAttackHeld = false; // 누름 상태 스위치 OFF!
    //    }
    //}

 
    public void OnSkill(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            HAttackPressed = true;   // 분배기(Controller)를 뚫고 ReadyState
            HeavyAttackHeld = true;  // 지금부터 계속 누르고 있다는 스위치 
        }
        // E키에서 손가락을 떼는 순간
        else if (context.canceled)
        {
            HeavyAttackHeld = false; // 누름 상태 스위치 OFF!
        }
    }

    public void OnThrustATK(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            ThrustAttackPressed = true; // 상태 전이용 단발성 티켓 발급
            ThrustAttackHeld = true;    // 기 모으기 유지 스위치 ON
            Debug.Log("찌르기 키 눌림! (Held = true)"); //
        }
        else if (context.canceled)
        {
            ThrustAttackHeld = false;   // 손 떼면 유지 스위치 OFF
            Debug.Log("찌르기 키 떨어짐! (Held = false)"); //
        }
    }

    public bool GuardHeld;
    public bool GuardPressed;

    public void OnGuard(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            GuardPressed = true;
            GuardHeld = true;
        }
        else if (context.canceled)
        {
            GuardHeld = false;
        }
    }


    

    private bool _grapplePressed;
    public bool GrapplePressed
    {
        get
        {
            bool value = _grapplePressed;
            _grapplePressed = false; // 읽어가자마자 꺼버림! (점프랑 동일)

            return value;
        }
        set => _grapplePressed = value;
    }

    private bool _lightningPress; 
    public bool LightningPressed
    {
        get
        {
            bool value = _lightningPress;
            _lightningPress = false;
            return value;
        }
        set
        {
            _lightningPress = value;
        }
    
    }

    public bool LightningHeld;
    public void OnLightning(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            LightningPressed = true;  // 단발성 진입 티켓
            LightningHeld = true;     // 기 모으기 유지 스위치 ON
        }
        else if (context.canceled)
        {
            LightningHeld = false;    // 손 떼면 유지 스위치 OFF
        }
    }

    public void OnGrapple(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            GrapplePressed = true;
        }
    }
}