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

        Collider col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            col.isTrigger = true;
        }
    }

    private void Start()
    {
        ConnectSiblingLines();
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

    private void ConnectSiblingLines()
    {
        if (transform.parent == null)
        {
            Debug.LogWarning($"{gameObject.name} 没有父对象，无法连接同父对象下的 Line。");
            return;
        }

        foreach (Transform sibling in transform.parent)
        {
            if (sibling == transform) continue; 

            Line siblingLine = sibling.GetComponent<Line>();
            if (siblingLine != null && siblingLine.puzzleManager == this.puzzleManager)
            {
                if (connectedLines.Add(siblingLine))
                {
                    Debug.Log($"{gameObject.name} 与同父对象的 {siblingLine.gameObject.name} 建立连接.");
                }

                if (siblingLine.connectedLines.Add(this))
                {
                    Debug.Log($"{siblingLine.gameObject.name} 与同父对象的 {gameObject.name} 建立连接.");
                }
            }
        }
        if (connectedLines.Count > 0)
        {
            puzzleManager.UpdateConnections();
        }
    }
}
