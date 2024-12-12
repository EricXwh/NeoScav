using System;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    // 定义事件
    public event Action<GameObject> OnCircuitBlockConnected;
    public event Action<GameObject> OnCircuitBlockDisconnected;
    public event Action<GameObject> OnPressurePlateMechanism;

    private void Awake()
    {
        // 确保只有一个 EventManager 存在
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 触发连接事件
    public void TriggerCircuitBlockConnected(GameObject block)
    {
        OnCircuitBlockConnected?.Invoke(block);
    }

    // 触发断开连接事件
    public void TriggerCircuitBlockDisconnected(GameObject block)
    {
        OnCircuitBlockDisconnected?.Invoke(block);
    }

    public void TriggerPressurePlateMechanism(GameObject pressurePlate)
    {
        OnPressurePlateMechanism?.Invoke(pressurePlate);
    }
}
