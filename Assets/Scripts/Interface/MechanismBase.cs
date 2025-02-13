using UnityEngine;

public abstract class MechanismBase : MonoBehaviour, ITriggerable
{
    public abstract void TriggerActivate();
    public abstract void TriggerDeactivate();
}
