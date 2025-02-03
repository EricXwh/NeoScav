using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockBlock : MonoBehaviour
{
    public DescendingBlock linkedDescendingBlock;

    public void TriggerDescend()
    {
        if (linkedDescendingBlock != null)
        {
            linkedDescendingBlock.TriggerDescend();
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
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} 没有关联下降机关！");
        }
    }

}
