using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockBlock : MonoBehaviour
{
    public DescendingBlock linkedDescendingBlock;
    private Renderer blockRenderer;

    private void Awake()
    {
        blockRenderer = GetComponent<Renderer>();
        if (blockRenderer == null)
        {
            Debug.LogError($"{gameObject.name} 上没有找到 Renderer 组件！");
        }
    }

    public void TriggerDescend()
    {
        if (linkedDescendingBlock != null)
        {
            linkedDescendingBlock.TriggerDescend();
            
            if (blockRenderer != null)
            {
                blockRenderer.material.color = Color.green;
            }
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} 没有关联下降机关！");
        }
    }

    public void TriggerRise()
    {
        if (linkedDescendingBlock != null)
        {
            linkedDescendingBlock.TriggerRise();
            
            if (blockRenderer != null)
            {
                blockRenderer.material.color = Color.red;
            }
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} 没有关联下降机关！");
        }
    }
}
