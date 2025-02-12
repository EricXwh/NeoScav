using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
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

    public Vector3 CurrentVelocity { get; private set; }
    private Vector3 lastPosition;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void Start()
    {
        initialPosition = transform.position;
        targetDescendPosition = initialPosition - new Vector3(0, descendDistance, 0);
        lastPosition = transform.position;
    }

    private void FixedUpdate()
    {
        CurrentVelocity = (transform.position - lastPosition) / Time.fixedDeltaTime;
        lastPosition = transform.position;
    }

    public override void TriggerActivate()
    {
        if (loop)
        {
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
            currentMovementCoroutine = StartCoroutine(MoveToPosition(targetDescendPosition, descendSpeed));
            yield return currentMovementCoroutine;
            currentMovementCoroutine = null;

            currentMovementCoroutine = StartCoroutine(MoveToPosition(initialPosition, descendSpeed));
            yield return currentMovementCoroutine;
            currentMovementCoroutine = null;
        }
    }

    /// 利用 Rigidbody.MovePosition 实现物理移动
    private IEnumerator MoveToPosition(Vector3 targetPos, float speed)
    {
        while (Vector3.Distance(transform.position, targetPos) > 0.001f)
        {
            Vector3 newPosition = Vector3.MoveTowards(transform.position, targetPos, speed * Time.fixedDeltaTime);
            rb.MovePosition(newPosition);
            yield return new WaitForFixedUpdate();
        }
        rb.MovePosition(targetPos);
    }
}
