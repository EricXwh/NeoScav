using System;
using System.Collections.Generic;
using UnityEngine;

public class CircuitBlock : MonoBehaviour
{
    [Header("Line Detection Layers")]
    public LayerMask lineLayer; 

    public GameObject linkedDescendingBlock;

    // 使用 HashSet 跟踪当前连接的 Circuit 对象
    private HashSet<GameObject> connectedCircuits = new HashSet<GameObject>();

    // 存储前一帧的连接状态
    private bool wasConnected = false;
    public bool IsNewMechanic = false;

    private void Start()
    {
        UpdateLineColor();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsValidCircuit(other.gameObject))
        {
            if (connectedCircuits.Add(other.gameObject))
            {
                Debug.Log($"{gameObject.name} 已连接到 {other.gameObject.name}");
                UpdateConnectionStatus();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsValidCircuit(other.gameObject))
        {
            if (connectedCircuits.Remove(other.gameObject))
            {
                Debug.Log($"{gameObject.name} 已与 {other.gameObject.name} 断开连接");
                UpdateConnectionStatus();
            }
        }
    }

    /// <summary>
    /// 判断一个 GameObject 是否是有效的 Circuit 对象
    /// </summary>
    private bool IsValidCircuit(GameObject obj)
    {
        return ((1 << obj.layer) & lineLayer) != 0 && obj.CompareTag("Circuit") && obj != this.gameObject;
    }

    /// <summary>
    /// 更新连接状态，并根据连接数触发相应的事件
    /// </summary>
    private void UpdateConnectionStatus()
    {
        bool isNowConnected = connectedCircuits.Count == 2;

        // 只有当连接状态发生变化时才触发事件
        if (wasConnected != isNowConnected)
        {
            if (isNowConnected)
            {
                EventManager.Instance?.TriggerCircuitBlockConnected(linkedDescendingBlock);
                Debug.Log($"{gameObject.name} 已达到连接条件（2个连接）");
            }
            else
            {
                EventManager.Instance?.TriggerCircuitBlockDisconnected(linkedDescendingBlock);
                Debug.Log($"{gameObject.name} 不再满足连接条件（少于或多于2个连接）");
            }
        }

        // 更新前一帧的连接状态
        wasConnected = isNowConnected;

        UpdateLineColor();
    }

    /// <summary>
    /// 根据连接状态更新方块颜色
    /// </summary>
    void UpdateLineColor()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = (connectedCircuits.Count == 2) ? Color.green : Color.red;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            Gizmos.DrawWireCube(collider.bounds.center, collider.bounds.size);
        }
    }
}
