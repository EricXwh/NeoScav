using UnityEngine;

public class LockPlate : MonoBehaviour
{
    [Header("关联的锁模块")]
    public LockBlock linkedLock;

    // Renderer 用于改变颜色显示
    private Renderer rend;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            // 初始状态为未激活（红色）
            rend.material.color = Color.red;
        }
    }

    // 当电池进入 Plate 的触发区域时调用
    private void OnTriggerEnter(Collider other)
    {
        BatteryBlock battery = other.GetComponent<BatteryBlock>();
        if (battery != null)
        {
            if (linkedLock != null)
            {
                linkedLock.TriggerDescend();
            }
            if (rend != null)
            {
                rend.material.color = Color.green;
            }
        }
    }

    // 当电池离开 Plate 的触发区域时调用
    private void OnTriggerExit(Collider other)
    {
        BatteryBlock battery = other.GetComponent<BatteryBlock>();
        if (battery != null)
        {
            if (linkedLock != null)
            {
                linkedLock.TriggerRise();
            }
            if (rend != null)
            {
                rend.material.color = Color.red;
            }
        }
    }
}
