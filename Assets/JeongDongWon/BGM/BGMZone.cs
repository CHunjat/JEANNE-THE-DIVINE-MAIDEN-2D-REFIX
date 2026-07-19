using UnityEngine;

/// <summary>
/// BGM 구역을 표시하는 트리거 콜라이더.
/// 플레이어가 들어오면 BGMManager에 자신을 등록하고, 나가면 등록 해제한다.
/// 실제로 무슨 곡을 틀지, 언제 전환할지는 BGMManager가 전담 판단한다.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class BGMZone : MonoBehaviour
{
    [Header("FMOD 이벤트 경로")]
    [Tooltip("이 구역에서 재생할 BGM 이벤트 경로 (예: event:/BGM/Forest/ForestBGM_001)")]
    public string eventPath;

    [Header("겹칠 때 우선순위 (숫자가 클수록 우선)")]
    public int priority = 0;

    // TODO: 특정 조건(퀘스트 진행도, 보스 생존 여부 등)에 따라 false로 바뀔 수 있도록 확장 예정
    public bool IsValid => true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // ?. 대신 명시적 null 체크 (Unity가 파괴한 오브젝트를 확실히 걸러내기 위함)
        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.RegisterZone(this);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.UnregisterZone(this);
        }
    }
}