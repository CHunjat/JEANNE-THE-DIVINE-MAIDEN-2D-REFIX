using UnityEngine;
using System.Collections;

// FinalBossPattern5.cs
// 데몬 누나 2페이즈 패턴 3 - 손톱과 에너지 볼트
public class FinalBossPattern5 : FinalBossPatternBase
{
    [Header("손톱 및 에너지 볼트 설정 - 기획 확정 후 수정할 것")]
    [SerializeField] private float hitboxActiveDuration = 0.15f;
    [SerializeField] private float shortDelay = 0.2f;
    [SerializeField] private float longDelay = 0.5f;
    [SerializeField] private float clone1Offset = 10f;
    [SerializeField] private float clone2Offset = 5f;
    [SerializeField] private float boltSpeed = 10f;
    [SerializeField] private float boltRange = 30f;
    [SerializeField] private float boltFireInterval = 0.4f;

    [Header("히트박스 연결 - 인스펙터에서 드래그해서 넣을 것")]
    [SerializeField] private GameObject leftClawHitbox;
    [SerializeField] private GameObject rightClawHitbox;

    [Header("프리팹 연결")]
    [SerializeField] private GameObject clonePrefab;
    [SerializeField] private GameObject energyBoltPrefab;

    protected override void Awake()
    {
        base.Awake();
        cooldown = 6f;

        if (leftClawHitbox != null) leftClawHitbox.SetActive(false);
        if (rightClawHitbox != null) rightClawHitbox.SetActive(false);
    }

    protected override void OnExecute()
    {
        Debug.Log("FinalBossPattern5 손톱과 에너지 볼트 시전!");
        StartCoroutine(ClawAndBoltRoutine());
    }

    private IEnumerator ClawAndBoltRoutine()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null)
        {
            EndExecution();
            yield break;
        }

        yield return StartCoroutine(ClawSet(leftClawHitbox));

        Vector2 playerPosAtSpawn = playerObj.transform.position;
        Vector2 dirFromBossToPlayer = ((Vector2)(playerObj.transform.position - transform.position)).normalized;

        Vector2 clone1Pos = (Vector2)playerObj.transform.position + dirFromBossToPlayer * clone1Offset;
        Vector2 clone2Pos = (Vector2)playerObj.transform.position + dirFromBossToPlayer * clone2Offset;

        if (clonePrefab != null)
        {
            StartCoroutine(CloneBoltRoutine(clone1Pos, playerPosAtSpawn));
            StartCoroutine(CloneBoltRoutine(clone2Pos, playerPosAtSpawn));
        }

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

    private IEnumerator CloneBoltRoutine(Vector2 clonePos, Vector2 targetPos)
    {
        if (clonePrefab == null || energyBoltPrefab == null) yield break;

        GameObject clone = Instantiate(clonePrefab, clonePos, Quaternion.identity);
        Vector2 fireDir = (targetPos - clonePos).normalized;

        FireBolt(clonePos, fireDir);
        yield return new WaitForSeconds(boltFireInterval);
        FireBolt(clonePos, fireDir);

        Destroy(clone, boltFireInterval + 0.5f);
        Debug.Log("FinalBossPattern5 분신 에너지 볼트 2회 발사 후 소멸.");
    }

    private void FireBolt(Vector2 origin, Vector2 dir)
    {
        if (energyBoltPrefab == null) return;

        GameObject bolt = Instantiate(energyBoltPrefab, origin, Quaternion.identity);
        EnergyBoltProjectile projectile = bolt.GetComponent<EnergyBoltProjectile>();
        if (projectile != null)
            projectile.Initialize(dir, boltSpeed, boltRange);
    }
}