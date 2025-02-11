using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DescendingBlock : MechanismBase
{
    [Header("下降参数")]
    public float descendDistance = 2f;
    public float descendSpeed = 1f;  // 单位：米/秒

    [Header("循环选项")]
    [Tooltip("勾选后触发会持续循环降落和上升，取消触发时会立即停止运动")]
    public bool loop = false;

    private Vector3 initialPosition;
    private Vector3 targetDescendPosition;

    private Coroutine movementCoroutine;
    private Coroutine loopCoroutine;
    private Coroutine currentMovementCoroutine;

    private void Start()
    {
        initialPosition = transform.position;
        targetDescendPosition = initialPosition - new Vector3(0, descendDistance, 0);
    }

    public override void TriggerActivate()
    {
        if (loop)
        {
            // 循环模式：启动循环运动协程
            if (loopCoroutine != null)
            {
                StopCoroutine(loopCoroutine);
            }
            loopCoroutine = StartCoroutine(LoopMovementCoroutine());
        }
        else
        {
            TriggerDescend();
        }
    }

    public override void TriggerDeactivate()
    {
        if (loop)
        {
            // 循环模式下，立即停止所有运动
            if (loopCoroutine != null)
            {
                StopCoroutine(loopCoroutine);
                loopCoroutine = null;
            }
            if (currentMovementCoroutine != null)
            {
                StopCoroutine(currentMovementCoroutine);
                currentMovementCoroutine = null;
            }
        }
        else
        {
            TriggerRise();
        }
    }

    public void TriggerDescend()
    {
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }
        movementCoroutine = StartCoroutine(MoveToPosition(targetDescendPosition, descendSpeed));
    }

    public void TriggerRise()
    {
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }
        movementCoroutine = StartCoroutine(MoveToPosition(initialPosition, descendSpeed));
    }

    private IEnumerator LoopMovementCoroutine()
    {
        while (true)
        {
            // 下降阶段
            currentMovementCoroutine = StartCoroutine(MoveToPosition(targetDescendPosition, descendSpeed));
            yield return currentMovementCoroutine;
            currentMovementCoroutine = null;

            // 上升阶段
            currentMovementCoroutine = StartCoroutine(MoveToPosition(initialPosition, descendSpeed));
            yield return currentMovementCoroutine;
            currentMovementCoroutine = null;
        }
    }

    /// 按照设定速度将物体移动到目标位置
    private IEnumerator MoveToPosition(Vector3 targetPos, float speed)
    {
        while (Vector3.Distance(transform.position, targetPos) > 0.001f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;
    }
}
