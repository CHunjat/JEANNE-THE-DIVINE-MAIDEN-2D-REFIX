using UnityEngine;
using System.Collections;

public class MidBossPattern5 : BossPatternBase
{
    [Header("클리어링 설정 (여기서 넉백 수치 조절)")]
    [SerializeField] private float clearingRange = 3f;           // 플레이어가 이 거리 안에 들어와야만 스킬 발동
    [SerializeField] private float knockbackDistance = 10f;      // 플레이어를 밀어내는 거리 (기획서 기준 10m)
    [SerializeField] private float knockbackDuration = 0.3f;     // 얼마나 휙! 하고 빠르게 밀어낼지 (시간이 짧을수록 빠름)
    [SerializeField] private float hitboxActiveDuration = 0.5f;  // 밀쳐내기 판정 유지 시간 

    [Header("히트박스 연결 (건드리면 안되는 거!)")]
    [SerializeField] private GameObject clearingHitbox;

    private Animator visualAnimator;

    private void Awake()
    {
        cooldown = 5f;  // 이것도 임시 쿨타임!
        visualAnimator = GetComponentInChildren<Animator>();

        // 처음엔 밀쳐내기 판정 꺼두기
        if (clearingHitbox != null)
            clearingHitbox.SetActive(false);
    }

    protected override void OnExecute()
    {
        // 플레이어 찾아오기
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null) return;

        // 보스랑 플레이어 사이의 거리를 재서, 조건(clearingRange) 밖이면 스킬 취소
        float dist = Vector2.Distance(transform.position, playerObj.transform.position);
        if (dist > clearingRange) return;

        Debug.Log("패턴 5번(클리어링) 시작! 플레이어 쳐내기!");
        if (visualAnimator != null) visualAnimator.Play("buff/attack 4");

        // 딜레이 없이 바로 밀쳐내는 로직 시작
        StartCoroutine(ClearingRoutine(playerObj));
    }

    private IEnumerator ClearingRoutine(GameObject playerObj)
    {
        // 1. 방어 스킬이니까 선딜레이 없이 바로 밀쳐내기 판정 켬! (데미지는 인스펙터에서 0으로 세팅됨)
        if (clearingHitbox != null)
            clearingHitbox.SetActive(true);

        // 2. 기획서대로 플레이어를 뒤로 확 밀어버리는 함수 실행
        ApplyClearing(playerObj);

        // 3. 판정 켜두는 시간 대기
        yield return new WaitForSeconds(hitboxActiveDuration);

        // 4. 끝났으니 다시 판정 끄기
        if (clearingHitbox != null)
            clearingHitbox.SetActive(false);
    }

    // [플레이어 밀쳐내는 핵심 로직]
    private void ApplyClearing(GameObject playerObj)
    {
        // 보스랑 플레이어의 좌우 위치 차이 계산
        float xDiff = playerObj.transform.position.x - transform.position.x;
        Vector2 knockbackDir;

        // 기획서 조건: 플레이어와 보스가 완벽히 겹쳐있으면 일단 오른쪽(전방)으로 밀고, 
        // 아니면 플레이어가 있는 방향(왼쪽 or 오른쪽)으로 밀어버림.
        if (Mathf.Abs(xDiff) < 0.01f)
            knockbackDir = ((Vector2)(playerObj.transform.position - transform.position)).normalized;
        else
            knockbackDir = xDiff > 0 ? Vector2.right : Vector2.left;

        if (knockbackDir == Vector2.zero)
            knockbackDir = Vector2.right;

        // 플레이어의 물리 엔진(Rigidbody2D)에 직접 속도를 줘서 날려버림
        Rigidbody2D playerRb = playerObj.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            float knockbackSpeed = knockbackDistance / knockbackDuration;
            playerRb.linearVelocity = knockbackDir * knockbackSpeed; // 유니티 6부터는 velocity 대신 linearVelocity 사용
        }
    }
}