using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

/// <summary>
/// BGM 전체를 관리하는 싱글톤 매니저.
/// - 씬 전환에도 파괴되지 않음 (DontDestroyOnLoad)
/// - 재생된 적 있는 곡은 stop 대신 Pause로 보관해서, 나중에 다시 나올 때 이어서 재생
/// - Zone 우선순위 + 전투 상태를 조합해서 "지금 뭘 재생할지" 스스로 판단
/// - 이 스크립트는 "숲"이니 "보스"니 하는 맵 정보를 전혀 모른다.
///   그런 정보는 전부 BGMZone / 보스 스크립트 쪽에서 이벤트 경로로 넘겨준다.
/// </summary>
public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance { get; private set; }

    [Header("페이드 설정")]
    [SerializeField] private float fadeOutDuration = 1.5f;
    [SerializeField] private float fadeInDuration = 1.5f; // 재개(Pause 해제)될 때만 사용

    [Header("보스 전투 BGM (페이즈 순서대로)")]
    [Tooltip("인덱스 0 = Phase 1, 1 = Phase 2 ...")]
    [SerializeField] private string[] bossBattleEventPaths;

    // 재생된 적 있는 곡들을 Pause 상태로 보관 (이벤트 경로 → 인스턴스)
    private readonly Dictionary<string, EventInstance> pausedInstances = new();

    private EventInstance currentInstance;
    private string currentEventPath;
    private Coroutine transitionRoutine;

    // ── 전투 상태 ────────────────────────────
    private bool isBattleStart = false;
    public bool IsBattleStart => isBattleStart;

    private int currentBattlePhase = 0; // 0-based index

    // ── Zone 스택 관리 ────────────────────────────
    private readonly List<BGMZone> activeZones = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        if (currentInstance.isValid())
        {
            currentInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            currentInstance.release();
        }

        foreach (var kvp in pausedInstances)
        {
            if (kvp.Value.isValid())
            {
                kvp.Value.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                kvp.Value.release();
            }
        }
        pausedInstances.Clear();
    }

    // ── Zone 등록/해제 (BGMZone에서 호출) ────────────────────────────
    public void RegisterZone(BGMZone zone)
    {
        if (!activeZones.Contains(zone))
            activeZones.Add(zone);

        RecalculateBGM();
    }

    public void UnregisterZone(BGMZone zone)
    {
        activeZones.Remove(zone);
        RecalculateBGM();
    }

    // ── 전투 상태 제어 (컷씬/보스가 호출) ────────────────────────────
    public void EnterBattle(int startPhase = 0)
    {
        isBattleStart = true;
        currentBattlePhase = startPhase;
        RecalculateBGM();
    }

    public void ExitBattle()
    {
        isBattleStart = false;
        currentBattlePhase = 0;
        RecalculateBGM(); // 전투 종료 → 다시 Zone 기준으로 판단
    }

    public void SetBattlePhase(int phaseIndex)
    {
        if (!isBattleStart) return; // 전투 중이 아니면 의미 없음
        currentBattlePhase = phaseIndex;
        RecalculateBGM();
    }

    // ── 핵심 판단 로직 ────────────────────────────
    private void RecalculateBGM()
    {
        string targetPath = DetermineTargetEventPath();

        if (targetPath == currentEventPath) return; // 이미 재생 중이면 아무것도 안 함

        if (transitionRoutine != null) StopCoroutine(transitionRoutine);
        transitionRoutine = StartCoroutine(TransitionRoutine(targetPath));
    }

    private string DetermineTargetEventPath()
    {
        // 1. 전투 중이면 무조건 보스 BGM이 우선
        if (isBattleStart && bossBattleEventPaths != null && bossBattleEventPaths.Length > 0)
        {
            int clampedPhase = Mathf.Clamp(currentBattlePhase, 0, bossBattleEventPaths.Length - 1);
            return bossBattleEventPaths[clampedPhase];
        }

        // 2. 평시엔 살아있는 Zone 중 priority 최고
        BGMZone best = activeZones
            .Where(z => z != null && z.IsValid)
            .OrderByDescending(z => z.priority)
            .FirstOrDefault();

        return best != null ? best.eventPath : null; // 없으면 무음
    }

    // ── 실제 전환 처리 (페이드아웃 → Pause → 다음 곡 재생/재개) ──────
    private IEnumerator TransitionRoutine(string targetPath)
    {
        // 1. 지금 재생 중인 곡이 있으면 페이드아웃 후 Pause
        if (currentInstance.isValid())
        {
            yield return FadeVolume(currentInstance, 1f, 0f, fadeOutDuration);
            currentInstance.setPaused(true);
            pausedInstances[currentEventPath] = currentInstance;
        }

        currentInstance = default;
        currentEventPath = targetPath;

        // targetPath가 null이면 그냥 무음 상태 유지 (아무 Zone도 없는 경우)
        if (string.IsNullOrEmpty(targetPath)) yield break;

        // 2. 이미 들어본 적 있는 곡(Pause 보관 중)이면 이어서 재생
        if (pausedInstances.TryGetValue(targetPath, out EventInstance existing) && existing.isValid())
        {
            pausedInstances.Remove(targetPath);
            currentInstance = existing;
            currentInstance.setVolume(0f);
            currentInstance.setPaused(false);
            yield return FadeVolume(currentInstance, 0f, 1f, fadeInDuration);
        }
        // 3. 처음 재생하는 곡이면 새로 만들고 즉시 풀볼륨
        else
        {
            currentInstance = RuntimeManager.CreateInstance(targetPath);
            currentInstance.setVolume(1f);
            currentInstance.start();
        }
    }

    private IEnumerator FadeVolume(EventInstance instance, float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float v = Mathf.Lerp(from, to, elapsed / duration);
            if (instance.isValid()) instance.setVolume(v);
            yield return null;
        }
        if (instance.isValid()) instance.setVolume(to);
    }
}