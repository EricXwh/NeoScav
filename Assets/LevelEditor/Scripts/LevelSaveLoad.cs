using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LevelSaveLoad : MonoBehaviour
{
    [Header("引用")]
    public Transform placementRoot;
    public string    saveFileName = "level_config.json";

    /// <summary>
    /// 保存当前关卡布局（包括 Properties）
    /// </summary>
    public void SaveLevel()
    {
        var list = new List<PlacedItemData>();

        // 遍历所有放置的 MechanismBase
        foreach(var mtr in placementRoot.GetComponentsInChildren<MechanismTypeReference>(true))
        {
            GameObject go = mtr.gameObject;
            Debug.Log(go);
            var pd = new PlacedItemData();
            // id 解析
            var split = go.name.Split('_');
            pd.id = (split.Length > 1 && int.TryParse(split.Last(), out var x)) 
                    ? x : go.GetInstanceID();

            // Addressables 地址
            pd.mechanismTypeAddress = mtr.prefabReference.RuntimeKey.ToString();

            // Transform
            pd.position = go.transform.position;
            pd.rotation = go.transform.eulerAngles;
            pd.scale    = go.transform.localScale;

            // Links
            if (go.TryGetComponent<ILinkable>(out var linkable))
                pd.linkedIds = linkable.LinkedMechanisms
                                .Select(l => {
                                    var ss = l.gameObject.name.Split('_');
                                    return (ss.Length>1 && int.TryParse(ss.Last(), out var y)) 
                                            ? y : l.gameObject.GetInstanceID();
                                })
                                .ToArray();
            else
                pd.linkedIds = new int[0];

            // —— 新：收集 Properties 字段 —— 
            pd.properties = new List<PropertyData>();
            foreach (var comp in go.GetComponents<MonoBehaviour>())
            {
                var type = comp.GetType();
                var fields = type.GetFields(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                ).Where(f => f.IsPublic || f.GetCustomAttribute<SerializeField>() != null);

                foreach (var f in fields)
                {
                    if (f.FieldType == typeof(int) ||
                        f.FieldType == typeof(float) ||
                        f.FieldType == typeof(string))
                    {
                        var val = f.GetValue(comp);
                        pd.properties.Add(new PropertyData {
                            componentType = type.FullName,
                            fieldName     = f.Name,
                            value         = val?.ToString()
                        });
                    }
                }
            }

            list.Add(pd);
        }

        var data = new UserLevelData { items = list };
        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine(Application.persistentDataPath, saveFileName);
        File.WriteAllText(path, json);
        Debug.Log($"Save Level → {path}");
    }

    /// <summary>
    /// 加载关卡，并恢复所有设置（包括 Properties）
    /// </summary>
    public void LoadLevel()
    {
        string path = Path.Combine(Application.persistentDataPath, saveFileName);
        if (!File.Exists(path))
        {
            Debug.LogError("Load 失败，文件不存在: " + path);
            return;
        }
        string json = File.ReadAllText(path);
        var data = JsonUtility.FromJson<UserLevelData>(json);

        // 1) 清空旧实例
        foreach (Transform c in placementRoot) Destroy(c.gameObject);

        // 2) 实例化所有项，并记录 id→GameObject 及组件列表
        var idMap = new Dictionary<int, GameObject>();
        var compMap = new Dictionary<int, List<MonoBehaviour>>(); // id -> 所有关联脚本
        var ops = new List<AsyncOperationHandle<GameObject>>();

        foreach (var pd in data.items)
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(pd.mechanismTypeAddress);
            ops.Add(handle);

            handle.Completed += op =>
            {
                var go = Instantiate(op.Result, placementRoot);
                go.name = $"{op.Result.name}_{pd.id}";
                go.transform.position    = pd.position;
                go.transform.eulerAngles  = pd.rotation;
                go.transform.localScale   = pd.scale;
                if (go.GetComponent<Selectable>() == null)
                    go.AddComponent<Selectable>();

                idMap[pd.id] = go;
                // 缓存它所有脚本组件，方便后续写值
                compMap[pd.id] = go.GetComponents<MonoBehaviour>().ToList();
            };
        }

        // 3) 等所有 Prefab 加载完，再恢复 Links + Properties
        StartCoroutine(WaitAndRestore(data.items, ops.ToArray(), idMap, compMap));
    }

    private IEnumerator WaitAndRestore(
        List<PlacedItemData> items,
        AsyncOperationHandle<GameObject>[] ops,
        Dictionary<int, GameObject> idMap,
        Dictionary<int, List<MonoBehaviour>> compMap)
    {
        foreach (var op in ops) yield return op;

        // 恢复 Links
        foreach (var pd in items)
        {
            if (!idMap.TryGetValue(pd.id, out var go)) continue;
            if (go.GetComponent<ILinkable>() is ILinkable linkable)
            {
                // 重新映射 id → MechanismBase
                var arr = pd.linkedIds
                     .Where(idMap.ContainsKey)
                     .Select(id => idMap[id].GetComponent<MechanismBase>())
                     .ToArray();
                linkable.LinkedMechanisms = arr;
            }
        }

        // 恢复 Properties
        foreach (var pd in items)
        {
            if (!compMap.TryGetValue(pd.id, out var comps)) continue;
            foreach (var prop in pd.properties)
            {
                // 找到对应组件
                var comp = comps.FirstOrDefault(c => c.GetType().FullName == prop.componentType);
                if (comp == null) continue;
                var f = comp.GetType().GetField(
                    prop.fieldName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );
                if (f == null) continue;

                // 根据 FieldType 解析并赋值
                if (f.FieldType == typeof(int) && int.TryParse(prop.value, out var iv))
                    f.SetValue(comp, iv);
                else if (f.FieldType == typeof(float) && float.TryParse(prop.value, out var fv))
                    f.SetValue(comp, fv);
                else if (f.FieldType == typeof(string))
                    f.SetValue(comp, prop.value);
            }
        }

        Debug.Log("Load Level 完成（含 Properties）");
    }
}
