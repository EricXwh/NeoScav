using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CircuitBlock : MonoBehaviour
{
    [Header("Line Detection Layers")]
    public LayerMask lineLayer;

    [Header("关联的 PuzzleManager")]
    public PuzzleManager associatedPuzzleManager; // 关联的 PuzzleManager

    private Line[] lines;

    private void Awake()
    {
        if (associatedPuzzleManager == null)
        {
            Debug.LogError($"{gameObject.name} 需要分配 associatedPuzzleManager。");
        }

        // 获取所有子对象的 Line 脚本
        lines = GetComponentsInChildren<Line>();
        foreach (Line line in lines)
        {
            if (line.puzzleManager != associatedPuzzleManager)
            {
                line.puzzleManager = associatedPuzzleManager;
            }
        }
    }

    private void Start()
    {
        // 初始更新连接状态
        associatedPuzzleManager.UpdateConnections();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            Gizmos.DrawWireCube(collider.bounds.center, collider.bounds.size);
        }
    }
}
