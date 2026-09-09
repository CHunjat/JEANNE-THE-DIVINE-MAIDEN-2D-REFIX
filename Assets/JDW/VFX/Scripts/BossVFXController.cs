using UnityEngine;
using UnityEngine.VFX;
using System.Collections;
using System.Collections.Generic;

public class BossVFXController : MonoBehaviour
{
    [Header("스탬프 (Stamp_1/Stamp_2 중 원하는 걸로 연결, 풀링)")]
    [SerializeField] private ParticleSystem stampVFX;
    [SerializeField] private int stampVFXPoolSize = 3;
    [SerializeField] private float stampVFXDuration = 1.5f;

    [Header("더스트 (VFX Graph, 풀링)")]
    [SerializeField] private VisualEffect dustVFX;
    [SerializeField] private int dustVFXPoolSize = 3;
    [SerializeField] private float dustVFXDuration = 1.5f;

    [Header("랜딩 (Landing_1: 파티클시스템 / Landing_2: VFX Graph, 동시 재생, 풀링 없음)")]
    [SerializeField] private ParticleSystem landingVFX_1;
    [SerializeField] private VisualEffect landingVFX_2;

    [Header("스핏 (풀링 없음)")]
    [SerializeField] private ParticleSystem spitVFX;

    private List<ParticleSystem> stampVFXPool = new List<ParticleSystem>();
    private List<Coroutine> stampVFXCoroutines = new List<Coroutine>();

    private List<VisualEffect> dustVFXPool = new List<VisualEffect>();
    private List<Coroutine> dustVFXCoroutines = new List<Coroutine>();

    private void Awake()
    {
        // 스탬프 풀링
        stampVFX.gameObject.SetActive(false);
        stampVFXPool.Add(stampVFX);
        stampVFXCoroutines.Add(null);

        for (int i = 1; i < stampVFXPoolSize; i++)
        {
            ParticleSystem clone = Instantiate(stampVFX, stampVFX.transform.parent);
            clone.transform.localPosition = stampVFX.transform.localPosition;
            clone.transform.localRotation = stampVFX.transform.localRotation;
            clone.gameObject.SetActive(false);
            stampVFXPool.Add(clone);
            stampVFXCoroutines.Add(null);
        }

        // 더스트 풀링
        dustVFX.gameObject.SetActive(false);
        dustVFXPool.Add(dustVFX);
        dustVFXCoroutines.Add(null);

        for (int i = 1; i < dustVFXPoolSize; i++)
        {
            VisualEffect clone = Instantiate(dustVFX, dustVFX.transform.parent);
            clone.transform.localPosition = dustVFX.transform.localPosition;
            clone.transform.localRotation = dustVFX.transform.localRotation;
            clone.gameObject.SetActive(false);
            dustVFXPool.Add(clone);
            dustVFXCoroutines.Add(null);
        }

        landingVFX_1.gameObject.SetActive(false);
        landingVFX_2.gameObject.SetActive(false);

        spitVFX.gameObject.SetActive(false);
    }

    // ── 스탬프 (풀링, 1.5초 뒤 자동 비활성화) ────────────────────
    public void OnStampVFXPlay()
    {
        int index = GetAvailableStampVFXIndex();

        ParticleSystem ps = stampVFXPool[index];

        if (stampVFXCoroutines[index] != null)
            StopCoroutine(stampVFXCoroutines[index]);

        ps.gameObject.SetActive(true);
        ps.Clear();
        ps.Play();

        stampVFXCoroutines[index] = StartCoroutine(DisableStampVFXAfterDelay(index, stampVFXDuration));
    }

    private int GetAvailableStampVFXIndex()
    {
        for (int i = 0; i < stampVFXPool.Count; i++)
        {
            if (!stampVFXPool[i].gameObject.activeSelf)
                return i;
        }
        return 0; // 전부 사용 중이면 첫 슬롯 재활용
    }

    private IEnumerator DisableStampVFXAfterDelay(int index, float delay)
    {
        yield return new WaitForSeconds(delay);
        stampVFXPool[index].gameObject.SetActive(false);
        stampVFXCoroutines[index] = null;
    }

    // ── 더스트 (VFX Graph, 풀링, 1.5초 뒤 자동 비활성화) ────────────────────
    public void OnDustVFXPlay()
    {
        int index = GetAvailableDustVFXIndex();

        VisualEffect vfx = dustVFXPool[index];

        if (dustVFXCoroutines[index] != null)
            StopCoroutine(dustVFXCoroutines[index]);

        vfx.gameObject.SetActive(true);
        vfx.Reinit();
        vfx.Play();

        dustVFXCoroutines[index] = StartCoroutine(DisableDustVFXAfterDelay(index, dustVFXDuration));
    }

    private int GetAvailableDustVFXIndex()
    {
        for (int i = 0; i < dustVFXPool.Count; i++)
        {
            if (!dustVFXPool[i].gameObject.activeSelf)
                return i;
        }
        return 0; // 전부 사용 중이면 첫 슬롯 재활용
    }

    private IEnumerator DisableDustVFXAfterDelay(int index, float delay)
    {
        yield return new WaitForSeconds(delay);
        dustVFXPool[index].gameObject.SetActive(false);
        dustVFXCoroutines[index] = null;
    }

    // ── 랜딩 (Landing_1 + Landing_2 동시 재생, 풀링 없음) ────────────────────
    public void OnLandingVFXPlay()
    {
        landingVFX_1.gameObject.SetActive(true);
        landingVFX_1.Clear();
        landingVFX_1.Play();

        landingVFX_2.gameObject.SetActive(true);
        landingVFX_2.Reinit();
        landingVFX_2.Play();
    }

    public void OnLandingVFXStop()
    {
        landingVFX_1.gameObject.SetActive(false);
        landingVFX_2.gameObject.SetActive(false);
    }

    // ── 스핏 (풀링 없음) ────────────────────
    public void OnSpitVFXPlay()
    {
        spitVFX.gameObject.SetActive(true);
        spitVFX.Clear();
        spitVFX.Play();
    }

    public void OnSpitVFXStop()
    {
        spitVFX.gameObject.SetActive(false);
    }
}