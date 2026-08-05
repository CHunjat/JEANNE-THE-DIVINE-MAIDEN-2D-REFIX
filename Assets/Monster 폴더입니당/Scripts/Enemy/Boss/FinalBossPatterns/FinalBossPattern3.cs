using UnityEngine;
using System.Collections;

// FinalBossPattern3.cs
// 데몬 누나 2페이즈 패턴 1 - 강화 손톱 베기
public class FinalBossPattern3 : FinalBossPatternBase
{
    [Header("강화 손톱 베기 설정 - 기획 확정 후 수정할 것")]
    [SerializeField] private float hitboxActiveDuration = 0.15f;
    [SerializeField] private float shortDelay = 0.2f;
    [SerializeField] private float longDelay = 0.5f;
    [SerializeField] private float cloneSpawnDelay = 0.1f;

    [Header("히트박스 연결 - 인스펙터에서 드래그해서 넣을 것")]
    [SerializeField] private GameObject leftClawHitbox;
    [SerializeField] private GameObject rightClawHitbox;

    [Header("분신 설정")]
    [SerializeField] private GameObject clonePrefab;
    [SerializeField] private float cloneOffset = 3f;

    protected override void Awake()
    {
        base.Awake();
        cooldown = 5f;

        if (leftClawHitbox != null) leftClawHitbox.SetActive(false);
        if (rightClawHitbox != null) rightClawHitbox.SetActive(false);
    }

    protected override void OnExecute()
    {
        Debug.Log("FinalBossPattern3 강화 손톱 베기 시전!");
        StartCoroutine(EnhancedClawRoutine());
    }

    private IEnumerator EnhancedClawRoutine()
    {
        yield return StartCoroutine(ClawSet(leftClawHitbox));

        yield return new WaitForSeconds(cloneSpawnDelay);
        SpawnClone();

        yield return new WaitForSeconds(longDelay);

        yield return StartCoroutine(ClawSet(rightClawHitbox));

        EndExecution();
    }

    private IEnumerator ClawSet(GameObject hitbox)
    {
        if (hitbox != null) hitbox.SetActive(true);
        yield return new WaitForSeconds(hitboxActiveDuration);
        if (hitbox != null) hitbox.SetActive(false);

        yield return new WaitForSeconds(shortDelay);

        if (hitbox != null) hitbox.SetActive(true);
        yield return new WaitForSeconds(hitboxActiveDuration);
        if (hitbox != null) hitbox.SetActive(false);
    }

    private void SpawnClone()
    {
        if (clonePrefab == null)
        {
            Debug.LogWarning("FinalBossPattern3 clonePrefab이 연결되지 않음.");
            return;
        }

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null) return;

        Vector2 dirFromBossToPlayer = ((Vector2)(playerObj.transform.position - transform.position)).normalized;
        Vector2 clonePos = (Vector2)playerObj.transform.position + dirFromBossToPlayer * cloneOffset;

        Instantiate(clonePrefab, clonePos, Quaternion.identity);
        Debug.Log("FinalBossPattern3 분신 소환됨.");
    }
}