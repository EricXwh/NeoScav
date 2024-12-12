using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [Header("Puzzle Points")]
    public GameObject pointA;
    public GameObject pointB;

    [Header("Required Connections")]
    public int requiredConnectionCount = 2; // 需要的连接数

    [Header("Linked Mechanism")]
    public GameObject linkedDescendingBlock;

    public Color connectedColor = Color.green;    // 连接时的颜色
    public Color disconnectedColor = Color.red;   // 未连接时的颜色


    private List<Line> allLines = new List<Line>();
    private HashSet<Line> linesConnectedToA = new HashSet<Line>();
    private HashSet<Line> linesConnectedToB = new HashSet<Line>();

    private bool isMechanismTriggered = false;

    private void Awake()
    {
        // 验证必要的引用
        if (pointA == null || pointB == null)
        {
            Debug.LogError($"{gameObject.name} 需要分配 PointA 和 PointB。");
        }

        if (linkedDescendingBlock == null)
        {
            Debug.LogError($"{gameObject.name} 需要分配 linkedDescendingBlock。");
        }

        // 获取所有关联的 Line 脚本
        Line[] lines = GetComponentsInChildren<Line>();
        foreach (Line line in lines)
        {
            allLines.Add(line);
        }
    }

    private void Start()
    {
        SetLinesColor(disconnectedColor);
    }

    /// <summary>
    /// 当 Line 的连接状态改变时调用。
    /// </summary>
    public void UpdateConnections()
    {
        // 清除之前的连接记录
        linesConnectedToA.Clear();
        linesConnectedToB.Clear();

        // 遍历所有 Line，检查其是否连接到 PointA 或 PointB
        foreach (Line line in allLines)
        {
            if (IsConnectedToPoint(line, pointA))
            {
                linesConnectedToA.Add(line);
            }

            if (IsConnectedToPoint(line, pointB))
            {
                linesConnectedToB.Add(line);
            }
        }

        // 找到同时连接到两个点的 Line
        HashSet<Line> linesConnectedToBoth = new HashSet<Line>(linesConnectedToA);
        linesConnectedToBoth.IntersectWith(linesConnectedToB);

        // 检查是否满足触发机关的条件
        if (linesConnectedToBoth.Count >= requiredConnectionCount && !isMechanismTriggered)
        {
            isMechanismTriggered = true;
            TriggerMechanism();
            SetLinesColor(connectedColor);
        }
        else if (linesConnectedToBoth.Count < requiredConnectionCount && isMechanismTriggered)
        {
            isMechanismTriggered = false;
            ResetMechanism();
            SetLinesColor(disconnectedColor);
        }
    }

    /// <summary>
    /// 检查一个 Line 是否通过连接与指定的点相通
    /// </summary>
    private bool IsConnectedToPoint(Line line, GameObject point)
    {
        // 使用 BFS 来检查是否存在从 line 到 point 的路径
        HashSet<Line> visited = new HashSet<Line>();
        Queue<Line> queue = new Queue<Line>();

        queue.Enqueue(line);
        visited.Add(line);

        while (queue.Count > 0)
        {
            Line current = queue.Dequeue();

            // 检查当前 Line 是否与点重叠
            if (IsOverlappingWithPoint(current.gameObject, point))
            {
                return true;
            }

            // 遍历连接的 Line
            foreach (Line connectedLine in current.GetConnectedLines())
            {
                if (!visited.Contains(connectedLine))
                {
                    queue.Enqueue(connectedLine);
                    visited.Add(connectedLine);
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 检查一个 Line 是否与指定的点重叠
    /// </summary>
    private bool IsOverlappingWithPoint(GameObject line, GameObject point)
    {
        Collider pointCollider = point.GetComponent<Collider>();
        Collider lineCollider = line.GetComponent<Collider>();

        if (pointCollider != null && lineCollider != null)
        {
            return pointCollider.bounds.Intersects(lineCollider.bounds);
        }

        return false;
    }

    /// <summary>
    /// 触发关联的机关
    /// </summary>
    private void TriggerMechanism()
    {
        if (linkedDescendingBlock != null)
        {
            DescendingBlock descendingBlock = linkedDescendingBlock.GetComponent<DescendingBlock>();
            if (descendingBlock != null)
            {
                descendingBlock.TriggerDescend();
                Debug.Log($"{gameObject.name} 触发机关。");
            }
            else
            {
                Debug.LogError($"{linkedDescendingBlock.name} 没有附加 DescendingBlock 组件。");
            }
        }
    }

    /// <summary>
    /// 重置关联的机关
    /// </summary>
    private void ResetMechanism()
    {
        if (linkedDescendingBlock != null)
        {
            DescendingBlock descendingBlock = linkedDescendingBlock.GetComponent<DescendingBlock>();
            if (descendingBlock != null)
            {
                descendingBlock.TriggerRise();
                Debug.Log($"{gameObject.name} 重置机关。");
            }
            else
            {
                Debug.LogError($"{linkedDescendingBlock.name} 没有附加 DescendingBlock 组件。");
            }
        }
    }

    /// <summary>
    /// 设置所有 Line 的颜色
    /// </summary>
    /// <param name="color">要设置的颜色</param>
    private void SetLinesColor(Color color)
    {
        foreach (Line line in allLines)
        {
            Renderer renderer = line.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;
            }
            else
            {
                Debug.LogWarning($"{line.gameObject.name} 没有 Renderer 组件。");
            }
        }
    }

}
