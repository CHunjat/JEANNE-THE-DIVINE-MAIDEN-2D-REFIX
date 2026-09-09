using UnityEngine;

public class PlayerGrappleState : PlayerState
{
    private Vector2 grappleTarget;          // 최종 선택된 그래플 타겟의 목적지 좌표
    private Transform currentShieldPoint;   // 발사 지점 오브젝트 (지상/공중 분리용)
    private bool isGrappling;               // 현재 그래플링 상태가 활성화되었는지 여부
    private bool isPulling;                 // 캐릭터가 타겟을 향해 실제로 끌려가고 있는 상태인지 여부

    public PlayerGrappleState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        // 이미 그래플링 중이면 진입 차단 (연타 버그 방지)
        if (isGrappling) return;

        // 플레이어 위치 기준으로 최대 사거리(grappleMaxRange) 내에 있는 그래플 레이어 오브젝트들을 원(Circle) 형태로 탐색
        Collider2D[] hits = Physics2D.OverlapCircleAll(player.transform.position, player.grappleMaxRange, player.grappleLayer);

        Transform bestTarget = null;
        float minAngle = 180f;            // 각도 조준용 초기값 (최대치)
        float closestDistance = Mathf.Infinity; // 🔥 [복구] 중립용 거리 소팅 초기값 (무한대)

        // 인풋 시스템으로부터 현재 방향키 입력 벡터값을 읽어옴
        float inputY = player.inputReader.MoveValue.y;
        float inputX = player.inputReader.MoveValue.x;

        // 지금 유저가 방향키를 입력 중인지 여부 판별 (절대값이 0.1f 이하이면 입력이 없는 '중립' 상태로 간주)
        bool isInputNeutral = Mathf.Abs(inputX) <= 0.1f && Mathf.Abs(inputY) <= 0.1f;

        // 조준 방향 결정 (입력이 있으면 방향키, 없으면 캐릭터가 보는 정면 방향)
        Vector2 aimDir;
        if (!isInputNeutral)
            aimDir = new Vector2(inputX, inputY).normalized; // 입력을 정규화(길이 1)하여 방향 벡터로 사용
        else
            aimDir = player.isFacingRight ? Vector2.right : Vector2.left; // 중립일 땐 바라보는 방향 벡터 사용

        // 연산의 정확성을 위해 3D 플레이어 위치를 2D 평면 좌표로 변환
        Vector2 playerPos2D = new Vector2(player.transform.position.x, player.transform.position.y);

