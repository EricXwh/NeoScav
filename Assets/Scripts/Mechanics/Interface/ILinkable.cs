public interface ILinkable
{
    /// <summary>
    /// 被触发时，需要调用 TriggerActivate/Deactivate 的目标机制
    /// </summary>
    MechanismBase[] LinkedMechanisms { get; set; }
}
