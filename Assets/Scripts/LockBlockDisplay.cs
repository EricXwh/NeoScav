using UnityEngine;
using System.Collections.Generic;

public class LockBlockDisplay : MechanismBase
{
    [Tooltip("灯开启时使用的材质")]
    public Material onMaterial;
    [Tooltip("灯关闭时使用的材质")]
    public Material offMaterial;

    [Header("配置参数")]
    [Tooltip("需要激活的压力板总数（上限）")]
    public int requiredPressurePlateCount = 3;

    private int currentCount = 0;

    private Renderer[] indicatorRenderers;

    private void Awake()
    {
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
        List<Renderer> childRenderers = new List<Renderer>();
        foreach (var rend in allRenderers)
        {
            if (rend.gameObject != this.gameObject)
            {
                childRenderers.Add(rend);
            }
        }
        indicatorRenderers = childRenderers.ToArray();

        UpdateDisplay();
    }

    public override void TriggerActivate()
    {
        currentCount++;
        if (currentCount > requiredPressurePlateCount)
            currentCount = requiredPressurePlateCount;
        UpdateDisplay();
    }

    public override void TriggerDeactivate()
    {
        currentCount--;
        if (currentCount < 0)
            currentCount = 0;
        UpdateDisplay();
    }

    /// 根据当前激活数量更新显示灯的材质：如果某个灯的索引小于 currentCount 则使用 onMaterial，否则使用 offMaterial
    private void UpdateDisplay()
    {
        if (indicatorRenderers == null || indicatorRenderers.Length == 0)
            return;

        for (int i = 0; i < indicatorRenderers.Length; i++)
        {
            if (indicatorRenderers[i] != null)
            {
                if (i < currentCount)
                {
                    indicatorRenderers[i].material = onMaterial;
                }
                else
                {
                    indicatorRenderers[i].material = offMaterial;
                }
            }
        }
    }
}
