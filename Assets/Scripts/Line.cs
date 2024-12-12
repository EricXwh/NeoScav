using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Line : MonoBehaviour
{
    [Header("关联的 PuzzleManager")]
    public PuzzleManager puzzleManager; // 关联的 PuzzleManager

    private HashSet<Line> connectedLines = new HashSet<Line>();

    private void Awake()
    {
        if (puzzleManager == null)
        {
            Debug.LogError($"{gameObject.name} 的 Line 需要分配 PuzzleManager。");
        }

        // 确保 Collider 是 Trigger
        Collider col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Line otherLine = other.GetComponent<Line>();
        if (otherLine != null && otherLine.puzzleManager == puzzleManager)
        {
            if (connectedLines.Add(otherLine))
            {
                Debug.Log($"{gameObject.name} 与 {otherLine.gameObject.name} 建立连接.");
                puzzleManager.UpdateConnections();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Line otherLine = other.GetComponent<Line>();
        if (otherLine != null && otherLine.puzzleManager == puzzleManager)
        {
            if (connectedLines.Remove(otherLine))
            {
                Debug.Log($"{gameObject.name} 与 {otherLine.gameObject.name} 断开连接.");
                puzzleManager.UpdateConnections();
            }
        }
    }

    public IEnumerable<Line> GetConnectedLines()
    {
        return connectedLines;
    }
}
