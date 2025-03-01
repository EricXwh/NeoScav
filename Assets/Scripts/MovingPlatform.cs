using System.Collections;
using UnityEngine;

public class MovingPlatform : MechanismBase
{
    [Header("移动设置")]
    [Tooltip("沿着这些目标点移动，目标点可以是场景中关联的物体")]
    public Transform[] waypoints;

    [Tooltip("移动速度")]
    public float speed = 2f;

    [Tooltip("到达一个目标点后等待的时间（秒）")]
    public float waitTime = 1f;

    private bool isActive = false;
    private Coroutine moveCoroutine;
    private int currentIndex = 0;

    private Vector3 lastPosition;
    public Vector3 CurrentVelocity { get; private set; }

    private void Start()
    {
        lastPosition = transform.position;
    }

    private void Update()
    {
        CurrentVelocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;
    }

    public override void TriggerActivate()
    {
        if (!isActive)
        {
            isActive = true;
            moveCoroutine = StartCoroutine(MoveLoop());
        }
    }

    public override void TriggerDeactivate()
    {
        if (isActive)
        {
            isActive = false;
            if (moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
                moveCoroutine = null;
            }
        }
    }

    private IEnumerator MoveLoop()
    {
        if (waypoints == null || waypoints.Length == 0)
            yield break;

        if (Vector3.Distance(transform.position, waypoints[currentIndex].position) < 0.01f)
        {
            currentIndex = (currentIndex + 1) % waypoints.Length;
        }

        while (isActive)
        {
            Transform target = waypoints[currentIndex];
            while (Vector3.Distance(transform.position, target.position) > 0.01f && isActive)
            {
                transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
                yield return null;
            }
            yield return new WaitForSeconds(waitTime);
            if (Vector3.Distance(transform.position, target.position) < 0.01f)
                currentIndex = (currentIndex + 1) % waypoints.Length;
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        collision.transform.parent = transform;
    }

    private void OnCollisionExit(Collision collision)
    {
        collision.transform.parent = null;
    }
}
