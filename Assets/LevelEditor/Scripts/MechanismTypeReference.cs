using UnityEngine;
using UnityEngine.AddressableAssets;

/// 保存本实例对应的 Addressables 预制体引用，
/// 以便保存/加载时能够反向获取 Addressables Key。
public class MechanismTypeReference : MonoBehaviour
{
    [Tooltip("本实例所基于的 Addressable Prefab")]
    public AssetReferenceGameObject prefabReference;

    [HideInInspector]
    public int instanceId;
}
