using System.Collections;
using UnityEngine;

public class DescendingBlock : MonoBehaviour
{
    public GameObject linkedTriggerObject;
    [Header("下降参数")]
    public float descendDistance = 2f;      // 下降的距离
    public float descendDuration = 2f;      // 下降所需的时间

    private Vector3 initialPosition;
    private Vector3 targetPosition;
    private Coroutine descendCoroutine;

    private void Start()
    {
        // 记录初始位置和目标位置
        initialPosition = transform.position;
        targetPosition = initialPosition - new Vector3(0, descendDistance, 0);

        // 订阅 EventManager 的事件
        EventManager.Instance.OnCircuitBlockConnected += HandleCircuitBlockConnected;
        EventManager.Instance.OnCircuitBlockDisconnected += HandleCircuitBlockDisconnected;
        EventManager.Instance.OnPressurePlateMechanism += HandlePressurePlateMechanism;
    }

    private void OnDestroy()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnCircuitBlockConnected -= HandleCircuitBlockConnected;
            EventManager.Instance.OnCircuitBlockDisconnected -= HandleCircuitBlockDisconnected;
            EventManager.Instance.OnPressurePlateMechanism -= HandlePressurePlateMechanism;
        }
    }

    private void HandleCircuitBlockConnected(GameObject block)
    {
        if (block == linkedTriggerObject)
        {
            if (descendCoroutine != null)
            {
                StopCoroutine(descendCoroutine);
            }
            descendCoroutine = StartCoroutine(Descend());
        }
    }

    private void HandleCircuitBlockDisconnected(GameObject block)
    {
        if (block == linkedTriggerObject)
        {
            if (descendCoroutine != null)
            {
                StopCoroutine(descendCoroutine);
            }
            descendCoroutine = StartCoroutine(Rise());
        }
    }

    private void HandlePressurePlateMechanism(GameObject pressurePlate)
    {
        if (pressurePlate == linkedTriggerObject)
        {
            if(descendCoroutine != null)
            {
                StopCoroutine(descendCoroutine);
            }
            descendCoroutine = StartCoroutine(Descend());
        }
        
    }

    private IEnumerator Descend()
    {
        float elapsed = 0f;
        Vector3 start = transform.position;
        Vector3 end = targetPosition;

        while (elapsed < descendDuration)
        {
            transform.position = Vector3.Lerp(start, end, elapsed / descendDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = end;
    }

    private IEnumerator Rise()
    {
        float elapsed = 0f;
        Vector3 start = transform.position;
        Vector3 end = initialPosition;

        while (elapsed < descendDuration)
        {
            transform.position = Vector3.Lerp(start, end, elapsed / descendDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = end;
    }


}
