using UnityEngine;
using System.Collections.Generic;

public class LockBlock : MechanismBase, ILinkable
{
    [Tooltip("灯开启时使用的材质")]
    public Material onMaterial;
    [Tooltip("灯关闭时使用的材质")]
    public Material offMaterial;

    public int requiredPressurePlateCount = 3;

    public MechanismBase[] linkedMechanisms;

    // 当前接收到的激活计数
    private int currentCount = 0;
    // 记录是否已经达到最终激活状态
    private bool isActivated = false;
    // 自动获取的子物体 Renderer，用于显示指示灯状态
    private Renderer[] indicatorRenderers;
    
    MechanismBase[] ILinkable.LinkedMechanisms
    {
        get => linkedMechanisms;
        set => linkedMechanisms = value;
    }

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
        CheckActivation();
    }

    public override void TriggerDeactivate()
    {
        currentCount--;
        if (currentCount < 0)
            currentCount = 0;

        UpdateDisplay();
        CheckActivation();
    }

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

    /// 根据计数决定是否激活或复位关联机关
    private void CheckActivation()
    {
        // 当当前计数达到（或超过）预设值，且之前未激活时，触发最终机关
        if (!isActivated && currentCount >= requiredPressurePlateCount)
        {
            isActivated = true;
            foreach (var mech in linkedMechanisms)
            {
                mech.TriggerActivate();
            }
        }
        // 当当前计数低于预设值，而之前处于激活状态时，执行复位
        else if (isActivated && currentCount < requiredPressurePlateCount)
        {
            isActivated = false;
            foreach (var mech in linkedMechanisms)
            {
                mech.TriggerDeactivate();
            }
        }
    }
}
