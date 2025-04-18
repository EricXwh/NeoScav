using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(menuName = "LevelEditor/MechanismType", fileName = "NewMechanismType")]
public class MechanismType : ScriptableObject {
    [Header("编辑器展示")]
    public string displayName;    // 在 Palette 中显示的名称
    public Sprite icon;          // Palette 图标

    [Header("运行时实例化 (Addressable)")]
    public AssetReferenceGameObject prefabReference;

}
