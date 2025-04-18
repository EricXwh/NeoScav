using System.Collections.Generic;
using UnityEngine;

public class BatteryBlock : MonoBehaviour, ILinkable
{
    public MechanismBase[] linkedMechanisms;

    MechanismBase[] ILinkable.LinkedMechanisms
    {
        get => linkedMechanisms;
        set => linkedMechanisms = value;
    }
    public float triggerDistance = 2f;
    public float lineWidth = 0.1f;
    public Material lineMaterial;

    // 保存每个锁对象对应的 LineRenderer
    private Dictionary<MechanismBase, LineRenderer> lineRenderers = new();
    // 记录每个锁对象当前是否已触发（进入范围后触发）
    private Dictionary<MechanismBase, bool> isTriggered = new();
    

    private void Start()
    {
        // 为每个关联机制创建一条线并初始化状态
        foreach (var mech in linkedMechanisms)
        {
            if (mech == null) 
                continue;

            isTriggered[mech] = false;

            GameObject lineObj = new GameObject($"Line_{mech.name}");
            lineObj.transform.SetParent(transform);
            var lr = lineObj.AddComponent<LineRenderer>();

            lr.material        = lineMaterial;
            lr.widthMultiplier = lineWidth;
            lr.positionCount   = 2;
            lr.enabled         = false;

            lineRenderers[mech] = lr;
        }
    }

    private void Update()
    {
        // 每帧检查玩家/方块与每个机制的距离，决定激活或复位
        foreach (var mech in linkedMechanisms)
        {
            if (mech == null) 
                continue;

            float dist = Vector3.Distance(transform.position, mech.transform.position);
            bool wasTriggered = isTriggered[mech];

            if (dist <= triggerDistance)
            {
                if (!wasTriggered)
                {
                    mech.TriggerActivate();
                    isTriggered[mech] = true;
                }
                // 显示并更新连线
                var lr = lineRenderers[mech];
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
                // 隐藏连线
                lineRenderers[mech].enabled = false;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
}
