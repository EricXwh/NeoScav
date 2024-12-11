using System;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    // 定义事件
    public event Action<CircuitBlock> OnCircuitBlockConnected;
    public event Action<CircuitBlock> OnCircuitBlockDisconnected;

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
    public void TriggerCircuitBlockConnected(CircuitBlock block)
    {
        OnCircuitBlockConnected?.Invoke(block);
    }

    // 触发断开连接事件
    public void TriggerCircuitBlockDisconnected(CircuitBlock block)
    {
        OnCircuitBlockDisconnected?.Invoke(block);
    }
}
