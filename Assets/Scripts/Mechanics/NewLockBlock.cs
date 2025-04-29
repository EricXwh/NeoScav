using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class NewLockBlock : MechanismBase, ILinkable
{
    [Header("—— 结构绑定 ——")]
    [SerializeField] private Transform bodyTransform;
    [SerializeField] private Transform indicatorContainer;
    [SerializeField] private GameObject indicatorPrefab;

    [Header("—— 布局参数 ——")]
    [SerializeField] private float indicatorScale = 0.35f;
    [SerializeField] private float unitLength = 1f;

    [Header("—— 逻辑参数 ——")]
    public int requiredPressurePlateCount = 1;
    public Material onMaterial;
    public Material offMaterial;
    public MechanismBase[] linkedMechanisms;

    private int currentCount = 0;
    private bool isActivated = false;
    private List<Renderer> indicatorRenderers = new List<Renderer>();
    private int prevRequiredCount = -1;

    MechanismBase[] ILinkable.LinkedMechanisms
    {
        get => linkedMechanisms;
        set => linkedMechanisms = value;
    }

    private void Awake()
    {
        prevRequiredCount = -1;
        RebuildIndicators();
        UpdateDisplay();
    }

    private void Update()
    {
        if (Application.isPlaying && requiredPressurePlateCount != prevRequiredCount)
        {
            RebuildIndicators();
            UpdateDisplay();
        }
    }

    private void RebuildIndicators()
    {
        prevRequiredCount = requiredPressurePlateCount;

        for (int i = indicatorContainer.childCount - 1; i >= 0; i--)
        {
            var child = indicatorContainer.GetChild(i).gameObject;
            if (Application.isPlaying)
                Destroy(child);
            else
                DestroyImmediate(child);
        }
        indicatorRenderers.Clear();

        for (int i = 0; i < requiredPressurePlateCount; i++)
        {
            var go = Instantiate(indicatorPrefab, indicatorContainer);
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one * indicatorScale;
            indicatorRenderers.Add(go.GetComponent<Renderer>());
        }

        UpdateBodyScale();
        UpdateIndicatorPositions();
    }

    private void UpdateBodyScale()
    {
        if (bodyTransform == null) return;
        var s = bodyTransform.localScale;
        s.z = requiredPressurePlateCount * unitLength;
        bodyTransform.localScale = s;
    }

    private void UpdateIndicatorPositions()
    {
        if (indicatorRenderers.Count == 0) return;

        float halfOffset = (requiredPressurePlateCount - 1) / 2f;
        float topY = bodyTransform.localScale.y * 0.5f;

        for (int i = 0; i < indicatorRenderers.Count; i++)
        {
            var t = indicatorRenderers[i].transform;
            float z = (i - halfOffset) * unitLength;
            t.localPosition = new Vector3(0f, topY, z);
        }
    }

    private void UpdateDisplay()
    {
        for (int i = 0; i < indicatorRenderers.Count; i++)
        {
            indicatorRenderers[i].material =
                (i < currentCount ? onMaterial : offMaterial);
        }
    }

    public override void TriggerActivate()
    {
        currentCount = Mathf.Min(currentCount + 1, requiredPressurePlateCount);
        UpdateDisplay();
        CheckActivation();
    }

    public override void TriggerDeactivate()
    {
        currentCount = Mathf.Max(currentCount - 1, 0);
        UpdateDisplay();
        CheckActivation();
    }

    private void CheckActivation()
    {
        if (!isActivated && currentCount >= requiredPressurePlateCount)
        {
            isActivated = true;
            foreach (var mech in linkedMechanisms)
                mech.TriggerActivate();
        }
        else if (isActivated && currentCount < requiredPressurePlateCount)
        {
            isActivated = false;
            foreach (var mech in linkedMechanisms)
                mech.TriggerDeactivate();
        }
    }

    [ContextMenu("Rebuild Indicators")]
    private void RebuildInEditor()
    {
        RebuildIndicators();
        UpdateDisplay();
    }
}