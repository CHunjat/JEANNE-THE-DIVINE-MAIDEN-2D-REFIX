using FMODUnity;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using TheBlackCat.TrailEffect2D;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float groundedGraceTime = 0.8f; // 공중 판정 유예 시간
    public float groundedTimer;
    [HideInInspector] public Collider2D ignoredDropCollider;
    [HideInInspector] public PlayerStats playerStats; //스탯 컴포넌트 연결

    public bool ignoreSlopeDetection = false;

    [Header("Layer Settings")]
    public LayerMask stairsLayer; //
     public LayerMask enemyLayer; // 
    [Header("Input Data")]
    public InputReader inputReader;
    [HideInInspector]
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

    [Header("대쉬 잔상효과")]
    public GameObject playerModelForTrail;
    public Vector2 trailOffset = new Vector2(0f, -0.5f);


    [Header("착지딜레이 버니합금지")]
    public float landDashDelay = 0.5f;
    private float landTimer;

    [HideInInspector]
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
    [HideInInspector] public string anim_SprintStart = "To sprint";
    [HideInInspector] public string anim_SprintIng = "Sprinting";
    [HideInInspector] public string anim_SprintBreak = "SprintBreak";
    [HideInInspector] public string anim_SprintJump = "Sprint-jump-falling";
    [HideInInspector] public string anim_SprintLand = "sprint-falling-land";



    //스프린트 점프 쿨타임 변수
    public float sprintJumpCooldown = 0.5f; // 스프린트 점프 후 대기 시간
    [HideInInspector] private float sprintJumpCooldownTimer;
    [HideInInspector] public bool isJumpCut; //쿨타임때문에 점프가 캔슬되었는지 기록, 쿨타임이 안돌았는데 스프린트 점프시 다시 movestate로 돌아가는 코드로 스프린트애니메이션이 다시 재생되는 현상으로 컷내려고만듦

    
    [Header("강공 찌르기 애니메이션 관리변수")] //스킬X 스킬아님!!
    [HideInInspector]
    public string anim_ThrustReady = "MiddleToCharge"; // 기 모으기 모션 이름
    [HideInInspector]
    public string anim_ThrustAtk = "MiddleChargeATK";  // 찌르기 모션 이름

    [Header("찌르기(Thrust /F키) 데미지 뻥튀기 설정")]
    public float thrustInstantDamageRate = 1.0f;    // 즉발 시 데미지 배율 (기본 1배)
    public float thrustFullChargeDamageRate = 2.0f; // 풀차지 시 데미지 배율 (예: 2배)

    public void ExecuteThrustAttack(int index)
    {
        if (index < 0 || index >= attackLibrary.Count) return;

        // 핵심: 풀차지 깃발(isThrustCharged)이 켜져있으면 인스펙터의 '풀차지 배율'을, 아니면 '즉발 배율'을 가져옴!
        float damageBonus = isThrustCharged ? thrustFullChargeDamageRate : thrustInstantDamageRate;

        Debug.Log($"<color=orange>찌르기 발동! (풀차지 여부: {isThrustCharged} / 적용 배율: {damageBonus}배)</color>");

        currentActiveData = attackLibrary[index];
        gizmoDisplayTimer = 0.2f;

        // 3단계에서 수정할 함수에 배율(damageBonus)을 넘겨줍니다.
        PerformMeleeAttack(attackLibrary[index], damageBonus);
    }


    [Header("벽 애니메이션 관리변수")]
    [HideInInspector]
    public string anim_WallSlide = "walling";
    [HideInInspector]
    public string anim_WallJump = "WallJump";

    [HideInInspector]
    [Header("점프 직후 벽감지 쿨타임 추가")]
    public float wallGrabCooldown = 0.2f;
    public float wallGrabTimer;


    [Header("Thrust Attack Settings")]
    [HideInInspector] public AnimationCurve thrustVelocityCurve; // 찌르기 속도 그래프
    public float thrustDuration = 0.5f;        // 찌르기 전체 지속 시간 (초)


    [Header("공중공격 평타 설정")]
    public int currentAirActionCount = 0;   // 현재 공중 공격 횟수
    public int maxAirActions = 2;           // 최대 허용 횟수
    public float airAttackBounceForce = 2f; // 허공답보 (위로 살짝 뜨는 힘)

    [Header("공중 찍기공격")]
    public float diveDropSpeed = 25f; // 밑으로 내리꽂는 속도 (엄청 빨라야 찰집니다!)
    [HideInInspector] public string anim_DiveDrop = "AirHeavyDrop";
    [HideInInspector] public string anim_DiveLand = "AirHeavyAtk";

    [HideInInspector]
    [Header("방향키 공중 공격 애니메이션")]
    public string anim_AirUpAtk = "AirUpAtk";
    [HideInInspector]
    public string anim_AirDownAtk = "AirDownAtk";
    [HideInInspector]
    public bool hasUsedAirUp;   // 윗공격 1회 제한 스위치



    //[Header("스킬 데이터")]
    ////public float healAmount = 50f;  // 체력 회복량
    ////public float healMpCost = 30f;  // 마나 소모량
    ////public float HolySlashmp = 30;
    ////public float lightningMpCost = 30;

    [HideInInspector]
    [Header("히트 애니메이션 변수값")]
    public string anim_Hit = "Hit";

    [Header("히트 역경직 수치(기획자가 만지기)")]
    public float hitStopDuration = 0.1f;
    public float guardHitStopDuration = 0.05f;
    public float parryHitStopDuration = 0.15f;

    public void TriggerHitStop(float hitStopDuration = 0.05f)
    {
        StartCoroutine(HitStopRoutine(hitStopDuration));
    }


    [Header("방어 및 패리 애니메이션 관리")]
    [HideInInspector] public string anim_GuardNormal = "BlockNormal";
    [HideInInspector] public string anim_GuardOff = "Blockoff";
    [HideInInspector] public string anim_BlockHit = "BlockNormalHit";
    [HideInInspector] public string anim_GuardParry = "GuardParry";
    [HideInInspector] public string anim_ParryLightCounter = "ParryLightCounter";
    [HideInInspector] public string anim_ParryHeavyCounter = "ParryHeavyCounter";

    [Header("가드 및 패링 수치세팅")]
    public float parryWindowDuration = 0.2f;//패링 타이밍 시간 가드 키를 누른 직후, 정확히 0.2초 안에 적의 공격에 맞아야
    public float chipDamageMultiplier = 0.4f;//가드관통 데미지(내상) 가드 뎀감률
    public float guardKnockbackForce = 2f; // 일반 가드 넉백수치
    public float parryKnockbackForce = 1f; // 패리 성공 넉백수치
    [Header("패링 카운터 세팅")]
    public float parryCounterWindow = 0.3f;//유예시간(패링을하고 몇초안에 눌러야 나갈것인가(패리카운터를 위함)

    [HideInInspector] public float guardStartTime;

    // 에너미 공격의 유일한 진입점 (앞뒤 판별을 위해 몬스터 위치가 필요함)
    public void EvaluateAttack(float damage, Vector2 enemyPosition)
    {
        if (playerStats.isInvincible || playerStats.currentHp <= 0) return;

        // 패링 상태(ParryState 삭제됨) 대신 GuardState에서 처리함.
        // 카운터 상태들은 여전히 무적 로직 유지
        //if (StateMachine.CurrentState == ParryLightCounterState ||
        //    StateMachine.CurrentState == ParryHeavyCounterState) return;

        // 공격 방향 판별
        float dirToEnemy = enemyPosition.x - transform.position.x;
        bool isHitFromFront = (isFacingRight && dirToEnemy > 0) || (!isFacingRight && dirToEnemy < 0);

        // 1. 가드를 올렸고, 앞에서 날아온 공격일 때
        if (StateMachine.CurrentState == GuardState && isHitFromFront)
        {
            float timeSinceGuard = Time.time - guardStartTime;
            float pushDir = transform.position.x > enemyPosition.x ? 1f : -1f;

            if (timeSinceGuard <= parryWindowDuration)
            {
                // 패리 성공 (내상 HP 소멸 없음, 데미지 무효)
                Debug.Log("<color=cyan>패링 성공! 내상 유지 & 살짝 밀림</color>");
                rb.linearVelocity = new Vector2(pushDir * parryKnockbackForce, rb.linearVelocity.y);

                // 상태 전환 없이 GuardState 내부에서 애니메이션만 재생
                ((PlayerGuardState)GuardState).SetKnockbackLock(0.2f);
                ((PlayerGuardState)GuardState).TriggerParryAnimation();
                TriggerHitStop(parryHitStopDuration);
                return;
            }
            else
            {
                Debug.Log("<color=yellow>일반 가드!</color>");
                float chipDamage = damage * chipDamageMultiplier;
                playerStats.TakeDamage(chipDamage, true);
                if (playerStats.currentHp <= 0) return;

                float newInternalHp = chipDamage * playerStats.recoverableRatio;
                playerStats.SetInternalHp(newInternalHp);

                rb.linearVelocity = new Vector2(pushDir * guardKnockbackForce, rb.linearVelocity.y);

                // 넉백 보호 Lock 적용
                ((PlayerGuardState)GuardState).SetKnockbackLock(0.2f);
                animator.Play(anim_BlockHit, 0, 0f);
                TriggerHitStop(guardHitStopDuration);
                return;
            }
        }

        // 2. 가드를 안 했거나, 뒤통수를 맞았을 때 (쌩 피격)
        // 룰 2-1: 모아둔 내상 즉시 증발
        if (playerStats.loseInternalHpOnHit)
        {
            playerStats.currentRecoverableHp = 0f;
            Debug.Log("<color=red>피격당함! 내상 HP 즉시 증발</color>");
        }

        // 체력 깎고 HitState로 넘김 (쌩 피격이므로 무적 발동 -> false 전달)
        playerStats.TakeDamage(damage, false);
        if (playerStats.currentHp > 0)
        {
            StateMachine.ChangeState(HitState);

            TriggerHitStop(hitStopDuration); //히트스톱 트리거
        }
    }


    [Header("차지 공격 설정")]
    public float maxChargeTime = 1.5f; // 기획자가 인스펙터에서 조절할 풀차지 시간
    public int currentChargeLevel = 1; // 1: 즉발(또는 덜 모음), 2: 풀차지

    // 애니메이션 이벤트에서 호출할 차지 공격 전용 함수
    public void ExecuteChargeAttack(int baseIndex)
    {

        int finalIndex = baseIndex + (currentChargeLevel - 1);

        if (finalIndex < 0 || finalIndex >= attackLibrary.Count) return;

        Debug.Log($"홀리 슬래쉬 {currentChargeLevel}단계 발동! (인덱스: {finalIndex})");
        currentActiveData = attackLibrary[finalIndex]; // 기즈모에 그릴 SO 데이터 등록
        gizmoDisplayTimer = 0.2f;
        PerformMeleeAttack(attackLibrary[finalIndex]);
    }

    [HideInInspector]
    [Header("전투 세팅")]
    public Transform attackPoint;           // 타격 기준점
    public List<AttackDataSO> attackLibrary; // SO 파일들을 드래그해서 담는 곳

    
    [Header("실시간 기즈모 설정")]
    public bool useLiveGizmoOnly = true;     // true: 공격할 때만 뜸 / false: 기존처럼 에디터에서 항상 뜸
    private AttackDataSO currentActiveData;   // 현재 실행 중인 공격의 데이터 저장용
    private float gizmoDisplayTimer;          // 기즈모를 화면에 유지할 타이머
    private const float GIZMO_DURATION = 0.2f; // 기즈모가 켜져 있을 시간 (초단위, 취향껏 조절)


    // 애니메이션 이벤트에서 호출
    public void ExecuteAttack(int index)
    {
        if (index < 0 || index >= attackLibrary.Count)
        {
            return;
        }

        currentActiveData = attackLibrary[index];
        gizmoDisplayTimer = GIZMO_DURATION;
        PerformMeleeAttack(attackLibrary[index]);
    }

    private void PerformMeleeAttack(AttackDataSO data, float bonusMultiplier = 1f)
    {
        Debug.Log($"<color=red>[진짜 파일 확인]</color> 이름: {data.name} | 데미지 배율: {data.damageMultiplier}");
        float dir = isFacingRight ? 1f : -1f;
        Vector2 finalOffset = new Vector2(data.offset.x * dir, data.offset.y);
        Vector2 hitCenter = (Vector2)attackPoint.position + finalOffset;

        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(hitCenter, data.size, 0f, enemyLayer);
        float totalDealtDamage = 0f; // 다수 적 타격 시 총 데미지 합산 변수;
        bool hasHitEnemy = false;

        // [추가] 플레이어 스탯에서 기본 그로기 수치를 가져옵니다.
        float baseGroggy = playerStats.GetFinalGroggyPower();

        float totalAttackPower = playerStats.GetTotalAttackPower();

        foreach (Collider2D enemy in hitEnemies)
        {
            hasHitEnemy = true;

            EnemyFSM enemyFSM = enemy.GetComponent<EnemyFSM>();
            if (enemyFSM != null)
            {
                float finalDamage = totalAttackPower * data.damageMultiplier * bonusMultiplier;
                float finalGroggy = baseGroggy;
                // Debug.Log($"<color=cyan>[데미지 추적]</color> 기량스탯: {playerStats.statDex} / 근력스탯: {playerStats.statStr} / 기준치(SO): {playerStats.statBalance.baseAttackPerStat} / 캐릭터총공격력: {totalAttackPower} / 모션배율: {data.damageMultiplier}");
                // [추가] 인스펙터에서 설정한 Enum 카테고리에 맞춰 그로기 배율 곱하기
                switch (data.attackCategory)
                {
                    case AttackCategory.Light: finalGroggy *= data.lightAttackGroggyRatio; break;
                    case AttackCategory.Heavy: finalGroggy *= data.heavyAttackGroggyRatio; break;
                    case AttackCategory.JumpLight: finalGroggy *= data.jumpLightGroggyRatio; break;
                    case AttackCategory.JumpHeavy: finalGroggy *= data.jumpHeavyGroggyRatio; break;
                    case AttackCategory.ParryCounterLight: finalGroggy *= data.parryCounterLightGroggyRatio; break;
                    case AttackCategory.ParryCounterHeavy: finalGroggy *= data.parryCounterHeavyGroggyRatio; break;
                }



                // [수정] 몬스터에게 데미지와 그로기 데미지 2개를 전달!
                // ※ EnemyFSM 스크립트의 TakeDamage 함수 인자를 2개 받도록(float damage, float groggy) 수정해 주세요.
                enemyFSM.TakeDamage(finalDamage, finalGroggy);

                totalDealtDamage += finalDamage;
            }
            Debug.Log($"<color=orange>[타격 적중]</color> <b>{data.attackName}</b> -> {enemy.name}에게 적중! (타격 이펙트 생성 위치: {enemy.transform.position})");
        }

        // [기존 동일] 다수 타격 피흡 로직
        if (hasHitEnemy && playerStats.currentRecoverableHp > 0)
        {
            float healAmount = totalDealtDamage * playerStats.lifestealRatio;
            healAmount = Mathf.Min(healAmount, playerStats.currentRecoverableHp);

            playerStats.currentHp += healAmount;
            playerStats.currentRecoverableHp -= healAmount;

            if (playerStats.currentHp > playerStats.baseMaxHp) playerStats.currentHp = playerStats.baseMaxHp;

            Debug.Log($"<color=green>적중! 누적 데미지 {totalDealtDamage} 기반으로 {healAmount} 회복!</color>");
        }

        // [기존 동일] 적을 한 명이라도 맞췄을 때 한 번만 실행되는 '타격감' 연출
        if (hasHitEnemy)
        {
            //히트 소리 재생
            RuntimeManager.PlayOneShot("event:/Player/Interaction_Battle/Player_Attack_Hit", transform.position);
            
            // 고정값이 아니라, (누적 최종데미지, SO에 적힌 흡수비율) 2개를 넘겨줍니다!
            playerStats.RestoreMpByDamage(totalDealtDamage, data.mpRecoveryRatio);

            if (data.hitStopDuration > 0f)
            {
                StartCoroutine(HitStopRoutine(data.hitStopDuration));
                Debug.Log($"<color=yellow>[역경직 발생]</color> <b>{data.attackName}</b> 타격감 연출! {data.hitStopDuration}초 동안 정지!");
            }
        }
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        // 1. 기존 타임스케일 저장 
        // (보통 1f지만, 이미 슬로우 모션 중일 수도 있으니 원래 값을 기억해둡니다)
        float originalTimeScale = Time.timeScale;

        // 2. 게임 시간 정지! (역경직 발생)
        Time.timeScale = 0f;

        // 3.현실 시간(Realtime) 기준으로 대기
        // 타임스케일이 0이므로 일반 WaitForSeconds를 쓰면 ㅈ댐
        yield return new WaitForSecondsRealtime(duration);

        // 4. 시간이 다 되면 원래 타임스케일로 복구
        Time.timeScale = originalTimeScale;

        Debug.Log($"<color=yellow>[역경직 종료]</color> {duration}초 정지 해제!");
    }


    // 씬 뷰에서 공격 범위를 실시간 확인
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null || attackLibrary == null) return;
        float dir = isFacingRight ? 1f : -1f;
        Gizmos.color = Color.red;

        foreach (var data in attackLibrary)
        {
            if (data == null) continue;
            Vector2 drawPos = (Vector2)attackPoint.position + new Vector2(data.offset.x * dir, data.offset.y);
            Gizmos.DrawWireCube(drawPos, data.size);
        }
    }



    [Header("그래플링 훅 설정")]
    public LineRenderer grappleLine;      // 마법 로프 역할을 할 라인 렌더러
    public Transform shieldPoint;         // 줄이 뻗어나갈 시작점 (방패 쪽 손 위치에 빈 오브젝트 생성해서 할당)
    public Transform AirShieldPoint;      // 줄이 뻗어나갈 시작점 (공중)
    public float grappleSpeed = 30f;      // 날아가는 속도
    public float grappleMaxRange = 5f;      // 그래플링 가능 반경
    public float grappleMinRange = 0.1f;
    public LayerMask grappleLayer;        // "GrapplePoint" 레이어

    [HideInInspector]
    [Header("Grapple Animation")]
    public string anim_Grapple = "GrappleStart"; // 방금 주신 스프라이트 애니메이션 이름


    [Header("비탈길(Slope) 세팅")]
    public float maxSlopeAngle = 45f;
    [HideInInspector]
    public RaycastHit2D slopeHit; // 2D로 전환

    [Header("라이트닝 컷 변수")]
    [HideInInspector]
    public string anim_LightningReady = "LightningReady";   // 애니메이션 이름은 프로젝트에 맞게 수정
    [HideInInspector]
    public string anim_LightningCharge = "LightningCharge";
    [HideInInspector]
    public string anim_LightningAttack = "LightningAttack";


    [HideInInspector]
    [Header("힐 애니메이션")]
    public string anim_Heal = "Heal";


    [Header("이펙트(FX) 설정")] //middleATK 이펙트설정 
    //위치 조절용 오프셋 변수 
    public GameObject thrustChargeFxPrefab;
    public Transform thrustChargeFxSpawnPoint;


    [HideInInspector]
    [Header("변수 선언부")]

    public bool hasUsedAirDash;

    public bool CanDash
    {
        get
        {
            // 1. 쿨타임이 안 돌았으면 무조건 불가
            if (dashCooltimer > 0 || landTimer > 0) return false;

            // 2.핵심: 공중에 떠 있는데 이미 공중 대쉬를 한 번 썼다면 불가!
            if (!IsGrounded() && !OnSlope() && hasUsedAirDash) return false;

            return true;
        }
    }

    // 1. 비탈길인지 확인하고 경사면 정보(slopeHit)를 업데이트함
    private float defaultGravityScale;
    public bool IsActionLocked => StateMachine.CurrentState == HealState;
    [HideInInspector]
    public bool isThrustCharged = false;
    public bool isDashGracePeriod = false; // 대쉬 후 물리 튐 방지 유예 시간


    [HideInInspector]
    [Header("스킬 데이터 (SO)")]
    public AttackDataSO diveDropData; // 유니티 에디터에서 방금 만든 SO를 할당할 곳 오직 공중 강공격을 위한 선언;;

    public enum SkillSlot
    { HeavyAttack, LightningCut, Heal }
    public SkillSlot currentSkillSlot = SkillSlot.HeavyAttack; // 현재 선택된 스킬 슬롯

    [HideInInspector]
    [Header("체크포인트 애니메이션")]
    public string anim_ToRest = "ToRest";     // 앉는 과정 (0~5 프레임)
    public string anim_Resting = "Resting";   // 앉아서 대기 (0~8 프레임 반복)
    public string anim_Standing = "Standing"; // 일어나는 과정 (0~1 프레임)
    [HideInInspector]
    [Header("사망 애니메이션")]
    public string anim_DieGround = "Die"; // 땅 사망 모션
    public string anim_DieAir = "AirDie";// 공중 사망 모션

    [Header("코요테 타임")]
    public float coyoteTime = 0.1f; // 낭떠러지에서 떨어져도 이 시간 동안은 지상으로 판정
    private float lastGroundedTime; // 클래스 멤버 변수로 반드시 선언되어 있어야 함

    // 1. 순수 물리 판독기



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
    public PlayerParryLightCounterState ParryLightCounterState { get; private set; }
    public PlayerParryHeavyCounterState ParryHeavyCounterState { get; private set; }
    public PlayerGrappleState GrappleState { get; private set; }

    public PlayerDropState DropState { get; private set; }

    public PlayerLightningReadyState LightningReadyState { get; private set; }
    public PlayerLightningChargeState LightningChargeState { get; private set; }
    public PlayerLightningAttackState LightningAttackState { get; private set; }
    public PlayerHealState HealState { get; private set; }

    public PlayerRestState RestState { get; private set; }
    public PlayerStandUpState StandUpState { get; private set; }
    public PlayerDieState DieState { get; private set; }

    public PlayerHitState HitState { get; private set; }



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

        DiveDropState = new PlayerDiveDropState(this, StateMachine, anim_DiveDrop, diveDropData);
        DiveLandState = new PlayerDiveLandState(this, StateMachine, anim_DiveLand);
        AirUpAttackState = new PlayerAirUpAttackState(this, StateMachine, anim_AirUpAtk);

        GuardState = new PlayerGuardState(this, StateMachine, anim_GuardNormal);
        GuardOffState = new PlayerGuardOffState(this, StateMachine, anim_GuardOff);
        ParryLightCounterState = new PlayerParryLightCounterState(this, StateMachine, anim_ParryLightCounter);
        ParryHeavyCounterState = new PlayerParryHeavyCounterState(this, StateMachine, anim_ParryHeavyCounter);

        GrappleState = new PlayerGrappleState(this, StateMachine, anim_Grapple);

        DropState = new PlayerDropState(this, StateMachine, "Falling");

        LightningReadyState = new PlayerLightningReadyState(this, StateMachine, anim_LightningReady);
        LightningChargeState = new PlayerLightningChargeState(this, StateMachine, anim_LightningCharge);
        LightningAttackState = new PlayerLightningAttackState(this, StateMachine, anim_LightningAttack);
        HealState = new PlayerHealState(this, StateMachine, anim_Heal);
        RestState = new PlayerRestState(this, StateMachine, anim_ToRest);
        StandUpState = new PlayerStandUpState(this, StateMachine, anim_Standing);
        DieState = new PlayerDieState(this, StateMachine, anim_DieGround, anim_DieAir);
        HitState = new PlayerHitState(this, StateMachine, anim_Hit);


        rb = GetComponent<Rigidbody2D>(); // 2D로 변경
        cd = GetComponent<BoxCollider2D>(); // 2D로 변경
        defaultGravityScale = rb.gravityScale;
        playerStats = GetComponent<PlayerStats>(); //시작할 때 내 몸에 붙은 스탯 스크립트를 찾아둠

        if (playerModelForTrail != null)
        {
            var trailInstance = playerModelForTrail.GetComponent<TrailInstance>();
            if (trailInstance != null)
            {
                trailInstance.spawnOffset = trailOffset; // 내가 정한 값을 에셋에 덮어씌움
            }
        }
    }

    private void Start() => StateMachine.Initialize(IdleState);

    private void Update()
    {
        //테스트용 자살버튼 ㅋㅋ
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("<color=magenta>테스트용 자살 버튼 작동!</color>");

            // 만약 체력 UI도 같이 깎이는 걸 보고 싶다면 아래 주석 해제
            if (playerStats != null) playerStats.currentHp = 0;

            StateMachine.ChangeState(DieState);
            return;
        }
        //테스트용 쳐맞기버튼 ㅋㅋ
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("<color=magenta>테스트: 가상의 적에게 10 데미지 피격!</color>");

            // 플레이어의 살짝 앞(오른쪽을 보면 오른쪽, 왼쪽을 보면 왼쪽)에 가짜 적 위치를 만듦
            float dir = isFacingRight ? 1f : -1f;
            Vector2 fakeEnemyPos = transform.position + new Vector3(dir * 2f, 0f, 0f);

            // 이제 순수 계산기(TakeDamage) 대신, 통합 판독기(EvaluateAttack)로 데미지를 보냄!
            EvaluateAttack(10f, fakeEnemyPos);
        }


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


        if (gizmoDisplayTimer > 0)
        {
            gizmoDisplayTimer -= Time.deltaTime;
            if (gizmoDisplayTimer <= 0)
            {
                currentActiveData = null; // 시간이 다 되면 데이터를 비워 기즈모를 끕니다.
            }
        }

        //딱 idle, move에서만 가능
        //키보드 버튼은 하나인데, 땅이냐 공중이냐에 따라 다른 스킬을 나가게 해주는 분배기" 역할이 필요
        //평타는 콤보가 꼬이면 안 되니까 State 안에서만 부르고, 저건 언제든 튀어나가야 하는 스킬이니까 밖으로 뺌
        HandleGuardInput(); //가드입력을 최상단 감시하여 모든 공격상태를 캔슬
        HandleThrustAttackInput(); //강공찌르기 판독기 추가
        HandleActiveSkillInput();  // [수정] E키(OnSkill) 하나로 슬롯에 따라 스킬을 분배하는 통합 판독기




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
        if (StateMachine.CurrentState == LightningReadyState) return;
        if (StateMachine.CurrentState == LightningChargeState) return;
        if (StateMachine.CurrentState == LightningAttackState) return;
        if (StateMachine.CurrentState == HealState) return;
        if (StateMachine.CurrentState == HitState) return;

        bool isMidAir = StateMachine.CurrentState == JumpState ||
                        StateMachine.CurrentState == AirState ||
                        StateMachine.CurrentState == DropState ||
                        StateMachine.CurrentState == DashState;
        #endregion

        bool isAttacking = StateMachine.CurrentState is PlayerAttackState;
        bool isSprintLanding = StateMachine.CurrentState == LandState && isSprinting;

        if (!isAttacking)
        {
            if (Mathf.Abs(inputReader.MoveValue.x) < 0.1f && IsGrounded() && !isMidAir && !isSprintLanding
                && StateMachine.CurrentState != HitState && StateMachine.CurrentState != GuardState)
            {
                if (OnSlope())
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.gravityScale = 0f;
                }
                else
                {
                    rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                    rb.gravityScale = defaultGravityScale;
                }
            }
            else
            {
                rb.gravityScale = defaultGravityScale;
            }
        }

        // [내리막길 스프린트 시 튀어오름 방지]
        if (isSprinting && OnSlope() && !isMidAir && Mathf.Abs(inputReader.MoveValue.x) >= 0.1f)
        {
            Vector2 slopeDir = GetSlopeMoveDirection(rb.linearVelocity.normalized);
            float currentSpeed = rb.linearVelocity.magnitude;
            if (StateMachine.CurrentState != JumpState)
            {
                rb.linearVelocity = slopeDir * currentSpeed;
                rb.AddForce(Vector2.down * 50f, ForceMode2D.Force);
            }
        }

        //  기하학적 높이 비교를 통한 갈림길 완벽 분기
        if (StateMachine.CurrentState == DropState)
        {
            ToggleStairsCollision(false); // 밑점프는 묻지도 따지지도 않고 무조건 통과
        }
        else if (StateMachine.CurrentState == DashState && !IsGrounded())
        {
            // 공중(점프) 대쉬일 때는 비탈길에 안착하지 않고 유령처럼
            ToggleStairsCollision(false);
        }
        else if (StateMachine.CurrentState == AirState || StateMachine.CurrentState == JumpState)
        {
            // 공중 상태
            bool isBodyInside = Physics2D.OverlapBox(cd.bounds.center, cd.bounds.size * 0.9f, 0f, stairsLayer) != null;
            bool isStairUnder = Physics2D.BoxCast(new Vector2(cd.bounds.center.x, cd.bounds.min.y),
                new Vector2(cd.bounds.size.x * 0.7f, 0.1f), 0f, Vector2.down, 1.0f, stairsLayer).collider != null;

            if (isBodyInside) ToggleStairsCollision(false); // 머리가 박혀있으면 통과 (올라가는 중)
            else if (isStairUnder && rb.linearVelocity.y <= 0.1f) ToggleStairsCollision(true); // 하강 중엔 켜서 안착
            else ToggleStairsCollision(false);
        }
        else
        {
            // 지상 상태 (Idle, Move, Land, Dash/구르기 등)
            // OnSlope()의 사각지대를 없애기 위해 플레이어 발바닥 주변 비탈길을 물리 엔진 무시 여부와 상관없이 강제로 긁어옵니다.
            Vector2 boxCenter = new Vector2(cd.bounds.center.x, cd.bounds.min.y + 0.3f);
            Vector2 boxSize = new Vector2(cd.bounds.size.x * 0.8f, 0.6f);
            RaycastHit2D stairHit = Physics2D.BoxCast(boxCenter, boxSize, 0f, Vector2.down, 0.2f, stairsLayer);

            bool pureGround = IsPureGrounded();

            if (stairHit.collider != null && pureGround)
            {
                // 🚨 [핵심] 평지와 비탈길이 겹치는 "갈림길" 구역입니다!
                // 현재 닿은 비탈길의 '절반(Center Y)' 높이를 기준으로 내 발의 위치를 비교합니다.
                float slopeCenterY = stairHit.collider.bounds.center.y;
                float myFootY = cd.bounds.min.y;

                if (myFootY < slopeCenterY)
                {
                    // 1. 내 발이 비탈길 절반보다 아래에 있다 = "아랫길에서 비탈길 입구를 만남"
                    // 기획 의도: 점프하지 않았으므로 비탈길을 유령처럼 무시하고 통과해야 함!
                    ToggleStairsCollision(false);
                }
                else
                {
                    // 2. 내 발이 비탈길 절반보다 위에 있다 = "윗평지에서 내리막길을 만남"
                    // 기획 의도: 바닥이 끝날 때 스르륵 비탈길로 내려가야 하므로 켜둬야 함! (추락 방지)
                    ToggleStairsCollision(true);
                }
            }
            else if (stairHit.collider != null)
            {
                // 주변에 평지 없이 순수 비탈길만 밟고 있음 -> 당연히 타야 함
                ToggleStairsCollision(true);
            }
            else if (pureGround)
            {
                // 주변에 비탈길 없이 순수 평지만 밟고 있음 -> 통과 모드 대기
                ToggleStairsCollision(false);
            }
            else
            {
                // 허공 (예외 처리)
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
            if (IsGrounded())
            {
                StateMachine.ChangeState(DashAndSprintATK);
            }
            // 2. 공중(대쉬 중)이라면? 
            // 굳이 따로 안 만들고 우리가 고쳐놓은 그 '공중 공격'으로 바로 점프!
            else
            {
                if (currentAirActionCount >= maxAirActions) return;
                // 여기서 공중 공격 상태로 바로 넘김
                StateMachine.ChangeState(AirAttack1State);
            }
            return;
        }



        bool isReallyInAir = !IsGrounded() || StateMachine.CurrentState == JumpState || StateMachine.CurrentState == AirState;

        // 2. 만약 확실하게 땅에 있고, 콤보 중이 아니라면? -> 지상 1타 발동!
        if (!isReallyInAir && StateMachine.CurrentState != Attack1State)
        {
            StateMachine.ChangeState(Attack1State);
        }
        // 3. 공중 1타 분배
        else if (isReallyInAir && currentAirActionCount < maxAirActions
            && !(StateMachine.CurrentState is PlayerAirAttack1State)
            && !(StateMachine.CurrentState is PlayerAirAttack2State)
            && !(StateMachine.CurrentState is PlayerAirUpAttackState))
        {
            if (IsTooCloseToGround()) return; // 공중 윗공격 땅 x

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

    public void HandleActiveSkillInput()
    {

        if (StateMachine.CurrentState == RestState || StateMachine.CurrentState == StandUpState || StateMachine.CurrentState == DieState || StateMachine.CurrentState == HitState)
        {
            inputReader.HAttackPressed = false;
            return;
        }
        // 1. 공통 기모으기 입력 검사 (InputReader의 HAttackPressed 사용)
        if (!inputReader.HAttackPressed) return;

        if (StateMachine.CurrentState is PlayerAttackState)
        {
            Debug.Log("현재 공격 중이라 스킬 입력을 무시합니다.");
            inputReader.HAttackPressed = false;
            return;
        }

        // 스프린트중 스킬막음 없애려면 이거 지워라 기획;;
        if (isSprinting)
        {
            inputReader.HAttackPressed = false;
            return;
        }



        // 2. 땅에 있고, 현재 어떤 스킬 상태도 진행 중이 아닐 때만 진입
        if (IsGrounded() &&
            StateMachine.CurrentState != HeavyReadyState && StateMachine.CurrentState != HeavyChargeState && StateMachine.CurrentState != HeavyAttackState &&
            StateMachine.CurrentState != LightningReadyState && StateMachine.CurrentState != LightningChargeState && StateMachine.CurrentState != LightningAttackState &&
            StateMachine.CurrentState != HealState)
        {
            bool isSuccess = false;

            // 현재 활성화된 슬롯에 따라 전이할 상태 결정

            // 4. 결제 실패(마나 부족) 시 입력 강제 초기화
            //if (!isSuccess)
            //{
                inputReader.HAttackPressed = false;
            //}
        }
    }

    //강공 찌르기 //키 F
    public void HandleThrustAttackInput()
    {
        if (!inputReader.ThrustAttackPressed) return;

        if (StateMachine.CurrentState == RestState || StateMachine.CurrentState == StandUpState ||
            StateMachine.CurrentState == DieState || StateMachine.CurrentState == ParryLightCounterState ||
            StateMachine.CurrentState == ParryHeavyCounterState
        || StateMachine.CurrentState == HitState)
        {
            inputReader.ThrustAttackPressed = false;
            return;
        }

        //만약 패링중이면 패링카운터가 나가게끔 수정
        if (StateMachine.CurrentState == GuardState)
        {
            if (((PlayerGuardState)GuardState).isParrying)
            {
                // 패리 중일 때 찌르기 키를 누르면 
                // 입력을 소비하고 그 즉시 '강 카운터 상태'로 전환!
                inputReader.ThrustAttackPressed = false;
                StateMachine.ChangeState(ParryHeavyCounterState); // player. 안 붙여도 됨
                return;
            }
            else
            {
                // 일반 가드 중일 때는 찌르기가 나가면 안 되니까 입력 무시
                inputReader.ThrustAttackPressed = false;
                return;
            }
        }



        // 1. 이미 내려찍기 중이면 중복 방지
        if (StateMachine.CurrentState == DiveDropState || StateMachine.CurrentState == DiveLandState)
        {
            inputReader.ThrustAttackPressed = false;
            return;
        }

        bool isActuallyOnGround = IsGrounded() || OnSlope();

        // --- 지상/비탈길 (찌르기) ---
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
            // 어제 맞췄던 '공격 진행도' 로직 부활
            // 현재 공중 공격 중이라면 애니메이션이 어느 정도 진행되었는지 확인
            if (StateMachine.CurrentState is PlayerAirAttack1State ||
                StateMachine.CurrentState is PlayerAirAttack2State ||
                StateMachine.CurrentState is PlayerAirUpAttackState)
            {
                // normalizedTime이 0.4f~0.5f 정도는 지나야 하강 공격으로 캔슬 가능
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
        if (StateMachine.CurrentState == GuardState ||
            StateMachine.CurrentState == DieState ||
            StateMachine.CurrentState == HitState ||
            StateMachine.CurrentState == JumpState ||
            StateMachine.CurrentState == AirState ||
            StateMachine.CurrentState == DropState ||
            StateMachine.CurrentState == DiveDropState ||
            StateMachine.CurrentState == DiveLandState ||
            StateMachine.CurrentState == DashState || // 대시 중 가드 불가
            StateMachine.CurrentState == HealState || // 힐 중 가드 불가
            StateMachine.CurrentState == HeavyReadyState || StateMachine.CurrentState == HeavyChargeState || StateMachine.CurrentState == HeavyAttackState || // 해비 스킬 불가
            StateMachine.CurrentState == LightningReadyState || StateMachine.CurrentState == LightningChargeState || StateMachine.CurrentState == LightningAttackState) // 라이트닝 스킬 불가
        {
            return;
        }

        // 패리 카운터 상태일 때 즉시 씹힘 방지 쉴드 로직 헤비랑 라이트 둘이 나눠서 관리
        // =========================================================
        if (StateMachine.CurrentState == ParryLightCounterState)
        {
            float nTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            // LIGHT 카운터: 모션의 50%(0.5)가 지날 때까지 가드 캔슬 차단 (확정 타격 보장)
            if (nTime < 0.5f) return;
        }
        else if (StateMachine.CurrentState == ParryHeavyCounterState)
        {
            float nTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            // HEAVY 카운터: 모션의 30%(0.3)가 지날 때까지 가드 캔슬 차단 (빠른 캔슬 허용)
            if (nTime < 0.3f) return;
        }


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
        // 1. 발판 타겟 찾기 (가장 가까운 통과 가능한 발판 탐색)
        Vector2 rayOrigin = new Vector2(cd.bounds.center.x, cd.bounds.min.y + 0.1f);
        RaycastHit2D[] hits = Physics2D.BoxCastAll(rayOrigin, groundCheckSize, 0f, Vector2.down, 0.5f, groundLayer | stairsLayer);

        Collider2D bestTarget = null;
        float closestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            if (hit.collider == null || hit.collider == cd || hit.collider == ignoredDropCollider) continue;


            // hit.collider가 속한 레이어가 stairsLayer에 포함되는지 확인
            bool isStairs = ((1 << hit.collider.gameObject.layer) & stairsLayer) != 0;
            bool hasEffector = hit.collider.GetComponent<PlatformEffector2D>() != null;

            // 계단 레이어도 아니고, 이펙터도 없다면? -> 밑점프 불가능한 일반 쌩바닥!
            if (!isStairs && !hasEffector)
            {
                continue; // 타겟으로 잡지 말고 무시해라!
            }

            if (hit.distance < closestDist)
            {
                closestDist = hit.distance;
                bestTarget = hit.collider;
            }
        }

        // 2. 장애물 차단 검사 (bestTarget이 있을 때만 수행)
        if (bestTarget != null)
        {
            // --- 스프린트 상태에 따른 판정 수치 구분 ---
            // 스프린트면: 더 넓고(1.5f) 깊게(1.5f) 검사해서 깐깐하게 막음
            // 일반이면: 조금 좁고(0.95f) 얕게(0.9f) 검사해서 관대하게 허용
            float widthFactor = isSprinting ? 1.3f : 0.6f;
            float checkDistance = isSprinting ? 1.3f : 0.6f;

            Vector2 footPos = new Vector2(cd.bounds.center.x, cd.bounds.min.y);
            Vector2 checkSize = new Vector2(cd.bounds.size.x * widthFactor, 0.1f);

            RaycastHit2D[] checkHits = Physics2D.BoxCastAll(footPos, checkSize, 0f, Vector2.down, checkDistance, groundLayer | stairsLayer);

            foreach (var hit in checkHits)
            {
                // 자신, 지금 통과하려는 발판, 이미 무시 중인 발판은 제외
                if (hit.collider == cd || hit.collider == ignoredDropCollider || hit.collider == bestTarget) continue;

                // [차단 로직]
                // 감지된 것이 '통과 가능한 이펙터'가 없는 땅(Ground/Slope)이라면 좁은 공간으로 간주하고 차단
                if (hit.collider.GetComponent<PlatformEffector2D>() == null)
                {
                    Debug.Log($"[차단] (스프린트:{isSprinting}) {hit.collider.name} 때문에 밑점프 불가");
                    return null;
                }
            }
        }

        return bestTarget;
    }

    public bool OnSlope(bool isDashing = false)
    {
        if (cd == null || ignoreSlopeDetection ||
            (StateMachine != null && StateMachine.CurrentState == DropState))
            return false;

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
                // 핵심: 현재 저장된 slopeHit(이전 프레임의 비탈길)이 있다면, 
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

            if (angle <= 0.1f) return false;

            if (angle > 0.1f && angle <= maxSlopeAngle)
            {
                // 모서리 제외 필터 (Edge Detection)
                // 콜라이더의 좌우 끝단(min.x, max.x)으로부터 margin 만큼은 경사로 판정에서 제외합니다.
                // 이렇게 하면 모서리 끝에 도달했을 때 OnSlope가 false를 반환하여 평지로 전환됩니다.
                float margin = isDashing ? 0.05f : 0.5f;

                if (bestHit.point.x <= bestHit.collider.bounds.min.x + margin ||
                    bestHit.point.x >= bestHit.collider.bounds.max.x - margin)
                {
                    return false; // 모서리이므로 경사로 취급 안 함!
                }

                slopeHit = bestHit;
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

        return tangent * 0.95f;
    }


    // 공중 횟수 초기화 함수
    public void ResetAirActions()
    {
        currentAirActionCount = 0;
        hasUsedAirDash = false; // 추가: 땅에 닿으면 공중 대쉬 장전!
    }

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

        // 1. 착지 모션 등 강제 상태 예외
        if (StateMachine != null)
        {
            if (StateMachine.CurrentState == LandState) return true;
            // [핵심 추가] 밑점프 중이면 강제로 공중 판정!
            if (StateMachine.CurrentState == DropState) return false;
        }



        // 2. 바닥 체크 (BoxCast)
        Vector2 rayStartPos = new Vector2(cd.bounds.center.x, cd.bounds.min.y + 0.1f);
        RaycastHit2D[] hits = Physics2D.BoxCastAll(rayStartPos, groundCheckSize, 0f, Vector2.down, groundCheckDistance + 0.1f, GetCurrentGroundMask());

        bool isCurrentlyTouchingGround = false;

        foreach (var hit in hits)
        {
            // ignoredDropCollider가 있으면 밑점프 중이라는 뜻이니까 일단 무시
            if (hit.collider != null && hit.collider != ignoredDropCollider)
            {
                isCurrentlyTouchingGround = true;
                break;
            }
        }

        // 경사로 판정
        if (OnSlope()) isCurrentlyTouchingGround = true;

        // 3. 땅에 닿았다면 시간 갱신
        if (isCurrentlyTouchingGround)
        {
            lastGroundedTime = Time.time;
            return true;
        }

        // 4. Coyote Time 
        // 만약 밑점프 중(ignoredDropCollider != null)이라면 버퍼를 무시하고 바로 false를 리턴!
        // 이게 밑점프를 뚫어주는 열쇠야.
        if (ignoredDropCollider == null && Time.time < lastGroundedTime + 0.1f)
        {
            return true;
        }

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

        if (attackPoint == null || attackLibrary == null) return;

        float dir = isFacingRight ? 1f : -1f;

        // 실시간 모드: 게임 플레이 중 공격할 때만 잠깐 반짝이게 그리기
        if (useLiveGizmoOnly)
        {
            if (Application.isPlaying && currentActiveData != null)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.4f); // 살짝 투명한 빨간색 (속 채우기)
                Vector2 drawPos = (Vector2)attackPoint.position + new Vector2(currentActiveData.offset.x * dir, currentActiveData.offset.y);

                Gizmos.DrawCube(drawPos, currentActiveData.size);      // 속이 찬 박스
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(drawPos, currentActiveData.size);  // 테두리 선
            }
        }
        // 설계 모드: 게임이 꺼져있거나 인스펙터에서 튜닝할 때 
        else
        {
            for (int i = 0; i < attackLibrary.Count; i++)
            {
                var data = attackLibrary[i];
                if (data == null) continue;

                // 콤보 순서에 따라 색상 다르게 (1타: 빨강, 2타: 노랑, 3타: 파랑)
                Gizmos.color = i == 0 ? Color.red : (i == 1 ? Color.yellow : Color.blue);
                Vector2 drawPos = (Vector2)attackPoint.position + new Vector2(data.offset.x * dir, data.offset.y);

                Gizmos.DrawWireCube(drawPos, data.size);

            }
        }



    }

    public bool IsGroundAttacking()
    {
        if (StateMachine == null || StateMachine.CurrentState == null) return false;

        var currentState = StateMachine.CurrentState;

        // 순수 평타, 대시/스프린트 공격, 강공 찌르기까지만 가드 캔슬 허용!
        return currentState == Attack1State ||
               currentState == Attack2State ||
               currentState == Attack3State ||
               currentState == DashAndSprintATK ||
               currentState == ThrustReadyState ||
               currentState == ParryLightCounterState ||
               currentState == ParryHeavyCounterState;


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

    public bool IsNearGround()
    {
        if (cd == null) return false;

        Vector2 rayStartPos = new Vector2(cd.bounds.center.x, cd.bounds.min.y + 0.1f);

        // 0.6f 정도 거리 확인 (비탈길을 스무스하게 내려가고 있을 때는 무조건 이 안에 걸림)
        RaycastHit2D hit = Physics2D.BoxCast(rayStartPos, groundCheckSize, 0f, Vector2.down, 0.6f, GetCurrentGroundMask());

        // 밑점프 무시 콜라이더면 안 닿은 걸로 처리!
        return hit.collider != null && hit.collider != ignoredDropCollider;
    }






}