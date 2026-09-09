using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SkillSlotRegisterEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform slotRoot;
    [SerializeField] private RectTransform skillIcon;
    [SerializeField] private RectTransform registerFlash;

    [Header("Slot Punch")]
    [SerializeField] private float punchPower = 0.10f;
    [SerializeField] private float punchDuration = 0.28f;

    [Header("Icon")]
    [SerializeField] private float iconStartScale = 0.7f;
    [SerializeField] private float iconDuration = 0.20f;

    [Header("Flash")]
    [SerializeField] private float flashStartScale = 0.85f;
    [SerializeField] private float flashEndScale = 1.35f;
    [SerializeField] private float flashDuration = 0.32f;
    [SerializeField] private float flashStartAlpha = 0.65f;

    private CanvasGroup iconCanvasGroup;
    private CanvasGroup flashCanvasGroup;

    private Vector3 slotBaseScale;
    private Vector3 iconBaseScale;
    private Vector3 flashBaseScale;

    private Sequence effectSequence;

    private void Awake()
    {
        if (slotRoot == null)
            slotRoot = transform as RectTransform;

        if (skillIcon != null)
        {
            iconCanvasGroup = GetOrAddCanvasGroup(skillIcon.gameObject);
            iconBaseScale = skillIcon.localScale;
        }

        if (registerFlash != null)
        {
            flashCanvasGroup = GetOrAddCanvasGroup(registerFlash.gameObject);
            flashBaseScale = registerFlash.localScale;

            flashCanvasGroup.alpha = 0f;
            registerFlash.gameObject.SetActive(false);
        }

        if (slotRoot != null)
            slotBaseScale = slotRoot.localScale;
    }

    public void PlayRegisterEffect()
    {
        effectSequence?.Kill();

        // 기존 상태 복구
        if (slotRoot != null)
            slotRoot.localScale = slotBaseScale;

        // =========================
        // 아이콘 시작 상태
        // =========================

        if (skillIcon != null)
        {
            skillIcon.localScale =
                iconBaseScale * iconStartScale;

            iconCanvasGroup.alpha = 0f;
        }

        // =========================
        // Flash 시작 상태
        // =========================

        if (registerFlash != null)
        {
            registerFlash.gameObject.SetActive(true);

            registerFlash.localScale =
                flashBaseScale * flashStartScale;

            flashCanvasGroup.alpha = flashStartAlpha;
        }

        effectSequence = DOTween.Sequence()
            .SetUpdate(true);

        // 1. 슬롯 자체가 짧게 반응
        if (slotRoot != null)
        {
            effectSequence.Insert(0f,slotRoot.DOPunchScale(Vector3.one * punchPower,punchDuration,3,0.25f));
        }

        // 2. 아이콘이 안쪽에서 나타남
        if (skillIcon != null)
        {
            effectSequence.Insert(
                0.03f,
                skillIcon.DOScale(
                    iconBaseScale,
                    iconDuration
                )
                .SetEase(Ease.OutCubic)
            );

            effectSequence.Insert(
                0.03f,
                iconCanvasGroup.DOFade(
                    1f,
                    iconDuration * 0.7f
                )
            );
        }

        // 3. 금빛 프레임이 바깥으로 퍼짐
        if (registerFlash != null)
        {
            effectSequence.Insert(
                0.02f,
                registerFlash.DOScale(
                    flashBaseScale * flashEndScale,
                    flashDuration
                )
                .SetEase(Ease.OutCubic)
            );

            effectSequence.Insert(
                0.02f,
                flashCanvasGroup.DOFade(
                    0f,
                    flashDuration
                )
                .SetEase(Ease.OutQuad)
            );
        }

        effectSequence.OnComplete(() =>
        {
            if (slotRoot != null)
                slotRoot.localScale = slotBaseScale;

            if (skillIcon != null)
            {
                skillIcon.localScale = iconBaseScale;
                iconCanvasGroup.alpha = 1f;
            }

            if (registerFlash != null)
            {
                registerFlash.localScale = flashBaseScale;
                flashCanvasGroup.alpha = 0f;
                registerFlash.gameObject.SetActive(false);
            }
        });
    }

    private CanvasGroup GetOrAddCanvasGroup(GameObject target)
    {
        CanvasGroup group = target.GetComponent<CanvasGroup>();

        if (group == null)
            group = target.AddComponent<CanvasGroup>();

        return group;
    }

    private void OnDisable()
    {
        effectSequence?.Kill();
        effectSequence = null;

        if (slotRoot != null)
            slotRoot.localScale = slotBaseScale;

        if (skillIcon != null)
        {
            skillIcon.localScale = iconBaseScale;

            if (iconCanvasGroup != null)
                iconCanvasGroup.alpha = 1f;
        }

        if (registerFlash != null)
        {
            registerFlash.localScale = flashBaseScale;
            registerFlash.gameObject.SetActive(false);
        }
    }
}