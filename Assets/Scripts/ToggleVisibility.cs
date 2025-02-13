using UnityEngine;

public class ToggleVisibilityMechanism : MechanismBase
{
    [Header("需要切换可见性的 Renderer 组件")]
    [Tooltip("在触发时将隐藏这些 Renderer，复位时重新显示。如果未手动指定，将自动获取当前物体下所有子物体的 Renderer")]
    public Renderer[] targetRenderers;

    private void Awake()
    {
        // 如果没有在 Inspector 中手动指定 Renderer，就自动获取当前物体（包括子物体）中的所有 Renderer
        if (targetRenderers == null || targetRenderers.Length == 0)
        {
            targetRenderers = GetComponentsInChildren<Renderer>();
        }
        SetVisibility(false);
    }

    public override void TriggerActivate()
    {
        SetVisibility(true);
    }

    public override void TriggerDeactivate()
    {
        SetVisibility(false);
    }

    /// 设置所有目标 Renderer 的可见性
    private void SetVisibility(bool visible)
    {
        if (targetRenderers != null)
        {
            foreach (Renderer rend in targetRenderers)
            {
                if (rend != null)
                {
                    rend.enabled = visible;
                }
            }
        }
    }
}
