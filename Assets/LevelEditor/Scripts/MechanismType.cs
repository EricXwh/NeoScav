using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(menuName = "LevelEditor/MechanismType", fileName = "NewMechanismType")]
public class MechanismType : ScriptableObject {
    [Header("编辑器展示")]
    public string displayName; 
    public Sprite icon; 

    [Header("运行时实例化 (Addressable)")]
    public AssetReferenceGameObject prefabReference;

    [Tooltip("如果勾选，编辑器每关只允许放置一个此类型实例")]
    public bool allowOnlyOne = false;

}
