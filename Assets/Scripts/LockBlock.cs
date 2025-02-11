using UnityEngine;

public class LockBlock : MechanismBase
{
    public int requiredPressurePlateCount = 1;

    public MechanismBase[] linkedMechanisms;

    // 当前来自 PressurePlate 的激活计数
    private int activePlateCount = 0;
    // 标记当前是否处于最终激活状态
    private bool isActivated = false;

    public override void TriggerActivate()
    {
        activePlateCount++;
        CheckActivation();
    }

    public override void TriggerDeactivate()
    {
        activePlateCount = Mathf.Max(0, activePlateCount - 1);
        CheckActivation();
    }

    private void CheckActivation()
    {
        // 当计数达到或超过设定值且当前未激活时
        if (!isActivated && activePlateCount >= requiredPressurePlateCount)
        {
            isActivated = true;
            UpdatePlateColor(isActivated);
            // 遍历所有关联机关并调用它们的 TriggerActivate()
            foreach (var mech in linkedMechanisms)
            {
                mech.TriggerActivate();
            }
        }
        // 当计数低于设定值且当前已经激活时，执行复位操作
        else if (isActivated && activePlateCount < requiredPressurePlateCount)
        {
            isActivated = false;
            UpdatePlateColor(isActivated);
            // 遍历所有关联机关并调用它们的 TriggerDeactivate()
            foreach (var mech in linkedMechanisms)
            {
                mech.TriggerDeactivate();
            }
        }
    }

    private void UpdatePlateColor(bool activated)
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = activated ? Color.green : Color.red;
        }
    }
}
