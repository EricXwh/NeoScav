using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DescendingBlock : MonoBehaviour
{
    [Header("关联的 Trigger 对象")]
    public GameObject linkedTriggerObject;

    [Header("下降参数")]
    public float descendDistance = 2f;      // 下降的距离
    public float descendDuration = 2f;      // 下降所需的时间

    [Header("上升参数")]
    public float riseDistance = 2f;         // 上升的距离（与下降距离相同，可根据需要调整）
    public float riseDuration = 2f;         // 上升所需的时间

    private Vector3 initialPosition;
    private Vector3 targetDescendPosition;
    private Vector3 targetRisePosition;
    private Coroutine movementCoroutine;

    private void Start()
    {
        // 记录初始位置和目标位置
        initialPosition = transform.position;
        targetDescendPosition = initialPosition - new Vector3(0, descendDistance, 0);
        targetRisePosition = initialPosition + new Vector3(0, riseDistance, 0);
        // 订阅 PuzzleCompleted 和 PuzzleReset 事件
        if (EventManager.Instance != null)
        {
            EventManager.Instance.PuzzleCompleted += HandlePuzzleCompleted;
            EventManager.Instance.PuzzleReset += HandlePuzzleReset;
            EventManager.Instance.PressurePlateMechanism += HandlePressurePlateMechanism;
            EventManager.Instance.PressurePlateReset += HandlePressurePlateReset;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: 未找到 EventManager 实例。");
        }
    }



    private void OnDisable()
    {
        // 取消订阅所有事件
        if (EventManager.Instance != null)
        {
            EventManager.Instance.PuzzleCompleted -= HandlePuzzleCompleted;
            EventManager.Instance.PuzzleReset -= HandlePuzzleReset;
            EventManager.Instance.PressurePlateMechanism -= HandlePressurePlateMechanism;
            EventManager.Instance.PressurePlateReset -= HandlePressurePlateReset;
        }
    }

    /// <summary>
    /// 处理 PuzzleCompleted 事件。
    /// 当收到与自身关联的事件时，触发下降动作。
    /// </summary>
    /// <param name="linkedMechanism">关联的机关对象。</param>
    private void HandlePuzzleCompleted(GameObject linkedMechanism)
    {
        if (linkedMechanism == gameObject)
        {
            TriggerDescend();
        }
    }

    /// <summary>
    /// 处理 PuzzleReset 事件。
    /// 当收到与自身关联的事件时，触发上升动作。
    /// </summary>
    /// <param name="linkedMechanism">关联的机关对象。</param>
    private void HandlePuzzleReset(GameObject linkedMechanism)
    {
        if (linkedMechanism == gameObject)
        {
            TriggerRise();
        }
    }

    /// <summary>
    /// 处理 PressurePlateMechanism 事件。
    /// 当压力板被激活时，触发下降动作。
    /// </summary>
    /// <param name="pressurePlate">被激活的压力板对象。</param>
    private void HandlePressurePlateMechanism(GameObject pressurePlate)
    {
        if (pressurePlate == linkedTriggerObject)
        {
            TriggerDescend();
        }
    }

    private void HandlePressurePlateReset(GameObject pressurePlate)
    {
        if (pressurePlate == linkedTriggerObject)
        {
            TriggerRise();
        }
    }

    /// <summary>
    /// 触发下降动作
    /// </summary>
    public void TriggerDescend()
    {
        StartMovementCoroutine(targetDescendPosition, descendDuration);
    }

    /// <summary>
    /// 触发上升动作
    /// </summary>
    public void TriggerRise()
    {
        StartMovementCoroutine(initialPosition, riseDuration);
    }

    /// <summary>
    /// 开始移动协程到指定位置
    /// </summary>
    /// <param name="targetPos">目标位置</param>
    /// <param name="duration">移动持续时间</param>
    private void StartMovementCoroutine(Vector3 targetPos, float duration)
    {
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }

        movementCoroutine = StartCoroutine(MoveToPosition(targetPos, duration));
    }

    /// <summary>
    /// 协程，逐渐移动到目标位置
    /// </summary>
    private IEnumerator MoveToPosition(Vector3 targetPos, float duration)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        movementCoroutine = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(initialPosition, 0.1f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(targetDescendPosition, 0.1f);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(initialPosition, targetDescendPosition);
    }
}
