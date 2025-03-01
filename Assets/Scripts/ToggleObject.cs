using UnityEngine;

public class ToggleObject : MechanismBase
{
    private bool isActive = true;

    public override void TriggerActivate()
    {
        SetObjectActive();
    }

    public override void TriggerDeactivate()
    {
        SetObjectActive();
    }

    private void SetObjectActive()
    {
        gameObject.SetActive(!isActive);
        isActive = !isActive;
    }
}
