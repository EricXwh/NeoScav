using UnityEngine;

public class HintTrigger : MonoBehaviour
{
    // 关联要显示/隐藏的 Hint Canvas 的控制脚本
    public HintCanvasController hintCanvasController;

    // 可以通过设置标签来识别玩家（例如确保玩家 GameObject 的 Tag 为 "Player"）
    private void OnTriggerEnter(Collider other)
    {
        // 检查是否为玩家，并且当前教程模式为 Minimal
        if (other.CompareTag("Player"))
        {
            hintCanvasController?.ShowHint();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 玩家离开区域后隐藏提示
        if (other.CompareTag("Player"))
        {
            hintCanvasController?.HideHint();
        }
    }
}
