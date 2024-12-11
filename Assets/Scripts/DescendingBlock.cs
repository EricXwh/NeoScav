using System.Collections;
using UnityEngine;

public class DescendingBlock : MonoBehaviour
{
    [Header("关联的 CircuitBlock")]
    public CircuitBlock interactiveBlock; // 在 Inspector 中关联

    [Header("下降参数")]
    public float descendDistance = 2f;      // 下降的距离
    public float descendDuration = 2f;      // 下降所需的时间

    private Vector3 initialPosition;
    private Vector3 targetPosition;
    private Coroutine descendCoroutine;

    private void Start()
    {
        if (interactiveBlock == null)
        {
            Debug.LogError("DescendingBlock 脚本的 CircuitBlock 未被分配");
            return;
        }

        // 记录初始位置和目标位置
        initialPosition = transform.position;
        targetPosition = initialPosition - new Vector3(0, descendDistance, 0);

        // 订阅 EventManager 的事件
        EventManager.Instance.OnCircuitBlockConnected += HandleCircuitBlockConnected;
        EventManager.Instance.OnCircuitBlockDisconnected += HandleCircuitBlockDisconnected;
    }

    private void OnDestroy()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnCircuitBlockConnected -= HandleCircuitBlockConnected;
            EventManager.Instance.OnCircuitBlockDisconnected -= HandleCircuitBlockDisconnected;
        }
    }

    private void HandleCircuitBlockConnected(CircuitBlock block)
    {
        if (block == interactiveBlock)
        {
            //ChangeParentLayer(LayerMask.NameToLayer("Default"));
            if (descendCoroutine != null)
            {
                StopCoroutine(descendCoroutine);
            }
            descendCoroutine = StartCoroutine(Descend());
        }
    }

    private void HandleCircuitBlockDisconnected(CircuitBlock block)
    {
        if (block == interactiveBlock)
        {
            if (descendCoroutine != null)
            {
                StopCoroutine(descendCoroutine);
            }
            descendCoroutine = StartCoroutine(Rise());
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

    private void ChangeParentLayer(int newLayer)
    {
        if (interactiveBlock.transform.parent != null)
        {
            interactiveBlock.transform.parent.gameObject.layer = newLayer;
            Debug.Log($"{interactiveBlock.transform.parent.gameObject.name} 的层已更改为 {LayerMask.LayerToName(newLayer)}");
        }
        else
        {
            Debug.LogWarning("CircuitBlock 的父对象不存在，无法更改层。");
        }
    }

}