        // 오버랩 서클로 탐지된 모든 타겟 후보군을 순회하며 최적의 타겟을 소팅
        foreach (Collider2D hit in hits)
        {
            Vector2 targetPos2D = new Vector2(hit.transform.position.x, hit.transform.position.y);
            Vector2 dirToTarget = (targetPos2D - playerPos2D).normalized; // 플레이어에서 타겟을 향하는 방향 벡터
            float distance = Vector2.Distance(playerPos2D, targetPos2D);   // 두 오브젝트 사이의 실제 거리

            // 거리 제한 검사 (최소 사거리 미만이거나 최대 사거리를 초과하면 타겟 후보에서 즉시 탈락)
            if (distance < player.grappleMinRange || distance > player.grappleMaxRange) continue;

            // 플레이어 위치에서 타겟 위치까지 가상의 레이저(Linecast)를 쏩니다.
            // 만약 그 레이저가 땅이나 벽(Ground ,Wall)에 부딪힌다면, 벽 뒤에 있는 타겟이므로 무시(continue)합니다.
            LayerMask obstacleMask = player.groundLayer | player.wallLayer;

            if (Physics2D.Linecast(player.transform.position, hit.transform.position, obstacleMask))
            {
                continue;
            }
            // 조준 방향(aimDir)과 타겟 방향(dirToTarget) 사이의 사잇각(0도 ~ 180도)을 계산
            float angle = Vector2.Angle(aimDir, dirToTarget);

            // 방향키를 입력 중일 때는 45도 깐깐한 시야각 적용
            // 방향키 중립(안 누름)일 때는 머리 위(90도)나 대각선 지형도 잡힐 수 있게 허용 각도를 95도
            float allowedAngle = isInputNeutral ? 95f : 45f;

            // 허용 시야각 이내에 존재할 때만 타겟 알고리즘 작동
            if (angle <= allowedAngle)
            {
                // 🔥 [복구 완료] 입력 상태에 따라 소팅 기준을 이원화함
                if (!isInputNeutral)
                {
                    // 1️방향키 입력 중: [조준선 각도]가 가장 칼같이 정면인 놈 우선
                    if (angle < minAngle)
                    {
                        minAngle = angle;
                        bestTarget = hit.transform;
                    }
                }
                else
                {
                    // 2️방향키 중립: 시야 범위 내에서 [물리적 거리]가 가장 가까운 놈 우선!
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        bestTarget = hit.transform;
                    }
                }
            }
        }

        // 범위 내에 유효한 타겟이 아예 없으면 그래플 상태를 취소하고 이전 상태(지상/공중)로 즉시 튕겨냄
        if (bestTarget == null)
        {
            stateMachine.ChangeState(player.IsGrounded() ? player.IdleState : player.AirState);
            return;
        }

        base.Enter();
        isGrappling = true; // 그래플 메커니즘 가동 플래그 ON

        // 최종 선택된 타겟의 위치를 목적지로 저장
        grappleTarget = bestTarget.position;

        // 회전 처리 (목적지가 플레이어의 좌/우 어디에 있냐에 따라 캐릭터 방향을 강제로 돌려줌)
        float directionX = grappleTarget.x - player.transform.position.x;
        if (Mathf.Abs(directionX) > 0.1f) player.FlipController(directionX);

        // 견인 시작 전 중력을 끄고, 기존에 가지고 있던 물리 속도(이동/추락 힘)를 0으로 초기화
        player.rb.gravityScale = 0f; // 2D gravityScale 사용
        player.SetVelocity(0f, 0f);

        // [진입 조건 분기] 땅에 붙어있으면서 동시에 '공중 연타로 들어온 상태(!isPulling)'가 아닐 때만 지상 로직 실행
        if (player.IsGrounded() && !isPulling)
        {
            currentShieldPoint = player.shieldPoint; // 지상용 발사체 앵커 포인트 할당
            isPulling = false;                     // 지상은 선딜레이 애니메이션이 있으므로 당겨지는 플래그는 잠시 대기
            if (player.grappleLine != null) player.grappleLine.enabled = false; // 와이어 라인은 애니메이션 특정 프레임에서 켜기 위해 대기
            player.animator.Play(player.anim_Grapple, 0, 0f); // 지상 전용 그래플 발사 징검다리 애니메이션 재생
        }
        else
        {
            // 공중에서 진입했거나, 공중 비행 중 연타로 재진입했을 때 (공중 연타 버그 방어막)
            currentShieldPoint = player.AirShieldPoint; // 공중용 발사체 앵커 포인트 할당
            isPulling = true;                           // 공중은 선딜레이 없이 즉시 견인(Pulling) 시작
            //player.animator.SetBool(player.anim_Grapple, false); // 지상 애니메이션용 파라미터 Off

            // 와이어 렌더러 활성화 및 시작점(방패 위치), 끝점(타겟 위치) 실시간 매핑
            if (player.grappleLine != null && currentShieldPoint != null)
            {
                player.grappleLine.enabled = true;
                player.grappleLine.SetPosition(0, currentShieldPoint.position);
                player.grappleLine.SetPosition(1, grappleTarget);
            }
            // 찰나의 연타 순간에 지상 애니메이션 찌꺼기가 화면에 튀는 것을 막기 위해 Jump 모션을 0번 레이어의 0프레임부터 강제 재생
            player.animator.Play("Jump", 0, 0f);
        }
        isGrappling = true;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // [지상 진입 전용 예외처리] 선딜레이 애니메이션이 끝나기를 기다리는 루틴
        if (isGrappling && !isPulling)
        {
            var stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);
            // 지상 발사 애니메이션이 90% 이상 진행되었다면 밧줄이 타겟에 꽂힌 것으로 간주하고 견인 모드로 전환
            if (stateInfo.IsName(player.anim_Grapple) && stateInfo.normalizedTime >= 0.99f)
            {
                isPulling = true; // 견인 시작
                if (player.grappleLine != null && currentShieldPoint != null)
                {
                    player.grappleLine.enabled = true;
                    player.grappleLine.SetPosition(0, currentShieldPoint.position);
                    player.grappleLine.SetPosition(1, grappleTarget);
                }
            }
        }

        // 그래플링 비행 도중 유저가 점프 키를 누르면 점프 캔슬 액션 발동
        if (player.inputReader.JumpPressed && player.CanJump)
        {
            player.inputReader.JumpPressed = false; // 예약 입력 방지용 수동 소거
            stateMachine.ChangeState(player.JumpState); // 즉시 점프 상태로 강제 전환
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        // 실제로 끌려가는 상태(isPulling)일 때 주 실시간 물리 연산 처리
        if (isGrappling && isPulling)
        {
            // 캐릭터가 이동하므로 와이어의 시작점(방패 위치)을 실시간 갱신
            if (player.grappleLine != null && currentShieldPoint != null)
                player.grappleLine.SetPosition(0, currentShieldPoint.position);

            // 등속도(MoveTowards) 계산식으로 플레이어의 위치를 목적지(grappleTarget)까지 강제로 당겨옴
            player.transform.position = Vector2.MoveTowards(
                player.transform.position,
                grappleTarget,
                player.grappleSpeed * Time.deltaTime
            );

            // [도착 판정] 타겟과의 거리가 0.9 유닛보다 가까워지면 와이어 견인 종료
            if (Vector2.Distance(player.transform.position, grappleTarget) < 0.9f)
            {
                // 날아가던 방향 벡터를 실시간으로 계산 (플레이어 -> 타겟)
                Vector2 grappleDir = (grappleTarget - (Vector2)player.transform.position).normalized;

                float targetY = grappleTarget.y;
                float playerY = player.transform.position.y;

                // [방향성 물리 보정] 위로 쏠 때와 아래로 쏠 때의 물리적 튕김 분리
                if (targetY > playerY + 0.5f)
                {
                    // [위로 올라갈 때 관성 강화]
                    // X축: 가던 방향으로 미끄러지듯 발사 (보존 배율 0.65f)
                    // Y축: 턱을 자연스럽고 강하게 넘어가도록 기본 점프 힘의 0.85배로 강력하게 쏘아올림
                    float launchX = grappleDir.x * player.grappleSpeed * 0.65f;
                    float launchY = player.jumpForce * 1.0f;

                    player.rb.linearVelocity = new Vector2(launchX, launchY);
                }
                else
                {
                    // [수평 또는 밑으로 쏠 때 관성 강화]
                    // X축: 비행 가속도감을 받도록 수평 관성력을 대폭 보존 (보존 배율 0.85f)
                    // Y축: 위로 솟구치지 않고 하방 궤적 힘을 유지하며 묵직하게 떨어짐
                    float launchX = grappleDir.x * player.grappleSpeed * 0.85f;
                    float launchY = grappleDir.y * player.grappleSpeed * 0.85f;

                    player.rb.linearVelocity = new Vector2(launchX, launchY);
                }

                // 이동을 완수했으므로 공중 액션 연계를 위해 더블점프 카운트와 대시 쿨타임을 초기화해줌 (기획 사양)
                player.RestJumpCount();
                player.ResetDashCooldown();
                stateMachine.ChangeState(player.AirState); // 부드러운 낙하 및 관성 비행 연계를 위해 공중 상태로 상태 전이
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        if (player.grappleLine != null) player.grappleLine.enabled = false; // 탈출 시 와이어 그래픽 끄기
        player.rb.gravityScale = 1f; // 2D gravityScale 사용 (원래대로 다시 중력 적용)
        isGrappling = false; // 다음 그래플 진입을 위해 내부 상태 초기화
        isPulling = false;   // 다음 그래플 진입을 위해 내부 상태 초기화
    }
}