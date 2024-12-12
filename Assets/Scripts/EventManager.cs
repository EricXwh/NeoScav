using System;
using UnityEngine;

/// <summary>
/// Singleton EventManager，用于管理游戏中的全局事件。
/// </summary>
public class EventManager : MonoBehaviour
{
    /// <summary>
    /// 单例实例。
    /// </summary>
    public static EventManager Instance;

    /// <summary>
    /// 当一个谜题完成时触发的事件。
    /// 传递关联的机关对象（例如 DescendingBlock）。
    /// </summary>
    public event Action<GameObject> PuzzleCompleted;

    /// <summary>
    /// 当一个谜题被重置时触发的事件。
    /// 传递关联的机关对象（例如 DescendingBlock）。
    /// </summary>
    public event Action<GameObject> PuzzleReset;

    /// <summary>
    /// 当压力板被激活时触发的事件。
    /// 传递被激活的压力板对象。
    /// </summary>
    public event Action<GameObject> PressurePlateMechanism;

    public event Action<GameObject> PressurePlateReset;

    /// <summary>
    /// Awake 方法用于初始化单例模式。
    /// 确保场景中只有一个 EventManager 实例存在。
    /// </summary>
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
    /// 调用此方法时，应传递关联的机关对象。
    /// </summary>
    /// <param name="linkedMechanism">关联的机关对象（例如 DescendingBlock）。</param>
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

    /// <summary>
    /// 触发 PuzzleReset 事件。
    /// 调用此方法时，应传递关联的机关对象。
    /// </summary>
    /// <param name="linkedMechanism">关联的机关对象（例如 DescendingBlock）。</param>
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

    /// <summary>
    /// 触发 PressurePlateMechanism 事件。
    /// 调用此方法时，应传递被激活的压力板对象。
    /// </summary>
    /// <param name="pressurePlate">被激活的压力板对象。</param>
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
