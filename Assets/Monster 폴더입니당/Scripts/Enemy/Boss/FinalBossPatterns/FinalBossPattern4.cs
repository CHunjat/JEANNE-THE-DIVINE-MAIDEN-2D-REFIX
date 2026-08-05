using UnityEngine;
using System.Collections;

// FinalBossPattern4.cs
// 데몬 누나 2페이즈 패턴 2 - 강화 에너지 볼트
public class FinalBossPattern4 : FinalBossPatternBase
{
    [Header("강화 에너지 볼트 설정 - 기획 확정 후 수정할 것")]
    [SerializeField] private float teleportRange = 30f;
    [SerializeField] private float cloneSpacing = 5f;
    [SerializeField] private float boltSpeed = 12f;
    [SerializeField] private float boltRange = 30f;
    [SerializeField] private float fireInterval = 0.3f;

    [Header("프리팹 연결 - 인스펙터에서 드래그해서 넣을 것")]
    [SerializeField] private GameObject energyBoltPrefab;
    [SerializeField] private GameObject clonePrefab;

    protected override void Awake()
    {
        base.Awake();
        cooldown = 7f;
    }

    protected override void OnExecute()
    {
        Debug.Log("FinalBossPattern4 강화 에너지 볼트 시전!");
        StartCoroutine(EnhancedBoltRoutine());
    }

    private IEnumerator EnhancedBoltRoutine()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null)
        {
            EndExecution();
            yield break;
        }

        Transform visual = transform.Find("Visual");
        if (visual != null) visual.gameObject.SetActive(false);

        Vector2 dirToPlayer = ((Vector2)(playerObj.transform.position - transform.position)).normalized;
        transform.position = (Vector2)playerObj.transform.position + dirToPlayer * teleportRange;

        if (visual != null) visual.gameObject.SetActive(true);
        Debug.Log("FinalBossPattern4 순간이동 완료.");

        Vector2 fireDir = -dirToPlayer;
        Vector2 clone1Pos = (Vector2)transform.position + fireDir * cloneSpacing;
        Vector2 clone2Pos = (Vector2)transform.position + fireDir * (cloneSpacing * 2f);

        GameObject clone1 = null, clone2 = null;
        if (clonePrefab != null)
        {
            clone1 = Instantiate(clonePrefab, clone1Pos, Quaternion.identity);
            clone2 = Instantiate(clonePrefab, clone2Pos, Quaternion.identity);
        }

        yield return new WaitForSeconds(0.2f);

        FireBolt(clone1Pos, fireDir);
        yield return new WaitForSeconds(fireInterval);

        FireBolt(clone2Pos, fireDir);
        yield return new WaitForSeconds(fireInterval);

        FireBolt(transform.position, fireDir);

        if (clone1 != null) Destroy(clone1, 0.5f);
        if (clone2 != null) Destroy(clone2, 0.5f);

        EndExecution();
    }

    private void FireBolt(Vector2 origin, Vector2 dir)
    {
        if (energyBoltPrefab == null) return;

        GameObject bolt = Instantiate(energyBoltPrefab, origin, Quaternion.identity);
        EnergyBoltProjectile projectile = bolt.GetComponent<EnergyBoltProjectile>();
        if (projectile != null)
            projectile.Initialize(dir, boltSpeed, boltRange);

        Debug.Log($"FinalBossPattern4 에너지 볼트 발사! 위치: {origin}");
    }
}