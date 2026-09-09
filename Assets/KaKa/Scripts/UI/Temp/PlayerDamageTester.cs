using UnityEngine;

public class PlayerDamageTester : MonoBehaviour
{
    [Header("플레이어 스탯 참조")]
    [SerializeField] private PlayerStats playerStats;

    [Header("HP 일반 피격 테스트")]
    [SerializeField] private float hpDamageAmount = 30f;
    [SerializeField] private KeyCode hpDamageKey = KeyCode.F1;

    [Header("HP 가드 / 내상 테스트")]
    [SerializeField] private float guardDamageAmount = 30f;
    [SerializeField] private KeyCode guardDamageKey = KeyCode.F2;

    [Tooltip("실제 받은 피해 중 리커버리 HP로 표시할 비율")]
    [Range(0f, 1f)]
    [SerializeField] private float testRecoverableRatio = 0.5f;

    [Header("HP 회복 테스트")]
    [SerializeField] private float hpHealAmount = 30f;
    [SerializeField] private KeyCode hpHealKey = KeyCode.F3;

    [Header("내상 HP 제거 테스트")]
    [SerializeField] private KeyCode clearRecoverableKey = KeyCode.F4;

    [Header("MP 차감 테스트")]
    [SerializeField] private float mpDecreaseAmount = 100f;
    [SerializeField] private KeyCode mpDecreaseKey = KeyCode.P;

    [Header("MP 회복 테스트")]
    [SerializeField] private float mpIncreaseAmount = 50f;
    [SerializeField] private KeyCode mpIncreaseKey = KeyCode.O;

    private void Start()
    {
        if (playerStats == null)
        {
            playerStats = FindFirstObjectByType<PlayerStats>();
        }

        if (playerStats == null)
        {
            Debug.LogError(
                "PlayerDamageTester: 씬에서 PlayerStats를 찾지 못했습니다."
            );

            enabled = false;
        }
    }

    private void Update()
    {
        if (playerStats == null)
        {
            return;
        }

        // 일반 피격
        if (Input.GetKeyDown(hpDamageKey))
        {
            TestNormalDamage();
        }

        // 가드 피격 + 내상 HP 생성
        if (Input.GetKeyDown(guardDamageKey))
        {
            TestGuardDamage();
        }

        // HP 회복
        if (Input.GetKeyDown(hpHealKey))
        {
            TestHeal();
        }

        // 내상 HP 제거
        if (Input.GetKeyDown(clearRecoverableKey))
        {
            ClearRecoverableHp();
        }

        // MP 소모
        if (Input.GetKeyDown(mpDecreaseKey))
        {
            TestMpDecrease();
        }

        // MP 회복
        if (Input.GetKeyDown(mpIncreaseKey))
        {
            TestMpIncrease();
        }
    }

    private void TestNormalDamage()
    {
        float previousHp = playerStats.currentHp;

        // 실제 게임에서 사용하는 피격 함수 호출
        playerStats.TakeDamage(hpDamageAmount, false);

        float actualDamage = previousHp - playerStats.currentHp;

        Debug.Log(
            $"<color=red>[일반 피격 테스트]</color> " +
            $"요청 피해: {hpDamageAmount}, " +
            $"실제 피해: {actualDamage}, " +
            $"현재 HP: {playerStats.currentHp}"
        );
    }

    private void TestGuardDamage()
    {
        float previousHp = playerStats.currentHp;

        // isGuard를 true로 전달
        playerStats.TakeDamage(guardDamageAmount, true);

        float actualDamage = previousHp - playerStats.currentHp;

        if (actualDamage <= 0f || playerStats.currentHp <= 0f)
        {
            return;
        }

        // 실제로 감소한 체력을 기준으로 내상 HP 생성
        float addedRecoverableHp =
            actualDamage * testRecoverableRatio;

        // 내상 HP는 잃어버린 체력보다 커질 수 없음
        float maximumRecoverableHp =
            playerStats.GetMaxHp() - playerStats.currentHp;

        float nextRecoverableHp = Mathf.Clamp(
            playerStats.currentRecoverableHp + addedRecoverableHp,
            0f,
            maximumRecoverableHp
        );

        // PlayerStats의 내상 타이머도 함께 초기화
        playerStats.SetInternalHp(nextRecoverableHp);

        Debug.Log(
            $"<color=yellow>[가드 피격 테스트]</color> " +
            $"실제 피해: {actualDamage}, " +
            $"추가 내상 HP: {addedRecoverableHp}, " +
            $"총 내상 HP: {playerStats.currentRecoverableHp}"
        );
    }

    private void TestHeal()
    {
        float previousHp = playerStats.currentHp;

        playerStats.Heal(hpHealAmount);

        float actualHeal = playerStats.currentHp - previousHp;

        Debug.Log(
            $"<color=green>[HP 회복 테스트]</color> " +
            $"요청 회복: {hpHealAmount}, " +
            $"실제 회복: {actualHeal}, " +
            $"현재 HP: {playerStats.currentHp}"
        );
    }

    private void ClearRecoverableHp()
    {
        playerStats.currentRecoverableHp = 0f;

        Debug.Log(
            "<color=gray>[내상 테스트]</color> " +
            "리커버리 HP를 제거했습니다."
        );
    }

    private void TestMpDecrease()
    {
        // 직접 값을 깎지 않고 기존 MP 결제 함수 사용
        bool success = playerStats.TryConsumeMp(mpDecreaseAmount);

        Debug.Log(
            $"<color=cyan>[MP 소모 테스트]</color> " +
            $"성공 여부: {success}, 현재 MP: {playerStats.currentMp}"
        );
    }

    private void TestMpIncrease()
    {
        float maxMp = playerStats.GetMaxMp();

        playerStats.currentMp = Mathf.Min(
            playerStats.currentMp + mpIncreaseAmount,
            maxMp
        );

        Debug.Log(
            $"<color=blue>[MP 회복 테스트]</color> " +
            $"회복량: {mpIncreaseAmount}, 현재 MP: {playerStats.currentMp}"
        );
    }
}