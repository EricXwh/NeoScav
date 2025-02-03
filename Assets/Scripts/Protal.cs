using UnityEngine;

public class Portal : MonoBehaviour
{
    public LevelManager levelManager;

    // 当玩家进入传送门触发区域时调用
    private void OnTriggerEnter(Collider other)
    {
        // 检查是否是玩家进入传送门（确保玩家 Tag 为 "Player"）
        if (other.CompareTag("Player"))
        {
            if (levelManager != null)
            {
                levelManager.NextLevel();
            }
            else
            {
                Debug.LogWarning("LevelManager 未手动赋值，请在 Inspector 中赋值！");
            }
        }
    }
}
