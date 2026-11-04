
using UnityEngine;

public class MidBossWebProjectile : MonoBehaviour
{
    [Header("Laser VFX")]
    [SerializeField] private ParticleSystem spitParticle;

    [Header("Particle Settings")]
    [SerializeField] private float particleSpeed = 20f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLog = true;

    private float speed;
    private float maxRange;
    private float bindDuration;

    private Vector2 startPos;
    private Vector2 currentDir;

    private bool isHit;

    private void Awake()
    {
        if (spitParticle == null)
        {
            ParticleSystem[] particles =
                GetComponentsInChildren<ParticleSystem>(true);

            foreach (ParticleSystem particle in particles)
            {
                if (particle.name == "Spit")
                {
                    spitParticle = particle;
                    break;
                }
            }
        }
    }

    public void Initialize(
        Vector2 dir,
        float spd,
        float range,
        float bind)
    {
        speed = spd;
        maxRange = range;
        bindDuration = bind;

        startPos = transform.position;
        currentDir = dir.normalized;

        isHit = false;

        SetupParticleDirection();
    }

    private void SetupParticleDirection()
    {
        if (spitParticle == null)
        {
            Debug.LogError(
                "[MidBossWebProjectile] Spit을 찾지 못했습니다.",
                this
            );
            return;
        }

        spitParticle.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );

        // 파티클을 월드 좌표 기준으로 이동
        var main = spitParticle.main;
        main.simulationSpace =
            ParticleSystemSimulationSpace.World;

        // 기존 방출 속도 제거
        main.startSpeed = 0f;

        // 기존 Velocity over Lifetime 설정을 덮어씀
        var velocity = spitParticle.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space =
            ParticleSystemSimulationSpace.World;

        velocity.x = new ParticleSystem.MinMaxCurve(
            currentDir.x * particleSpeed
        );

        velocity.y = new ParticleSystem.MinMaxCurve(
            currentDir.y * particleSpeed
        );

        velocity.z = new ParticleSystem.MinMaxCurve(0f);

        // 기존 회전과는 무관하게 파티클 이동
        spitParticle.Play(true);

        if (showDebugLog)
        {
            Debug.Log(
                "[MidBossWebProjectile] 파티클 방향: " +
                currentDir +
                " / X속도: " +
                currentDir.x * particleSpeed,
                this
            );
        }
    }

    private void Update()
    {
        if (isHit)
            return;

        transform.Translate(
            currentDir * speed * Time.deltaTime,
            Space.World
        );

        if (Vector2.Distance(startPos, transform.position)
            >= maxRange)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isHit)
            return;

        if (other.gameObject.layer !=
            LayerMask.NameToLayer("Player"))
        {
            return;
        }

        isHit = true;
        speed = 0f;

        if (showDebugLog)
        {
            Debug.Log(
                "[MidBossWebProjectile] 플레이어 충돌 감지",
                this
            );
        }

        // 실제 데미지는 EnemyHitbox에서 처리
        Destroy(gameObject, 0.4f);
    }
}
