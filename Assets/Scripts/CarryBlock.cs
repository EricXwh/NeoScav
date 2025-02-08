using UnityEngine;

public class CarryBlock : MonoBehaviour
{
    [Header("Hint Settings (仅在 Minimal 模式下使用)")]
    [Tooltip("如果该方块需要提示，则将对应的 Hint Canvas（建议为 World Space Canvas）拖入此处")]
    public GameObject hintCanvas;

    /// 显示提示 Canvas
    public void ShowHint()
    {
        if (hintCanvas != null && !hintCanvas.activeSelf)
        {
            hintCanvas.SetActive(true);
        }
    }

    /// 隐藏提示 Canvas
    public void HideHint()
    {
        if (hintCanvas != null && hintCanvas.activeSelf)
        {
            hintCanvas.SetActive(false);
        }
    }
}
