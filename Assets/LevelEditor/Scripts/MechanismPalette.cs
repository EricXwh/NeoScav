using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MechanismPalette : MonoBehaviour
{
    [Header("UI 预制件 & 容器")]
    public GameObject paletteItemPrefab;
    public Transform contentRoot; 

    // 持有加载句柄，用于释放
    private AsyncOperationHandle<IList<MechanismType>> loadHandle;

    void Start()
    {
        loadHandle = Addressables.LoadAssetsAsync<MechanismType>(
            /* key */      "mechanismType",
            /* callback */ OnTypeLoaded
        );
        loadHandle.Completed += OnAllTypesLoaded;
    }

    // 每当加载到一个 MechanismType 就被调用
    private void OnTypeLoaded(MechanismType type)
    {
        GameObject go = Instantiate(paletteItemPrefab, contentRoot);
        go.GetComponent<PaletteItem>().Setup(type);
    }

    // 全部加载完毕后的回调
    private void OnAllTypesLoaded(AsyncOperationHandle<IList<MechanismType>> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
            Debug.Log($"已加载 {handle.Result.Count} 种机制类型");
        else
            Debug.LogError($"Addressables 加载失败：{handle.OperationException}");
    }

    private void OnDestroy()
    {
        if (loadHandle.IsValid())
            Addressables.Release(loadHandle);
    }
}
