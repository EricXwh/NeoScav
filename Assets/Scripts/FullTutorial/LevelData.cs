using UnityEngine;

public class LevelData : MonoBehaviour
{
    [Tooltip("是否在该关卡引入了新的游戏机制，需要显示介绍页面？")]
    public bool introducesNewMechanism = false;

    // [Tooltip("新机制的说明文本")]
    // public string mechanismTutorialMessage = "这里是新机制介绍……";
    public Sprite tutorialImage;
}
