using UnityEngine;

public class CarryBlock : MonoBehaviour
{
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
