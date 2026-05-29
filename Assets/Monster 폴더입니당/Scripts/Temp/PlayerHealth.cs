using UnityEngine;

// =====================================================
// PlayerHealth.cs
// 임시 플레이어 체력 스크립트임.
//
// [중요] 이 스크립트는 Player 담당자의 코드가 없는 동안만 사용하는 임시 스크립트임.
// 나중에 Player 담당자의 코드와 합칠 때 이 파일을 삭제하거나 교체할 것.
//
// [역할]
// 몬스터/보스가 플레이어에게 데미지를 줄 때 이 스크립트의 TakeDamage()를 호출함.
// Console 창에서 피격/사망 로그를 확인할 수 있음.
// =====================================================
public class PlayerHealth : MonoBehaviour
{
    [Header("임시 플레이어 체력 수치 - 기획 확정 후 수정할 것")]
    [SerializeField] private float maxHp = 200f;    // 최대 체력
    [SerializeField] private float currentHp;       // 현재 체력 (인스펙터에서 실시간 확인 가능)

    private void Awake()
    {
        currentHp = maxHp;
    }

    // 피격 처리 - 몬스터 공격 스크립트에서 호출함
    public void TakeDamage(float amount)
    {
        currentHp -= amount;
        currentHp = Mathf.Max(currentHp, 0f);  // 0 아래로 내려가지 않게 처리

        Debug.Log($"[Player] 피격! 받은 데미지: {amount} / 남은 체력: {currentHp}/{maxHp}");

        if (currentHp <= 0)
        {
            Debug.Log("[Player] 체력이 0이 됨. 사망 처리 필요.");
            // 사망 처리 - Player 담당자가 채울 것
        }
    }

    // 현재 체력 반환 (UI 연동 등에서 사용)
    public float GetCurrentHp() => currentHp;

    // 최대 체력 반환
    public float GetMaxHp() => maxHp;

    // 테스트용 - 키보드 H를 누르면 10 데미지를 받음 (테스트 완료 후 삭제할 것)
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log("[PlayerHealth 테스트] H키 입력 - 10 데미지 적용.");
            TakeDamage(10f);
        }
    }
}