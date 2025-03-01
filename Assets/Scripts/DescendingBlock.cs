using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class DescendingBlock : MechanismBase
{
    [Header("下降参数")]
    public float descendDistance = 2f;
    public float descendSpeed = 1f;  

    [Header("延迟参数")]
    [Tooltip("延迟多少秒后开始执行Active或Deactive操作")]
    public float actionDelay = 0f;

    private Vector3 initialPosition;
    private int currentStep = 0; // 当前下降的格数

    private Coroutine movementCoroutine;

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
        lastPosition = transform.position;
    }

    private void FixedUpdate()
    {
        CurrentVelocity = (transform.position - lastPosition) / Time.fixedDeltaTime;
        lastPosition = transform.position;
    }

    public override void TriggerActivate()
    {
        StartCoroutine(DelayedActivate());
    }

    private IEnumerator DelayedActivate()
    {
        yield return new WaitForSeconds(actionDelay);
        TriggerDescend();
    }

    public override void TriggerDeactivate()
    {
        StartCoroutine(DelayedDeactivate());
    }

    private IEnumerator DelayedDeactivate()
    {
        yield return new WaitForSeconds(actionDelay);
        TriggerRise();
    }

    public void TriggerDescend()
    {
        currentStep++;  // 累计下降一格
        Vector3 targetPosition = initialPosition - transform.up * descendDistance * currentStep;
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }
        movementCoroutine = StartCoroutine(MoveToPosition(targetPosition, descendSpeed));
    }

    public void TriggerRise()
    {
        if (currentStep > 0)
        {
            currentStep--;  // 上升一格
        }
        Vector3 targetPosition = initialPosition - transform.up * descendDistance * currentStep;
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }
        movementCoroutine = StartCoroutine(MoveToPosition(targetPosition, descendSpeed));
    }

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
