using UnityEngine;

/// <summary>
/// PlayerStats.cs를 전혀 수정하지 않고, 이미 준비된 가드 리게인(내상 체력) 필드들을
/// 실제로 채워주는 브릿지. PlayerStats에 이미 있는 public 멤버만 사용합니다:
/// currentHp, currentRecoverableHp, recoverableRatio, lifestealRatio,
/// loseInternalHpOnHit, SetInternalHp(), GetMaxHp()
///
/// [역할 1] 매 프레임 currentHp 감소를 감지해서 내상(회색) 체력 풀을 채움
/// [역할 2] 다른 스크립트가 "적에게 데미지를 줬다"고 알려주면 그만큼 내상 체력을 실체력으로 회복
///          → RecoverFromDamageDealt(damageDealt)를 공격 판정 스크립트에서 호출해줘야 완성됨
/// </summary>
public class PlayerVitalsBridge : MonoBehaviour
{
    [Header("연결")]
    public PlayerStats playerStats;

    private float lastKnownHp;
    private bool initialized;

    private void Update()
    {
        if (playerStats == null) return;

        if (!initialized)
        {
            lastKnownHp = playerStats.currentHp;
            initialized = true;
            return;
        }

        float delta = lastKnownHp - playerStats.currentHp;

        // 체력이 줄었다 = 피해를 받았다
        if (delta > 0.01f)
        {
            float damageBasedRecoverable = delta * playerStats.recoverableRatio;

            float newRecoverable = playerStats.loseInternalHpOnHit
                ? damageBasedRecoverable                                          // 맞을 때마다 기존 내상은 날아가고 이번 피해분으로 새로 시작
                : playerStats.currentRecoverableHp + damageBasedRecoverable;       // 기존 내상에 이번 피해분을 누적

            playerStats.SetInternalHp(newRecoverable); // 내부적으로 타이머도 같이 리셋됨
        }

        lastKnownHp = playerStats.currentHp;
    }

    /// <summary>
    /// 플레이어가 적에게 데미지를 입혔을 때, 그 데미지 값을 그대로 넘겨 호출.
    /// lifestealRatio 비율만큼 내상 체력 풀에서 실제 체력으로 전환됩니다.
    /// (호출 지점은 아직 미확정 - 공격 판정 스크립트 확인 후 연결 예정)
    /// </summary>
    public void RecoverFromDamageDealt(float damageDealt)
    {
        if (playerStats == null || playerStats.currentRecoverableHp <= 0f) return;

        float recovered = damageDealt * playerStats.lifestealRatio;
        recovered = Mathf.Min(recovered, playerStats.currentRecoverableHp);

        if (recovered <= 0f) return;

        playerStats.currentHp = Mathf.Min(playerStats.currentHp + recovered, playerStats.GetMaxHp());
        playerStats.SetInternalHp(playerStats.currentRecoverableHp - recovered);

        // lastKnownHp를 여기서도 갱신해줘야 Update()에서 이 회복분을 "피해"로 오인하지 않음
        lastKnownHp = playerStats.currentHp;
    }
}