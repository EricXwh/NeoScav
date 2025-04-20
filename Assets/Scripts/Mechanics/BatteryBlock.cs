using UnityEngine;
using System.Collections.Generic;

public class BatteryBlock : MonoBehaviour, ILinkable
{
    [Header("—— 链接的机关 ——")]
    [Tooltip("通过 Inspector 关联要被触发的 MechanismBase 们")]
    public MechanismBase[] linkedMechanisms;

    [Header("—— 线条参数 ——")]
    [Tooltip("触发距离")]
    public float triggerDistance = 2f;
    [Tooltip("连线宽度")]
    public float lineWidth = 0.1f;
    [Tooltip("连线材质")]
    public Material lineMaterial;

    // 每个 mech 对应一条 LineRenderer
    private Dictionary<MechanismBase, LineRenderer> lineRenderers = new();
    // 每个 mech 对应当前是否已经触发
    private Dictionary<MechanismBase, bool> isTriggered = new();

    MechanismBase[] ILinkable.LinkedMechanisms
    {
        get => linkedMechanisms;
        set => linkedMechanisms = value;
    }

    private void Awake()
    {
        InitializeAllLinkedMechanisms();
    }

    private void InitializeAllLinkedMechanisms()
    {
        for (int i = 0; i < linkedMechanisms.Length; i++)
        {
            var mech = linkedMechanisms[i];
            if (mech == null || lineRenderers.ContainsKey(mech))
                continue;

            isTriggered[mech] = false;

            GameObject lineObj = new GameObject($"Line_{mech.name}");
            lineObj.transform.SetParent(transform, worldPositionStays: true);
            var lr = lineObj.AddComponent<LineRenderer>();
            lr.material = lineMaterial;
            lr.widthMultiplier = lineWidth;
            lr.positionCount = 2;
            lr.enabled = false;

            lineRenderers[mech] = lr;
        }
    }

    private void Update()
    {
        InitializeAllLinkedMechanisms();

        foreach (var mech in linkedMechanisms)
        {
            if (mech == null)
                continue;

            if (!isTriggered.TryGetValue(mech, out bool wasTriggered))
                continue;

            float dist = Vector3.Distance(transform.position, mech.transform.position);
            var lr = lineRenderers[mech];

            if (dist <= triggerDistance)
            {
                if (!wasTriggered)
                {
                    mech.TriggerActivate();
                    isTriggered[mech] = true;
                }
                lr.enabled = true;
                lr.SetPosition(0, transform.position);
                lr.SetPosition(1, mech.transform.position);
            }
            else
            {
                if (wasTriggered)
                {
                    mech.TriggerDeactivate();
                    isTriggered[mech] = false;
                }
                lr.enabled = false;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
}
