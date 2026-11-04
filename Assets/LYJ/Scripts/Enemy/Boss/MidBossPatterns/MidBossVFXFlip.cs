
using UnityEngine;
using System.Collections.Generic;

public class MidBossVFXFlip : MonoBehaviour
{
    [Header("Boss Visual")]
    [SerializeField] private SpriteRenderer bossSprite;

    [Header("VFX Objects")]
    [SerializeField] private Transform[] vfxObjects;

    [Header("Auto Register Clones")]
    [SerializeField] private bool autoRegisterClones = true;

    [SerializeField] private Transform vfxRoot;

    [Header("Debug")]
    [SerializeField] private bool showDebugLog = false;

    private class VFXData
    {
        public Transform target;
        public Vector3 originalPosition;
        public Quaternion originalRotation;
    }

    private readonly List<VFXData> vfxData = new List<VFXData>();
    private readonly HashSet<Transform> registered =
        new HashSet<Transform>();

    private bool initialized;
    private bool lastFacingLeft;

    private void Awake()
    {
        if (bossSprite == null)
        {
            Debug.LogError(
                "[MidBossVFXFlip] Boss SpriteRenderer가 없습니다.",
                this
            );
            return;
        }

        // Inspector에서 연결한 원본 VFX 등록
        if (vfxObjects != null)
        {
            foreach (Transform vfx in vfxObjects)
            {
                RegisterVFX(vfx);
            }
        }

        initialized = true;
    }

    private void Start()
    {
        if (!initialized)
            return;

        // BossVFXController.Awake()에서 생성된 Clone 검색
        RegisterClones();

        lastFacingLeft = bossSprite.flipX;
        ApplyDirection(lastFacingLeft);
    }

    private void LateUpdate()
    {
        if (!initialized)
            return;

        // 이후 생성된 Clone도 자동 등록
        if (autoRegisterClones)
        {
            RegisterClones();
        }

        bool facingLeft = bossSprite.flipX;

        if (facingLeft != lastFacingLeft)
        {
            lastFacingLeft = facingLeft;
            ApplyDirection(facingLeft);
        }
    }

    private void RegisterVFX(Transform target)
    {
        if (target == null)
            return;

        if (registered.Contains(target))
            return;

        registered.Add(target);

        vfxData.Add(new VFXData
        {
            target = target,
            originalPosition = target.localPosition,
            originalRotation = target.localRotation
        });

        if (showDebugLog)
        {
            Debug.Log(
                "[MidBossVFXFlip] 등록: " + target.name,
                target
            );
        }
    }

    private void RegisterClones()
    {
        if (!autoRegisterClones || vfxRoot == null)
            return;

        // 비활성화된 Clone도 포함
        Transform[] children =
            vfxRoot.GetComponentsInChildren<Transform>(true);

        bool foundNewClone = false;

        foreach (Transform child in children)
        {
            if (child == vfxRoot)
                continue;

            if (!child.name.Contains("(Clone)"))
                continue;

            if (registered.Contains(child))
                continue;

            RegisterVFX(child);
            foundNewClone = true;
        }

        // 새로 등록된 Clone도 현재 방향으로 반전
        if (foundNewClone)
        {
            ApplyDirection(bossSprite.flipX);
        }
    }

    private void ApplyDirection(bool facingLeft)
    {
        foreach (VFXData data in vfxData)
        {
            if (data.target == null)
                continue;

            Vector3 pos = data.originalPosition;

            pos.x = facingLeft
                ? -Mathf.Abs(data.originalPosition.x)
                : Mathf.Abs(data.originalPosition.x);

            data.target.localPosition = pos;

            Quaternion flipRotation = facingLeft
                ? Quaternion.Euler(0f, 180f, 0f)
                : Quaternion.identity;

            data.target.localRotation =
                flipRotation * data.originalRotation;
        }

        if (showDebugLog)
        {
            Debug.Log(
                "[MidBossVFXFlip] 현재 방향: " +
                (facingLeft ? "왼쪽" : "오른쪽"),
                this
            );
        }
    }
}
