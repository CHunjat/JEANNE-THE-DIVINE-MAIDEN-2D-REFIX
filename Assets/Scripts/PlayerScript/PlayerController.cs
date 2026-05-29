using System;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [Header("Layer Settings")]
    public LayerMask stairsLayer; // 이 줄이 있는지 확인하세요!

    [Header("Input Data")]
    public InputReader inputReader;

    [Header("Components")]
    public Rigidbody2D rb; // 2D로 전환
    public BoxCollider2D cd; // 2D로 전환
    public Animator animator;

    [Header("Movement Settings")]
    public float moveSpeed = 7f;
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashcooltime = 1.5f;
    private float dashCooltimer;


    [Header("착지딜레이 버니합금지")]
    public float landDashDelay = 0.5f;
    private float landTimer;


    [Header("Orientation")]
    public bool isFacingRight = true;

    [Header("점프랑 공중세팅")]
    public float jumpForce = 12f; // 인스펙터에서 조절 가능
    public float airDeceleration = 5f;
    public int MaxJumpCount = 2;
    private int currentjumpCount;
    public void RestJumpCount() => currentjumpCount = MaxJumpCount;
    public bool CanJump => currentjumpCount > 0;
    public void UseJump() => currentjumpCount--;


    [Header("Ground Check Settings (BoxCast)")]
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.1f); // Vector2로 전환
    public float groundCheckDistance = 0.1f;
    public LayerMask groundLayer;

    [Header("벽, 벽점프 관리 세팅")]
    public float wallSlideSpeed = 2f;
    public Vector2 wallJumpForce = new Vector2(10f, 12f);
    public float wallCheckDistance = 0.1f;
    public LayerMask wallLayer;
    public Vector2 WallCheckSize = new Vector2(0.05f, 1.5f); // Vector2로 전환


    [Header("대쉬후 전력질주기능")]
    public float sprintSpeed = 10f;
    public bool isSprinting;
    public bool wasSprinting; // ★ 방금 전까지 전력질주 중이었는지 저장 (Break 모션용)

    [Header("스프린트 관리변수")]
    public string anim_SprintStart = "To sprint";
    public string anim_SprintIng = "Sprinting";
    public string anim_SprintBreak = "SprintBreak";
    public string anim_SprintJump = "Sprint-jump-falling";
    public string anim_SprintLand = "sprint-falling-land";
    //스프린트 점프 쿨타임 변수
    public float sprintJumpCooldown = 0.5f; // 스프린트 점프 후 대기 시간
    private float sprintJumpCooldownTimer;
    public bool isJumpCut; //쿨타임때문에 점프가 캔슬되었는지 기록, 쿨타임이 안돌았는데 스프린트 점프시 다시 movestate로 돌아가는 코드로 스프린트애니메이션이 다시 재생되는 현상으로 컷내려고만듦

    [Header("강공 찌르기 애니메이션 관리변수")] //스킬X 스킬아님!!
    public string anim_ThrustReady = "MiddleToCharge"; // 기 모으기 모션 이름
    public string anim_ThrustAtk = "MiddleChargeATK";  // 찌르기 모션 이름


    [Header("벽 애니메이션 관리변수")]
    public string anim_WallSlide = "walling";
    public string anim_WallJump = "WallJump";

    [Header("점프 직후 벽감지 쿨타임 추가")]
    public float wallGrabCooldown = 0.2f;
    public float wallGrabTimer;


    [Header("Thrust Attack Settings")]
    public AnimationCurve thrustVelocityCurve; // 찌르기 속도 그래프
    public float thrustDuration = 0.5f;        // 찌르기 전체 지속 시간 (초)


    [Header("공중공격 평타 설정")]
    public int currentAirActionCount = 0;   // 현재 공중 공격 횟수
    public int maxAirActions = 2;           // 최대 허용 횟수
    public float airAttackBounceForce = 2f; // 허공답보 (위로 살짝 뜨는 힘)

    [Header("공중 찍기공격")]
    public float diveDropSpeed = 25f; // 밑으로 내리꽂는 속도 (엄청 빨라야 찰집니다!)
    public string anim_DiveDrop = "AirHeavyDrop";
    public string anim_DiveLand = "AirHeavyAtk";

    [Header("방향키 공중 공격 애니메이션")]
    public string anim_AirUpAtk = "AirUpAtk";
    public string anim_AirDownAtk = "AirDownAtk";

    public bool hasUsedAirUp;   // 윗공격 1회 제한 스위치

    [Header("방어")]
    public string anim_GuardNormal = "BlockNormal";
    public string anim_GuardOff = "Blockoff";
    public string anim_BlockHit = "BlockNormalHit";
    public string anim_BlockBreak = "BlockBreak";

    [Header("패링")]



    [Header("그래플링 훅 설정")]
    public LineRenderer grappleLine;      // 마법 로프 역할을 할 라인 렌더러
    public Transform shieldPoint;         // 줄이 뻗어나갈 시작점 (방패 쪽 손 위치에 빈 오브젝트 생성해서 할당)
    public Transform AirShieldPoint;      // 줄이 뻗어나갈 시작점 (공중)
    public float grappleSpeed = 30f;      // 날아가는 속도
    public float grappleMaxRange = 5f;      // 그래플링 가능 반경
    public float grappleMinRange = 0.1f;
    public LayerMask grappleLayer;        // "GrapplePoint" 레이어

    [Header("Grapple Animation")]
    public string anim_Grapple = "GrappleStart"; // 방금 주신 스프라이트 애니메이션 이름

    [Header("밑점프 감지 타이머")]
    public Collider2D ignoredDropCollider;

    [Header("비탈길(Slope) 세팅")]
    public float maxSlopeAngle = 45f;
    public RaycastHit2D slopeHit; // 2D로 전환

    public PlayerAttack1State Attack1State { get; private set; }
    public PlayerAttack2State Attack2State { get; private set; }
    public PlayerAttack3State Attack3State { get; private set; }

    public PlayerDashAttackState DashAndSprintATK { get; private set; }
    public PlayerStateMachine StateMachine { get; private set; }
    public PlayerIdleState IdleState { get; private set; }
    public PlayerMoveState MoveState { get; private set; }
    public PlayerDashState DashState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }
    public PlayerAirState AirState { get; private set; }
    public PlayerLandState LandState { get; private set; }
    public PlayerWallSlideState WallSlideState { get; private set; }
    public PlayerWallJumpState WallJumpState { get; private set; }

    public PlayerHeavyReadyState HeavyReadyState { get; private set; }
    public PlayerHeavyChargeState HeavyChargeState { get; private set; }
    public PlayerHeavyAttackState HeavyAttackState { get; private set; }

    public PlayerThrustReadyState ThrustReadyState { get; private set; }
    public PlayerThrustAttackState ThrustAttackState { get; private set; }
    public PlayerAirAttack1State AirAttack1State { get; private set; }
    public PlayerAirAttack2State AirAttack2State { get; private set; }

    public PlayerDiveDropState DiveDropState { get; private set; }
    public PlayerDiveLandState DiveLandState { get; private set; }

    public PlayerAirUpAttackState AirUpAttackState { get; private set; }

    public PlayerGuardState GuardState { get; private set; }
    public PlayerGuardOffState GuardOffState { get; private set; }
    public PlayerGrappleState GrappleState { get; private set; }

    public PlayerDropState DropState { get; private set; }

    [Header("변수 선언부")]
    public bool CanDash => dashCooltimer <= 0 && landTimer <= 0;
    // 1. 비탈길인지 확인하고 경사면 정보(slopeHit)를 업데이트함
    private float defaultGravityScale;


    private void Awake()
    {
        StateMachine = new PlayerStateMachine();
        IdleState = new PlayerIdleState(this, StateMachine, "Idle");
        MoveState = new PlayerMoveState(this, StateMachine, "Move");
        DashState = new PlayerDashState(this, StateMachine, "Dash");
        JumpState = new PlayerJumpState(this, StateMachine, "Jump");
        AirState = new PlayerAirState(this, StateMachine, "Falling");
        LandState = new PlayerLandState(this, StateMachine, "Landing");

        WallSlideState = new PlayerWallSlideState(this, StateMachine, "walling");
        WallJumpState = new PlayerWallJumpState(this, StateMachine, "WallJump");

        Attack1State = new PlayerAttack1State(this, StateMachine, "ATK1");
        Attack2State = new PlayerAttack2State(this, StateMachine, "ATK2");
        Attack3State = new PlayerAttack3State(this, StateMachine, "ATK3");
        DashAndSprintATK = new PlayerDashAttackState(this, StateMachine, "Dash(Sprint)ATK");

        HeavyReadyState = new PlayerHeavyReadyState(this, StateMachine, "Idle");
        HeavyChargeState = new PlayerHeavyChargeState(this, StateMachine, "ToCharge");
        HeavyAttackState = new PlayerHeavyAttackState(this, StateMachine, "ChargingAtk");


        ThrustReadyState = new PlayerThrustReadyState(this, StateMachine, "MiddleToCharge");
        ThrustAttackState = new PlayerThrustAttackState(this, StateMachine, "MiddleChargeATK");

        AirAttack1State = new PlayerAirAttack1State(this, StateMachine, "AirAtk1");
        AirAttack2State = new PlayerAirAttack2State(this, StateMachine, "AirAtk2");

        DiveDropState = new PlayerDiveDropState(this, StateMachine, anim_DiveDrop);
        DiveLandState = new PlayerDiveLandState(this, StateMachine, anim_DiveLand);
        AirUpAttackState = new PlayerAirUpAttackState(this, StateMachine, anim_AirUpAtk);

        GuardState = new PlayerGuardState(this, StateMachine, anim_GuardNormal);
        GuardOffState = new PlayerGuardOffState(this, StateMachine, anim_GuardOff);
        GrappleState = new PlayerGrappleState(this, StateMachine, anim_Grapple);

        DropState = new PlayerDropState(this, StateMachine, "Falling");

        rb = GetComponent<Rigidbody2D>(); // 2D로 변경
        cd = GetComponent<BoxCollider2D>(); // 2D로 변경
        defaultGravityScale = rb.gravityScale;
    }

    private void Start() => StateMachine.Initialize(IdleState);

    private void Update()
    {

        if (sprintJumpCooldownTimer > 0)
            sprintJumpCooldownTimer -= Time.deltaTime;

        //지상에서점프시 벽판정쿨타임
        if (wallGrabTimer > 0)
        { wallGrabTimer -= Time.deltaTime; }

        if (dashCooltimer > 0)
            dashCooltimer -= Time.deltaTime;
        if (landTimer > 0) landTimer -= Time.deltaTime;

        if (inputReader.DashPressed && !CanDash)
        {
            inputReader.DashPressed = false;
        }
        //테스트 공격중일떄는 대시 입력을 강제로 차단
        if (StateMachine.CurrentState is PlayerAttackState && inputReader.DashPressed)
        {
            inputReader.DashPressed = false;
        }

        if (IsGrounded() && rb.linearVelocity.y <= 0.1f)
        {
            RestJumpCount();
        }

        if (IsGrounded() && rb.linearVelocity.y <= 0.1f)
        {
            RestJumpCount();
            ResetAirActions(); // 바닥에 닿으면 공중 공격 횟수 초기화
        }

        //딱 idle, move에서만 가능
        HandleThrustAttackInput(); //강공찌르기 판독기 추가
        HandleHeavyAttackInput(); //스킬찌르기 판독기 추가


        StateMachine.CurrentState.HandleInput();
        StateMachine.CurrentState.LogicUpdate();

    }
    public void ResetDashCooldown() => dashCooltimer = dashcooltime;
    public void ResetLandTimer() => landTimer = landDashDelay;
    private void FixedUpdate()
    {
        StateMachine.CurrentState.PhysicsUpdate();


        #region 중력 안받는 스테이트들
        // 그래플훅일땐 제외 / 미들찌르기/스킬제외 /공중공격제외
        if (StateMachine.CurrentState == GrappleState) return;
        if (StateMachine.CurrentState == ThrustReadyState) return;
        if (StateMachine.CurrentState == HeavyReadyState) return;
        if (StateMachine.CurrentState == HeavyChargeState) return;
        if (StateMachine.CurrentState == HeavyAttackState) return;
        if (StateMachine.CurrentState == AirAttack1State) return;
        if (StateMachine.CurrentState == AirAttack2State) return;

        bool isMidAir = StateMachine.CurrentState == JumpState ||
                        StateMachine.CurrentState == AirState ||
                        StateMachine.CurrentState == DropState ||
                        StateMachine.CurrentState == DashState;
        #endregion

        if (Mathf.Abs(inputReader.MoveValue.x) < 0.1f && IsGrounded() && !isMidAir)
        {
            if (OnSlope())
            {
                // 비탈길: 관성을 완전히 죽이고, 중력을 0으로 꺼서 제자리에 못 박음
                rb.linearVelocity = Vector2.zero;
                rb.gravityScale = 0f;
            }
            else
            {
                // 평지: 미끄러짐(X축 관성)만 없애고 중력(Y축)은 유지
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                rb.gravityScale = defaultGravityScale;
            }
        }
        else
        {
            // 이동 중이거나 공중이면 중력을 즉시 원래대로 복구
            rb.gravityScale = defaultGravityScale;
        }

        // [핵심 추가] 내리막길 스프린트 시 튀어오름 방지
        if (isSprinting && OnSlope() && !isMidAir && Mathf.Abs(inputReader.MoveValue.x) >= 0.1f)
        {
            Vector2 slopeDir = GetSlopeMoveDirection(rb.linearVelocity.normalized);
            float currentSpeed = rb.linearVelocity.magnitude;
            
                rb.linearVelocity = slopeDir * currentSpeed;
                rb.AddForce(Vector2.down * 50f, ForceMode2D.Force);
            
        }

        if (IsPureGrounded() && !OnSlope())
        {
            ToggleStairsCollision(false);
        }
        else if (StateMachine.CurrentState == DropState)
        {
            ToggleStairsCollision(false);
        }
        // ★ 수정 3: AirState(낙하) 시 안착 로직 (겹침 방지)
        else if (StateMachine.CurrentState == AirState)
        {
            // 캐릭터 몸통이 계단과 겹쳐있는지 간단히 확인 (0.95f로 약간 작게 해서 오작동 방지)
            bool isBodyInside = Physics2D.OverlapBox(cd.bounds.center, cd.bounds.size * 0.9f, 0f, stairsLayer) != null;

            // 2. 발밑에 계단이 있는지 확인 (안착할 땅이 있는가?)
            bool isStairUnder = Physics2D.BoxCast(new Vector2(cd.bounds.center.x, cd.bounds.min.y), new Vector2(cd.bounds.size.x * 0.7f, 0.1f), 0f, Vector2.down, 3.0f, stairsLayer).collider != null;

            // [순위 1] 대시 중일 때 -> 무조건 통과 (대시가 최우선)
            if (StateMachine.CurrentState == DashState)
            {
                ToggleStairsCollision(false);
            }
            // [순위 2] 몸통이 계단 안에 겹쳐 있을 때 -> 무조건 통과 (밑점프든, 점프 중이든 끼임 방지)
            else if (isBodyInside)
            {
                ToggleStairsCollision(false);
            }
            // [순위 3] 몸통은 밖으로 나왔는데, 발밑에 계단이 있을 때 -> 안착! (계단 위 착지 성공)
            else if (isStairUnder)
            {
                ToggleStairsCollision(true);
            }
            // [순위 4] 그 외 허공
            else
            {
                ToggleStairsCollision(false);
            }
        }


    }

    // 2D 리지드바디이므로 Vector2를 사용
    public void SetVelocity(float x, float y)
    {
        rb.linearVelocity = new Vector2(x, y);
    }

    public bool IsPureGrounded()
    {
        if (cd == null) return false;
        Vector2 rayStartPos = new Vector2(cd.bounds.center.x, cd.bounds.min.y + 0.1f);

        // stairsLayer 없이 오직 groundLayer만 단독으로 검사합니다.
        RaycastHit2D hit = Physics2D.BoxCast(rayStartPos, groundCheckSize, 0f, Vector2.down, groundCheckDistance + 0.1f, groundLayer);
        return hit.collider != null;
    }

    public LayerMask GetCurrentGroundMask()
    {
        LayerMask mask = groundLayer;

        // 현재 유니티 물리 엔진에서 플레이어와 계단이 충돌 가능한 상태인지 확인
        int playerLayer = LayerMask.NameToLayer("Player");
        int stairsLayerIdx = LayerMask.NameToLayer("Stairs");
        bool isCollisionEnabled = !Physics2D.GetIgnoreLayerCollision(playerLayer, stairsLayerIdx);
        if (StateMachine != null)
        {
            // 물리 충돌이 켜져 있거나(계단 위), 공중(AirState)일 때만 계단을 감지!
            if (isCollisionEnabled || StateMachine.CurrentState == AirState)
            {
                mask |= stairsLayer;
            }
        }
        return mask;
    }



    public void FlipController(float xInput)
    {
        if (xInput > 0) isFacingRight = true;
        else if (xInput < 0) isFacingRight = false;

        // 사이드뷰 3D 반전 (모델에 따라 90/270 혹은 0/180 사용)
        float targetY = isFacingRight ? 0f : 180f;
        transform.rotation = Quaternion.Euler(0, targetY, 0);

        if (xInput < 0) Debug.Log("왼쪽 바라보기 성공!");
    }

    public void SetDashJumpVelocity(float dashDir)
    {
        rb.linearVelocity = new Vector2(dashDir * dashSpeed, jumpForce);
    }
    //벽체크 함수
    public bool IsTouchingWall(float dir)
    {
        if (cd == null) return false;

        Vector2 origin = cd.bounds.center;

        // BoxCast (시작점, 박스크기/2, 각도, 방향, 거리, 레이어)
        float checkDist = cd.bounds.extents.x + wallCheckDistance;

        return Physics2D.BoxCast(origin, WallCheckSize, 0f, Vector2.right * dir, checkDist, wallLayer).collider != null;
    }

    //4.29 시작, 공격 분배기 함수
    public void HandleAttackInput()
    {
        // 1. 방금 1단계에서 만든 InputReader를 통해 공격을 눌렀는지 확인 (확인 즉시 값은 false로 깎임)
        if (!inputReader.AttackPressed) return;

        if (StateMachine.CurrentState == DashState || isSprinting)
        {
            // 대시 어택 진입 시 스프린트 상태를 강제로 끄기
            StateMachine.ChangeState(DashAndSprintATK);
            return;
        }
        // 2. 만약 땅에 있고, 콤보 중이 아니라면? -> 1타 발동!
        if (IsGrounded() && StateMachine.CurrentState != Attack1State)
        {
            StateMachine.ChangeState(Attack1State);
        }
        //공중 1타 분배
        else if (!IsGrounded() && currentAirActionCount < maxAirActions
            && !(StateMachine.CurrentState is PlayerAirAttack1State)
            && !(StateMachine.CurrentState is PlayerAirAttack2State)
            && !(StateMachine.CurrentState is PlayerAirUpAttackState))
        {
            if (IsTooCloseToGround()) return; //공중 윗공격 땅 x
            float yInput = inputReader.MoveValue.y;

            if (yInput > 0.5f)
            {
                // 윗방향키 + 공격
                StateMachine.ChangeState(AirUpAttackState);
            }
            else
            {
                // 방향키 입력이 없거나 좌우만 누르고 있을 때 -> 기본 공중 1타
                StateMachine.ChangeState(AirAttack1State);
            }
        }

    }

    //스킬공격 분배기 함수 // 헤비어택(스킬) 할당키 "E"
    public void HandleHeavyAttackInput()
    {
        // 1.
        if (!inputReader.HAttackPressed) return;

        if (StateMachine.CurrentState is PlayerAttackState)
        {
            Debug.Log("현재 공격 중이라 강공격 입력을 무시합니다.");
            inputReader.HAttackPressed = false;
            return;
        }
        //스프린트중 강공격막음 없애려면 이거 지워라 기획;;
        if (isSprinting)
        {
            inputReader.HAttackPressed = false;
            return;
        }

        // 2. 땅에 있고, 이미 기모으기 준비 중이 아닐 때만 진입
        if (IsGrounded() && StateMachine.CurrentState != HeavyReadyState &&
            StateMachine.CurrentState != HeavyChargeState && StateMachine.CurrentState != HeavyAttackState)
        {
            // 판독기(ReadyState)로 보냅니다.
            StateMachine.ChangeState(HeavyReadyState);
        }
    }



    //강공 찌르기 //키 F
    public void HandleThrustAttackInput()
    {
        if (!inputReader.ThrustAttackPressed) return;

        // 1. 이미 내려찍기 중이면 중복 방지
        if (StateMachine.CurrentState == DiveDropState || StateMachine.CurrentState == DiveLandState)
        {
            inputReader.ThrustAttackPressed = false;
            return;
        }

        bool isActuallyOnGround = IsGrounded() || OnSlope();

        // --- [A] 지상/비탈길 (찌르기) ---
        if (isActuallyOnGround)
        {
            if (!(StateMachine.CurrentState is PlayerAttackState) &&
                StateMachine.CurrentState != ThrustReadyState &&
                StateMachine.CurrentState != ThrustAttackState)
            {
                if (OnSlope()) SetVelocity(0f, 0f);
                inputReader.ThrustAttackPressed = false;
                StateMachine.ChangeState(ThrustReadyState);
            }
        }
        // --- [B] 공중 (하강 공격) ---
        else
        {
            // 🚨 어제 맞췄던 '공격 진행도' 로직 부활
            // 현재 공중 공격 중이라면 애니메이션이 어느 정도 진행되었는지 확인
            if (StateMachine.CurrentState is PlayerAirAttack1State ||
                StateMachine.CurrentState is PlayerAirAttack2State ||
                StateMachine.CurrentState is PlayerAirUpAttackState)
            {
                // 🔥 normalizedTime이 0.4f~0.5f 정도는 지나야 하강 공격으로 캔슬 가능
                float nTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

                if (nTime < 0.4f)
                {
                    return;
                }
            }

            // 공중 일반 상태거나, 위 조건을 통과한 공격 후반부라면 즉시 발동
            inputReader.ThrustAttackPressed = false;
            StateMachine.ChangeState(DiveDropState);
        }
    }

    public void HandleGuardInput()
    {
        // 방어 키(S)가 눌려있으면
        if (inputReader.GuardHeld && IsGrounded())
        {
            // 대시 중이었다면 대시 관성을 지우고 방어
            if (StateMachine.CurrentState == DashState)
            {
                rb.linearVelocity = Vector2.zero;
            }

            StateMachine.ChangeState(GuardState);
        }
    }

    public void HandleGrappleInput()
    {
        if (StateMachine.CurrentState == GrappleState) return;

        // 그래플 키가 눌렸을 때
        if (inputReader.GrapplePressed)
        {
            // 만약 전력질주(Sprint) 중이었다면 상태 전환 전에 꺼버렷
            if (isSprinting)
            {
                isSprinting = false;
            }

            // 그래플 상태로 즉시 전이!
            StateMachine.ChangeState(GrappleState);
        }
    }

    // 통과 가능한 계단이나 원웨이 플랫폼의 콜라이더를 찾아 반환합니다.
    public Collider2D GetDropThroughCollider()
    {
        float rayLength = cd.bounds.extents.y + 0.4f;
        Vector2 rayOrigin = cd.bounds.center;

        // 1. 발밑을 훑어서 감지된 모든 후보를 가져옴
        RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, Vector2.down, rayLength, stairsLayer | groundLayer);

        // 2. 가장 가까운 타겟을 찾는데, ignoredDropCollider는 절대 제외!
        Collider2D bestTarget = null;
        float closestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            if (hit.collider == null || hit.collider == cd || hit.collider == ignoredDropCollider)
                continue; // 밟고 있는 계단은 무조건 거름!

            // 이제 남은 놈들 중 진짜로 내가 내려가야 할 타겟을 찾음
            if (hit.distance < closestDist)
            {
                closestDist = hit.distance;
                bestTarget = hit.collider;
            }
        }

        // 3. 만약 비탈길 위에 있다면, 비탈길이 최우선 타겟이어야 함 (안전장치)
        if (OnSlope() && slopeHit.collider != null && slopeHit.collider != ignoredDropCollider)
        {
            return slopeHit.collider;
        }

        if (bestTarget != null)
        {
            Vector2 footPos = new Vector2(cd.bounds.center.x, cd.bounds.min.y);
            Vector2 checkSize = new Vector2(cd.bounds.size.x * 0.8f, 0.1f);

            // 아래쪽을 검사해서 target 이외의 다른 장애물(ground, stairs)이 있으면 취소
            RaycastHit2D[] checkHits = Physics2D.BoxCastAll(footPos, checkSize, 0f, Vector2.down, 0.3f, groundLayer | stairsLayer);

            foreach (var hit in checkHits)
            {
                if (hit.collider != null && hit.collider != bestTarget)
                {
                    Debug.Log("공간이 너무 좁아 밑점프를 취소합니다!");
                    return null; // 안전장치: 좁으면 통과 불가
                }
            }
        }


        return bestTarget;
    }

    public bool OnSlope()
    {
        if (cd == null) return false;

        float rayLength = cd.bounds.extents.y + 0.3f;
        LayerMask currentMask = GetCurrentGroundMask();

        // 기존 hits 로직 그대로 사용하되, 가장 가까운 놈을 잡는 방식을 "약간의 여유"를 둠
        RaycastHit2D[] hits = Physics2D.RaycastAll(cd.bounds.center, Vector2.down, rayLength, currentMask);

        RaycastHit2D bestHit = default;
        float minDist = float.MaxValue;

        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider != ignoredDropCollider)
            {
                // ★ 핵심: 현재 저장된 slopeHit(이전 프레임의 비탈길)이 있다면, 
                // 거리가 아주 크게 변하지 않는 이상 그대로 유지함 (0.1f 오차 허용)
                if (slopeHit.collider != null && hit.collider == slopeHit.collider)
                {
                    bestHit = hit;
                    break; // 같은 콜라이더라면 고민할 필요도 없이 유지
                }

                // 새로운 콜라이더라면 거리 비교
                if (hit.distance < minDist)
                {
                    minDist = hit.distance;
                    bestHit = hit;
                }
            }
        }

        if (bestHit.collider != null)
        {
            float angle = Vector2.Angle(Vector2.up, bestHit.normal);

            // 평지(0.1도 이하)는 비탈길 아님
            if (angle <= 0.1f) return false;

            // 비탈길 범위
            if (angle > 0.1f && angle <= maxSlopeAngle)
            {
                slopeHit = bestHit; // 판정 고정
                return true;
            }
        }
        return false;
    }

    // 2. 가고자 하는 방향(Vector3)을 경사면에 맞춰 꺾어주는 함수
    public Vector2 GetSlopeMoveDirection(Vector2 direction)
    {
        // 1. 비탈길의 각도를 구함
        Vector2 tangent = Vector2.Perpendicular(slopeHit.normal).normalized;

        // 2. [가장 중요] 탄젠트의 X값과 내 입력(direction.x)의 부호가 다르면 무조건 뒤집는다.
        // 이렇게 하면 입구에서 튕길 일이 없습니다.
        if (tangent.x * direction.x < 0)
        {
            tangent = -tangent;
        }

        return tangent;
    }


    // 공중 횟수 초기화 함수
    public void ResetAirActions() => currentAirActionCount = 0;

    //방어코드 (공중 윗공격 찰나의순간, 땅에서 써버리는거 막기위함)
    public bool IsTooCloseToGround()
    {
        Vector2 rayStartPos = new Vector2(cd.bounds.center.x, cd.bounds.min.y + 0.1f);
        return Physics2D.BoxCast(rayStartPos, groundCheckSize / 2, 0f, Vector2.down, 0.5f, groundLayer).collider != null;
    }


    //스프린트 점프 쿨타임 리셋함수
    public void ResetSprintJumpCooldown()
    {
        sprintJumpCooldownTimer = sprintJumpCooldown; // 
    }
    //프로퍼티
    public bool CanSprintJump => sprintJumpCooldownTimer <= 0;


    public bool IsGrounded()
    {

        if (cd == null) return false;

        if (StateMachine != null)
        {
            // 착지 모션 중이거나 대쉬 중일 때의 예외 처리
            if (StateMachine.CurrentState == LandState) return true;
        }

        Vector2 rayStartPos = new Vector2(cd.bounds.center.x, cd.bounds.min.y + 0.1f);

        // 방금 만든 GetCurrentGroundMask()를 사용
        RaycastHit2D[] hits = Physics2D.BoxCastAll(rayStartPos, groundCheckSize, 0f, Vector2.down, groundCheckDistance + 0.1f, GetCurrentGroundMask());

        foreach (var hit in hits)
        {
            // 감지된 놈이 내가 지금 통과 중인 그 발판(ignoredDropCollider)이 아니라면? -> 진짜 땅이다!
            if (hit.collider != null && hit.collider != ignoredDropCollider)
            {
                return true;
            }
        }

        if (OnSlope()) return true;

        return false;
    }

    private void OnDrawGizmos()
    {
        if (cd == null) return;

        Vector2 rayStartPos = new Vector2(cd.bounds.center.x, cd.bounds.min.y + 0.1f);

        Gizmos.color = IsGrounded() ? Color.green : Color.red;
        Gizmos.DrawWireCube(rayStartPos + Vector2.down * (groundCheckDistance + 0.1f), groundCheckSize);

        Gizmos.color = Color.blue;
        DrawWallGizmo(1f);
        DrawWallGizmo(-1f);

    }

    public bool CheckLandingSurface(out Collider2D hitCollider)
    {
        hitCollider = null;

        // 1. 플레이어 발바닥 기준점
        Vector2 footPos = new Vector2(cd.bounds.center.x, cd.bounds.min.y);
        Vector2 checkSize = new Vector2(cd.bounds.size.x * 0.7f, 0.1f);

        // 2. 밑점프 시에는 현재 밟고 있는 콜라이더(ignoredDropCollider)를 무시해야 함
        // 레이어 마스크를 이용해 Ground와 Stairs를 한 번에 검사
        LayerMask landingMask = groundLayer | stairsLayer;

        RaycastHit2D[] hits = Physics2D.BoxCastAll(footPos, checkSize, 0f, Vector2.down, 0.6f, landingMask);

        foreach (var hit in hits)
        {
            // 내 몸뚱이(Player)랑 겹친 건 제외
            if (hit.collider == cd) continue;

            // 밑점프 중이라면, 지금 통과 중인 계단(ignoredDropCollider)은 무시!
            if (StateMachine.CurrentState == DropState && hit.collider == ignoredDropCollider)
                continue;

            // 위에 조건 다 통과했으면 이게 진짜 착지할 바닥!
            hitCollider = hit.collider;
            return true;
        }
        return false;
    }

    private void DrawWallGizmo(float dir)
    {
        Vector2 origin = cd.bounds.center;
        float checkDist = cd.bounds.extents.x + wallCheckDistance;
        Vector2 hitCenter = origin + (Vector2.right * dir * checkDist);
        Gizmos.DrawWireCube(hitCenter, WallCheckSize);
    }


    public LayerMask GetGroundCheckMask() => groundLayer | stairsLayer;

    public void ToggleStairsCollision(bool enable)
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        int stairsLayerIdx = LayerMask.NameToLayer("Stairs");
        Physics2D.IgnoreLayerCollision(playerLayer, stairsLayerIdx, !enable);
    }

    public bool IsOnStairs()
    {
        if (cd == null) return false;

        RaycastHit2D[] hits = Physics2D.BoxCastAll(cd.bounds.center, cd.bounds.size, 0f, Vector2.down, 0.3f, stairsLayer);
        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider != ignoredDropCollider)
            {
                return true;
            }
        }
        return false;
    }
}