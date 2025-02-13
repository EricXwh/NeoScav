public interface ITriggerable
{
    /// 当触发板被激活时调用
    void TriggerActivate();

    /// 当触发板复位时调用
    void TriggerDeactivate();
}
