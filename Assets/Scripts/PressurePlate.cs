using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PressurePlate : MonoBehaviour
{
    [Header("触发条件设置")]
    public string[] triggeringTags;

    [Header("关联的机关对象")]
    [Tooltip("拖入需要响应触发的机关组件")]
    public MechanismBase[] linkedMechanisms;

    private bool isActivated = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!isActivated && CheckTriggerCondition(other))
        {
            isActivated = true;
            foreach (var mechanism in linkedMechanisms)
            {
                mechanism.TriggerActivate();
            }
            UpdatePlateColor(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isActivated && CheckTriggerCondition(other))
        {
            isActivated = false;
            foreach (var mechanism in linkedMechanisms)
            {
                mechanism.TriggerDeactivate();
            }
            UpdatePlateColor(false);
        }
    }

    private bool CheckTriggerCondition(Collider other)
    {
        if (triggeringTags != null)
        {
            foreach (var tag in triggeringTags)
            {
                if (other.CompareTag(tag))
                    return true;
            }
        }
        return false;
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
