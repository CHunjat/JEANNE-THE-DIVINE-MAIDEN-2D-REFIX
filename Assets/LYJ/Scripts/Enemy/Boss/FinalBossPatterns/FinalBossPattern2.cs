using UnityEngine;

// FinalBossPattern2.cs
// 데몬 누나 1페이즈 패턴 2 - 에너지 볼트
public class FinalBossPattern2 : FinalBossPatternBase
{
    [Header("에너지 볼트 설정 - 기획 확정 후 수정할 것")]
    [SerializeField] private float preDelay = 0.3f;
    [SerializeField] private float boltSpeed = 10f;
    [SerializeField] private float boltRange = 30f;

    [Header("히트박스 연결 - 인스펙터에서 EnergyBolt 프리팹을 드래그해서 넣을 것")]
    [SerializeField] private GameObject energyBoltPrefab;

    private Transform owner;

    protected override void Awake()
    {
        base.Awake();
        cooldown = 4f;
        owner = transform;
    }

    protected override void OnExecute()
    {
        Debug.Log("FinalBossPattern2 에너지 볼트 시전!");
        Invoke(nameof(FireBolt), preDelay);
    }

    private void FireBolt()
    {
        if (energyBoltPrefab == null)
        {
            Debug.LogWarning("FinalBossPattern2 energyBoltPrefab이 연결되지 않음. 인스펙터에서 프리팹을 넣을 것.");
            EndExecution();
            return;
        }

        GameObject playerObj = GameObject.FindWithTag("Player");
        Vector2 dir = playerObj != null
            ? ((Vector2)(playerObj.transform.position - owner.position)).normalized
            : Vector2.right;

        GameObject bolt = Instantiate(energyBoltPrefab, owner.position, Quaternion.identity);
        EnergyBoltProjectile projectile = bolt.GetComponent<EnergyBoltProjectile>();

        if (projectile != null)
            projectile.Initialize(dir, boltSpeed, boltRange);

        EndExecution();
    }
}