using UnityEngine;

public class Portal : MonoBehaviour
{
    public LevelManager levelManager;

    // 当玩家进入传送门触发区域时调用
    private void OnTriggerEnter(Collider other)
    {
        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc != null && pc.IsCarrying())
        {
            pc.StopCarryingAndDestroy();
            return;
        }
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
