using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections;

/// <summary>
/// Ambient 사운드를 재생하는 트리거 존.
/// 프리팹화해서 씬에 여러 개 배치, 콜라이더 크기만 조절해서 재사용.
/// 전투 중일 때는 BGMManager.IsBattleStart를 감지해서 볼륨을 억제한다.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class AmbientZone : MonoBehaviour
{
    [Header("FMOD 이벤트 경로")]
    [SerializeField] private string ambientEventPath;

    [Header("페이드 설정")]
    [SerializeField] private float fadeInDuration = 1.5f;
    [SerializeField] private float fadeOutDuration = 1.5f;
    [SerializeField] private float battleSuppressDuration = 0.8f; // 전투 중 억제될 때 페이드 속도

    private EventInstance ambientInstance;
    private bool isPlayerInside = false;
    private Coroutine volumeRoutine;
    private float currentVolume = 0f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInside = true;

        if (!ambientInstance.isValid())
        {
            ambientInstance = RuntimeManager.CreateInstance(ambientEventPath);
            ambientInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
            ambientInstance.start();
        }

        StartVolumeFade(GetTargetVolume(), fadeInDuration);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInside = false;
        StartVolumeFade(0f, fadeOutDuration, stopAfterFade: true);
    }

    private void Update()
    {
        // 존 안에 있는 동안, 전투 상태가 바뀌면 실시간으로 볼륨 재조정
        if (!isPlayerInside) return;
        if (!ambientInstance.isValid()) return;

        float target = GetTargetVolume();
        if (!Mathf.Approximately(target, currentVolume))
        {
            StartVolumeFade(target, battleSuppressDuration);
        }
    }

    // 전투 중이면 0, 아니면 1 (BGMManager가 없으면 그냥 정상 재생)
    private float GetTargetVolume()
    {
        if (BGMManager.Instance != null && BGMManager.Instance.IsBattleStart)
            return 0f;

        return 1f;
    }

    private void StartVolumeFade(float targetVolume, float duration, bool stopAfterFade = false)
    {
        // 오브젝트가 비활성 상태면 코루틴 대신 즉시 정지 처리
        if (!gameObject.activeInHierarchy)
        {
            if (ambientInstance.isValid())
            {
                ambientInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                ambientInstance.release();
            }
            return;
        }

        if (volumeRoutine != null) StopCoroutine(volumeRoutine);
        volumeRoutine = StartCoroutine(FadeVolumeRoutine(targetVolume, duration, stopAfterFade));
    }

    private IEnumerator FadeVolumeRoutine(float targetVolume, float duration, bool stopAfterFade)
    {
        float startVolume = currentVolume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            currentVolume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);

            if (ambientInstance.isValid())
                ambientInstance.setVolume(currentVolume);

            yield return null;
        }

        currentVolume = targetVolume;
        if (ambientInstance.isValid())
            ambientInstance.setVolume(currentVolume);

        if (stopAfterFade && ambientInstance.isValid())
        {
            ambientInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            ambientInstance.release();
        }
    }

    private void OnDestroy()
    {
        if (ambientInstance.isValid())
        {
            ambientInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            ambientInstance.release();
        }
    }
}