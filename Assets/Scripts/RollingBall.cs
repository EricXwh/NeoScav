using System.Collections;
using UnityEngine;

public class RollingBall : MonoBehaviour
{
    private Vector3 initialPosition;
    public float resetInterval = 6f;
    private Rigidbody rb;
    private bool isTriggered = false;
    private Coroutine resetRoutine;

    private void Awake()
    {
        // 记录初始位置，并获取刚体组件
        initialPosition = transform.position;
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        // 每次激活时启动重置协程（避免重复启动前先停止旧的协程）
        if (resetRoutine != null)
            StopCoroutine(resetRoutine);
        resetRoutine = StartCoroutine(ResetPositionRoutine());
    }

    private void OnDisable()
    {
        // 物体停用时停止协程
        if (resetRoutine != null)
        {
            StopCoroutine(resetRoutine);
            resetRoutine = null;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("PressurePlate"))
        {
            isTriggered = true;
        }
    }

    private IEnumerator ResetPositionRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(resetInterval);
            Debug.Log(isTriggered);
            if (!isTriggered)
            {
                ResetPosition();
            }
        }
    }

    private void ResetPosition()
    {
        // 重置物体状态
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = initialPosition;
    }
}
