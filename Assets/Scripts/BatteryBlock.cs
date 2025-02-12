using System.Collections.Generic;
using UnityEngine;

public class BatteryBlock : MonoBehaviour
{
    public List<GameObject> linkedLocks;
    public float triggerDistance = 2f;

    public Material lineMaterial;

    // 保存每个锁对象对应的 LineRenderer
    private Dictionary<GameObject, LineRenderer> lockLineRenderers = new Dictionary<GameObject, LineRenderer>();
    // 记录每个锁对象当前是否已触发（进入范围后触发）
    private Dictionary<GameObject, bool> lockTriggered = new Dictionary<GameObject, bool>();

    private void Start()
    {
        // 对每个关联的锁对象进行初始化
        foreach (GameObject lockObj in linkedLocks)
        {
            if (lockObj == null)
                continue;

            // 初始状态为未触发
            lockTriggered[lockObj] = false;

            // 创建连线对象并添加 LineRenderer 组件
            GameObject lineObj = new GameObject("LineTo_" + lockObj.name);
            lineObj.transform.SetParent(transform);
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();

            // 设置连线材质和宽度（可根据需要调整）
            lr.material = lineMaterial;
            lr.widthMultiplier = 0.1f;
            lr.positionCount = 2;
            lr.enabled = false;

            lockLineRenderers.Add(lockObj, lr);
        }
    }

    private void Update()
    {
        // 遍历所有关联的锁对象
        foreach (GameObject lockObj in linkedLocks)
        {
            if (lockObj == null)
                continue;

            float distance = Vector3.Distance(transform.position, lockObj.transform.position);

            // 获取关联的继承自 MechanismBase 的组件
            MechanismBase mechanism = lockObj.GetComponent<MechanismBase>();

            if (distance <= triggerDistance)
            {
                // 当进入触发范围，并且之前未触发时
                if (!lockTriggered[lockObj])
                {
                    if (mechanism != null)
                    {
                        mechanism.TriggerActivate();
                    }
                    lockTriggered[lockObj] = true;
                }

                // 启用并更新连线位置
                if (lockLineRenderers.TryGetValue(lockObj, out LineRenderer lr))
                {
                    lr.enabled = true;
                    lr.SetPosition(0, transform.position);
                    lr.SetPosition(1, lockObj.transform.position);
                }
            }
            else
            {
                // 当超出触发范围，并且之前处于触发状态时
                if (lockTriggered[lockObj])
                {
                    if (mechanism != null)
                    {
                        mechanism.TriggerDeactivate();
                    }
                    lockTriggered[lockObj] = false;
                }

                // 禁用连线显示
                if (lockLineRenderers.TryGetValue(lockObj, out LineRenderer lr))
                {
                    lr.enabled = false;
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
}
