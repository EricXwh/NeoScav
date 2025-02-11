using System;
using UnityEngine;

/// Singleton EventManager，用于管理游戏中的全局事件。
public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    /// 当一个谜题完成时触发的事件。
    public event Action<GameObject> PuzzleCompleted;


    /// 当一个谜题被重置时触发的事件。
    public event Action<GameObject> PuzzleReset;

    /// 当压力板被激活时触发的事件。
    /// 传递被激活的压力板对象。
    public event Action<GameObject> PressurePlateMechanism;

    public event Action<GameObject> PressurePlateReset;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Debug.LogWarning("检测到多个 EventManager 实例。销毁重复的实例。");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 触发 PuzzleCompleted 事件。
    public void TriggerPuzzleCompleted(GameObject linkedMechanism)
    {
        if (linkedMechanism == null)
        {
            Debug.LogError("TriggerPuzzleCompleted 被调用，但 linkedMechanism 为 null。");
            return;
        }

        PuzzleCompleted?.Invoke(linkedMechanism);
        Debug.Log($"PuzzleCompleted 事件已触发，关联机关: {linkedMechanism.name}");
    }

    /// 触发 PuzzleReset 事件。
    public void TriggerPuzzleReset(GameObject linkedMechanism)
    {
        if (linkedMechanism == null)
        {
            Debug.LogError("TriggerPuzzleReset 被调用，但 linkedMechanism 为 null。");
            return;
        }

        PuzzleReset?.Invoke(linkedMechanism);
        Debug.Log($"PuzzleReset 事件已触发，关联机关: {linkedMechanism.name}");
    }

    /// 触发 PressurePlateMechanism 事件。
    /// 调用此方法时，应传递被激活的压力板对象。
    public void TriggerPressurePlateMechanism(GameObject pressurePlate)
    {
        if (pressurePlate == null)
        {
            Debug.LogError("TriggerPressurePlateMechanism 被调用，但 pressurePlate 为 null。");
            return;
        }

        PressurePlateMechanism?.Invoke(pressurePlate);
        Debug.Log($"PressurePlateMechanism 事件已触发，压力板: {pressurePlate.name}");
    }

    public void TriggerPressurePlateReset(GameObject pressurePlate)
    {
        if (pressurePlate == null)
        {
            Debug.LogError("TriggerPressurePlateMechanism 被调用，但 pressurePlate 为 null。");
            return;
        }

        PressurePlateReset?.Invoke(pressurePlate);
        Debug.Log($"PressurePlateMechanism 事件已触发，压力板: {pressurePlate.name}");
    }
}
