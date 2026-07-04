using UnityEngine;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    [Header("체력 (Health)")]
    public float maxHp = 100f;
    public float currentHp;
    public float MaxMp = 100f;
    public float currentMp;

    [Header("가드 리게인 (내상 HP) 시스템")]
    public float currentRecoverableHp = 0f; // 현재 쌓여있는 내상 HP
    public float recoverableRatio = 0.5f;   // 가드 데미지의 내상 전환율 (예: 50%)
    public float lifestealRatio = 0.2f;     // 가한 데미지의 피흡 비율 (예: 20%)

    [Header("내상 소멸 세팅")]
    public float internalHpDuration = 6f;   // 유지 시간 (6초)
    private float internalHpTimer = 0f;     // 유지 시간 타이머
    public bool loseInternalHpOnHit = true; // 피격 시 내상 즉시 소멸 여부

    [Header("기본 스탯 (Base Stats)")]
    public float baseAttackPower = 0f; // 기본 공격력
    public float defense = 0f;          // 방어력

    [Header("상태 이상 및 무적 (Status)")]
    public bool isInvincible = false;   // 무적 상태 여부
    public float invincibilityDuration = 0.5f; // 피격 시 무적 시간

    private PlayerController playerController;
    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        // 게임 시작 시 체력을 최대치로 초기화
        currentHp = maxHp;
        currentMp = MaxMp;
    }


    private void Update()
    {
        // 룰: 내상 HP 6초 유지 후 즉시 소멸
        if (currentRecoverableHp > 0)
        {
            internalHpTimer -= Time.deltaTime;
            if (internalHpTimer <= 0)
            {
                currentRecoverableHp = 0f;
                Debug.Log("<color=gray>6초 경과: 내상 HP 즉시 소멸!</color>");
            }
        }
    }

    public void SetInternalHp(float amount) //가드로 새로운 내성이 생겼을때 필요한 함수임 ㄷ
    {
        currentRecoverableHp = amount;
        internalHpTimer = internalHpDuration;
    }

    //파트너 호출용 몬스터가 플레이어를 때릴 때 사용할 함수
    public void TakeDamage(float amount, bool isGuard = false)
    {
        if (isInvincible || currentHp <= 0) return;

        float finalDamage = Mathf.Max(amount - defense, 1f);
        currentHp -= finalDamage;

        Debug.Log($"플레이어 피격! 받은 데미지: {finalDamage} / 남은 체력: {currentHp}");

        if (currentHp <= 0)
        {
            currentHp = 0;
            if (playerController.StateMachine.CurrentState != playerController.DieState)
            {
                playerController.StateMachine.ChangeState(playerController.DieState);
            }
        }
        else
        {
            // 가드로 데미지를 입은 게 아닐 때(쌩으로 쳐맞았을 때)만 무적 발동!
            if (!isGuard)
            {
                StartCoroutine(InvincibilityRoutine());
            }
        }
    }

    //힐 스킬이나 물약을 먹었을 때 사용할 함수
    public void Heal(float amount, float mpCost)
    {
        if (currentHp <= 0) return; // 죽었을 땐 힐 불가

        if (currentMp < mpCost)
        {
            Debug.Log("MP가 부족하여 힐을 사용할 수 없습니다!");
            return;
        }

        // 3. MP 소모 및 체력 회복
        currentMp -= mpCost;
        currentHp = Mathf.Min(currentHp + amount, maxHp);

        Debug.Log($"힐 사용! MP 소모: {mpCost} / 현재 체력: {currentHp} / 남은 MP: {currentMp}");
    }

    // 피격 시 잠시 무적이 되는 코루틴
    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

   
   
}