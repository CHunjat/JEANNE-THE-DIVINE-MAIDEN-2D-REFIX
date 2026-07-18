using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerVFXController : MonoBehaviour
{
    [Header("스킬 차징 VFX")]
    [SerializeField] private ParticleSystem lightningChargingVFX;
    [SerializeField] private ParticleSystem ChargingCompleteVFX;

    [Header("일렉트릭 VFX 1 (풀링, 원본 하나만 연결)")]
    [SerializeField] private ParticleSystem electricVFX_1;
    [SerializeField] private int electricVFX_1_PoolSize = 3;
    [SerializeField] private float electricVFX_1_LingerDuration = 1f;

    [Header("일렉트릭 VFX 2")]
    [SerializeField] private ParticleSystem electricVFX_2;

    [Header("일렉트릭 VFX 3")]
    [SerializeField] private ParticleSystem electricVFX_3;

    [Header("패링")]
    [SerializeField] private ParticleSystem ParryVFX;
    [SerializeField] private int parryVFXPoolSize = 3;
    [SerializeField] private float parryVFXDuration = 0.5f;

    [Header("패링 카운터 하드 (슬래쉬)")]
    [SerializeField] private ParticleSystem ParryCountHardVFX;
    [SerializeField] private int parryCountHardVFXPoolSize = 3;
    [SerializeField] private float parryCountHardVFXDuration = 0.5f;


    private List<ParticleSystem> parryVFXPool = new List<ParticleSystem>();
    private List<Coroutine> parryVFXCoroutines = new List<Coroutine>();

    private List<ParticleSystem> parryCountHardVFXPool = new List<ParticleSystem>();
    private List<Coroutine> parryCountHardVFXCoroutines = new List<Coroutine>();

    private List<ParticleSystem> electricVFX_1_Pool = new List<ParticleSystem>();
    private List<Coroutine> electricVFX_1_StopRoutines = new List<Coroutine>();
    private int electricVFX_1_ActiveIndex = -1;

    private Coroutine chargingCompleteRoutine;

    private void Awake()
    {
        lightningChargingVFX.gameObject.SetActive(false);
        ChargingCompleteVFX.gameObject.SetActive(false);

        electricVFX_1.gameObject.SetActive(false);
        electricVFX_1_Pool.Add(electricVFX_1);
        electricVFX_1_StopRoutines.Add(null);

        for (int i = 1; i < electricVFX_1_PoolSize; i++)
        {
            ParticleSystem clone = Instantiate(electricVFX_1, electricVFX_1.transform.parent);
            clone.transform.localPosition = electricVFX_1.transform.localPosition;
            clone.transform.localRotation = electricVFX_1.transform.localRotation;
            clone.gameObject.SetActive(false);
            electricVFX_1_Pool.Add(clone);
            electricVFX_1_StopRoutines.Add(null);
        }

        electricVFX_2.gameObject.SetActive(false);
        electricVFX_3.gameObject.SetActive(false);

        ParryVFX.gameObject.SetActive(false);
        parryVFXPool.Add(ParryVFX);
        parryVFXCoroutines.Add(null);

        for (int i = 1; i < parryVFXPoolSize; i++)
        {
            ParticleSystem clone = Instantiate(ParryVFX, ParryVFX.transform.parent);
            clone.transform.localPosition = ParryVFX.transform.localPosition;
            clone.transform.localRotation = ParryVFX.transform.localRotation;
            clone.gameObject.SetActive(false);
            parryVFXPool.Add(clone);
            parryVFXCoroutines.Add(null);
        }

        ParryCountHardVFX.gameObject.SetActive(false);
        parryCountHardVFXPool.Add(ParryCountHardVFX);
        parryCountHardVFXCoroutines.Add(null);

        for (int i = 1; i < parryCountHardVFXPoolSize; i++)
        {
            ParticleSystem clone = Instantiate(ParryCountHardVFX, ParryCountHardVFX.transform.parent);
            clone.transform.localPosition = ParryCountHardVFX.transform.localPosition;
            clone.transform.localRotation = ParryCountHardVFX.transform.localRotation;
            clone.gameObject.SetActive(false);
            parryCountHardVFXPool.Add(clone);
            parryCountHardVFXCoroutines.Add(null);
        }
    }

    // ── 스킬 차징 ────────────────────────────────
    public void OnSkillLightningChargingStartVFX()
    {
        lightningChargingVFX.gameObject.SetActive(true);
        lightningChargingVFX.Clear();
        lightningChargingVFX.Play();

        chargingCompleteRoutine = StartCoroutine(PlayChargingCompleteVFXAfterDelay(1.5f));
    }

    private IEnumerator PlayChargingCompleteVFXAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ChargingCompleteVFX.gameObject.SetActive(true);
        ChargingCompleteVFX.Clear();
        ChargingCompleteVFX.Play();
    }

    public void OnSkillLightningChargingStopVFX()
    {
        lightningChargingVFX.gameObject.SetActive(false);

        if (chargingCompleteRoutine != null)
        {
            StopCoroutine(chargingCompleteRoutine);
            chargingCompleteRoutine = null;
        }
    }

    // ── 일렉트릭 VFX 1 (차징 중 유지, 놓으면 1초 잔향, 겹쳐 시전 가능) ──────────
    public void OnElectricVFX1Start()
    {
        int index = GetAvailableElectricVFX1Index();

        if (electricVFX_1_StopRoutines[index] != null)
        {
            StopCoroutine(electricVFX_1_StopRoutines[index]);
            electricVFX_1_StopRoutines[index] = null;
        }

        ParticleSystem ps = electricVFX_1_Pool[index];
        ps.gameObject.SetActive(true);
        ps.Clear();
        ps.Play();

        electricVFX_1_ActiveIndex = index;
    }

    public void OnElectricVFX1Stop()
    {
        if (electricVFX_1_ActiveIndex < 0) return;

        int index = electricVFX_1_ActiveIndex;
        electricVFX_1_ActiveIndex = -1;

        electricVFX_1_StopRoutines[index] = StartCoroutine(LingerThenDisable(index));
    }

    private int GetAvailableElectricVFX1Index()
    {
        for (int i = 0; i < electricVFX_1_Pool.Count; i++)
        {
            if (!electricVFX_1_Pool[i].gameObject.activeSelf && electricVFX_1_StopRoutines[i] == null)
                return i;
        }
        return (electricVFX_1_ActiveIndex + 1) % electricVFX_1_Pool.Count;
    }

    private IEnumerator LingerThenDisable(int index)
    {
        yield return new WaitForSeconds(electricVFX_1_LingerDuration);
        electricVFX_1_Pool[index].gameObject.SetActive(false);
        electricVFX_1_StopRoutines[index] = null;
    }

    // ── 일렉트릭 VFX 2 ────────────────────────────────
    public void OnElectricVFX2Start()
    {
        electricVFX_2.gameObject.SetActive(true);
        electricVFX_2.Clear();
        electricVFX_2.Play();
    }

    public void OnElectricVFX2Stop()
    {
        electricVFX_2.gameObject.SetActive(false);
    }

    // ── 일렉트릭 VFX 3 ────────────────────────────────
    public void OnElectricVFX3Start()
    {
        electricVFX_3.gameObject.SetActive(true);
        electricVFX_3.Clear();
        electricVFX_3.Play();
    }

    public void OnElectricVFX3Stop()
    {
        electricVFX_3.gameObject.SetActive(false);
    }

    // ── 패링 VFX (풀링, 재생하면 0.5초 뒤 자동 비활성화) ────────────────────────────────
    public void OnParryVFXPlay()
    {
        int index = GetAvailableParryVFXIndex();

        ParticleSystem ps = parryVFXPool[index];

        if (parryVFXCoroutines[index] != null)
            StopCoroutine(parryVFXCoroutines[index]);

        ps.gameObject.SetActive(true);
        ps.Clear();
        ps.Play();

        parryVFXCoroutines[index] = StartCoroutine(DisableParryVFXAfterDelay(index, parryVFXDuration));
    }

    private int GetAvailableParryVFXIndex()
    {
        for (int i = 0; i < parryVFXPool.Count; i++)
        {
            if (!parryVFXPool[i].gameObject.activeSelf)
                return i;
        }
        return 0; // 전부 사용 중이면 첫 슬롯 재활용
    }

    private IEnumerator DisableParryVFXAfterDelay(int index, float delay)
    {
        yield return new WaitForSeconds(delay);
        parryVFXPool[index].gameObject.SetActive(false);
        parryVFXCoroutines[index] = null;
    }

    // ── 패링 카운터 하드 VFX (슬래쉬, 풀링, 재생하면 0.5초 뒤 자동 비활성화) ────────────
    public void OnParryCountHardVFXPlay()
    {
        int index = GetAvailableParryCountHardVFXIndex();

        ParticleSystem ps = parryCountHardVFXPool[index];

        if (parryCountHardVFXCoroutines[index] != null)
            StopCoroutine(parryCountHardVFXCoroutines[index]);

        ps.gameObject.SetActive(true);
        ps.Clear();
        ps.Play();

        parryCountHardVFXCoroutines[index] = StartCoroutine(DisableParryCountHardVFXAfterDelay(index, parryCountHardVFXDuration));
    }

    private int GetAvailableParryCountHardVFXIndex()
    {
        for (int i = 0; i < parryCountHardVFXPool.Count; i++)
        {
            if (!parryCountHardVFXPool[i].gameObject.activeSelf)
                return i;
        }
        return 0; // 전부 사용 중이면 첫 슬롯 재활용
    }

    private IEnumerator DisableParryCountHardVFXAfterDelay(int index, float delay)
    {
        yield return new WaitForSeconds(delay);
        parryCountHardVFXPool[index].gameObject.SetActive(false);
        parryCountHardVFXCoroutines[index] = null;
    }

    // ── VFX 전체 정지 (방어 로직 포함) ────────────────────
    public void StopAllPlayerVFX()
    {
        if (chargingCompleteRoutine != null)
        {
            StopCoroutine(chargingCompleteRoutine);
            chargingCompleteRoutine = null;
        }
        if (lightningChargingVFX != null && lightningChargingVFX.gameObject.activeSelf)
        {
            lightningChargingVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            lightningChargingVFX.gameObject.SetActive(false);
        }

        if (electricVFX_1_ActiveIndex >= 0)
        {
            int index = electricVFX_1_ActiveIndex;
            electricVFX_1_ActiveIndex = -1;

            if (electricVFX_1_StopRoutines[index] != null)
            {
                StopCoroutine(electricVFX_1_StopRoutines[index]);
                electricVFX_1_StopRoutines[index] = null;
            }
            if (electricVFX_1_Pool[index] != null && electricVFX_1_Pool[index].gameObject.activeSelf)
            {
                electricVFX_1_Pool[index].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                electricVFX_1_Pool[index].gameObject.SetActive(false);
            }
        }

        if (electricVFX_2 != null && electricVFX_2.gameObject.activeSelf)
        {
            electricVFX_2.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            electricVFX_2.gameObject.SetActive(false);
        }

        if (electricVFX_3 != null && electricVFX_3.gameObject.activeSelf)
        {
            electricVFX_3.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            electricVFX_3.gameObject.SetActive(false);
        }
    }
}