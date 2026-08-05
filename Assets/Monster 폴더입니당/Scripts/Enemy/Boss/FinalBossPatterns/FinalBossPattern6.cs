using UnityEngine;
using System.Collections;

// FinalBossPattern6.cs
// 데몬 누나 2페이즈 패턴 4 - 필살 패턴
public class FinalBossPattern6 : FinalBossPatternBase
{
    [Header("필살 패턴 설정 - 기획 확정 후 수정할 것")]
    [SerializeField] private float teleportForward = 30f;
    [SerializeField] private float teleportHeight = 4f;
    [SerializeField] private float clone1ForwardOffset = 40f;
    [SerializeField] private float clone2BackOffset = 10f;
    [SerializeField] private float handDropWidth = 40f;
    [SerializeField] private float handDropDuration = 0.5f;
    [SerializeField] private float boltSpeed = 10f;
    [SerializeField] private float boltRange = 30f;

    [Header("히트박스 연결 - 인스펙터에서 드래그해서 넣을 것")]
    [SerializeField] private GameObject handDropHitbox;

    [Header("프리팹 연결")]
    [SerializeField] private GameObject energyBoltPrefab;
    [SerializeField] private GameObject clone1Prefab;
    [SerializeField] private GameObject chaseClone2Prefab;

    protected override void Awake()
    {
        base.Awake();
        cooldown = 30f;

        if (handDropHitbox != null)
            handDropHitbox.SetActive(false);
    }

    protected override void OnExecute()
    {
        Debug.Log("FinalBossPattern6 필살 패턴 시전!");
        StartCoroutine(UltimateRoutine());
    }

    private IEnumerator UltimateRoutine()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null)
        {
            EndExecution();
            yield break;
        }

        Vector2 playerPos = playerObj.transform.position;
        Vector2 dirFromPlayerToBoss = ((Vector2)(transform.position - playerObj.transform.position)).normalized;
        Vector2 forwardDir = -dirFromPlayerToBoss;

        Transform visual = transform.Find("Visual");
        if (visual != null) visual.gameObject.SetActive(false);

        Vector2 bossNewPos = playerPos + forwardDir * teleportForward;
        bossNewPos.y += teleportHeight;
        transform.position = bossNewPos;

        if (visual != null) visual.gameObject.SetActive(true);

        Vector2 clone1Pos = playerPos + forwardDir * clone1ForwardOffset;
        GameObject clone1 = null;
        if (clone1Prefab != null)
            clone1 = Instantiate(clone1Prefab, clone1Pos, Quaternion.identity);

        Vector2 clone2Pos = playerPos - forwardDir * clone2BackOffset;
        GameObject clone2 = null;
        if (chaseClone2Prefab != null)
        {
            clone2 = Instantiate(chaseClone2Prefab, clone2Pos, Quaternion.identity);
            FinalBossChaseClone chaseScript = clone2.GetComponent<FinalBossChaseClone>();
            if (chaseScript != null)
                chaseScript.Initialize(playerObj.transform, transform);
        }

        yield return new WaitForSeconds(0.5f);

        StartCoroutine(HandDropRoutine(forwardDir));

        if (clone1 != null)
        {
            Vector2 boltDir = ((Vector2)playerObj.transform.position - clone1Pos).normalized;
            FireBolt(clone1Pos, boltDir);
            Destroy(clone1, 2f);
        }

        yield return new WaitForSeconds(handDropDuration + 0.5f);

        EndExecution();
    }

    private IEnumerator HandDropRoutine(Vector2 forwardDir)
    {
        if (handDropHitbox == null) yield break;

        handDropHitbox.SetActive(true);
        Debug.Log($"FinalBossPattern6 손 낙하 공격! 범위: {handDropWidth}m");
        yield return new WaitForSeconds(handDropDuration);
        handDropHitbox.SetActive(false);
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