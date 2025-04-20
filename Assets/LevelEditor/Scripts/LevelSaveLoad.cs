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
    public string    saveFolder = "SavedLevels"; // 子文件夹

    private string SavePath(string name)
    {
        string dir = Path.Combine(Application.persistentDataPath, saveFolder);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        return Path.Combine(dir, name + ".json");
    }

    public void SaveLevel(string levelName)
    {
        var list = new List<PlacedItemData>();

        foreach (var mtr in placementRoot.GetComponentsInChildren<MechanismTypeReference>(true))
        {
            GameObject go = mtr.gameObject;

            // 直接用 mtr.instanceId 而不是 ParseId(go.name)
            var pd = new PlacedItemData {
                id = mtr.instanceId,
                mechanismTypeAddress = mtr.prefabReference.RuntimeKey.ToString(),
                position = go.transform.position,
                rotation = go.transform.eulerAngles,
                scale = go.transform.localScale,
                linkedIds = (go.GetComponent<ILinkable>()?.LinkedMechanisms
                                .Select(l => {
                                    var linkRef = l.GetComponent<MechanismTypeReference>();
                                    return linkRef != null 
                                        ? linkRef.instanceId 
                                        : 0;
                                })
                                .ToArray()) ?? new int[0],
                properties = CollectProperties(go)
            };
            list.Add(pd);
        }

        var data = new UserLevelData { items = list };
        string json = JsonUtility.ToJson(data, true);
        string path = SavePath(levelName);
        File.WriteAllText(path, json);
        Debug.Log($"Level '{levelName}' saved to {path}");
    }

    public void LoadLevel(string levelName)
    {
        string path = SavePath(levelName);
        if (!File.Exists(path)) { Debug.LogError($"存档不存在：{path}"); return; }
        string json = File.ReadAllText(path);
        var data = JsonUtility.FromJson<UserLevelData>(json);

        // 清空旧实例
        foreach (Transform c in placementRoot) Destroy(c.gameObject);

        var idMap  = new Dictionary<int, GameObject>();
        var compMap= new Dictionary<int, List<MonoBehaviour>>();
        var ops    = new List<AsyncOperationHandle<GameObject>>();

        foreach (var pd in data.items)
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(pd.mechanismTypeAddress);
            ops.Add(handle);
            handle.Completed += op =>
            {
                GameObject go = Instantiate(op.Result, placementRoot);

                // 先恢复 Transform
                go.transform.position    = pd.position;
                go.transform.eulerAngles  = pd.rotation;
                go.transform.localScale   = pd.scale;

                // 添加 Selectable
                if (go.GetComponent<Selectable>() == null)
                    go.AddComponent<Selectable>();

                // 恢复 MechanismTypeReference + instanceId
                var mtr = go.AddComponent<MechanismTypeReference>();
                mtr.prefabReference = new AssetReferenceGameObject(pd.mechanismTypeAddress);
                mtr.instanceId      = pd.id;

                // 用 instanceId 给物体命名（可选）
                go.name = $"{op.Result.name}_{pd.id}";

                idMap[pd.id]       = go;
                compMap[pd.id]     = go.GetComponents<MonoBehaviour>().ToList();
            };
        }

        StartCoroutine(AfterLoad(data.items, ops.ToArray(), idMap, compMap));
    }

    private IEnumerator AfterLoad(
        List<PlacedItemData> items,
        AsyncOperationHandle<GameObject>[] ops,
        Dictionary<int, GameObject> idMap,
        Dictionary<int, List<MonoBehaviour>> compMap)
    {
        foreach (var op in ops) yield return op;

        // 恢复 Links
        foreach (var pd in items)
        {
            if (!idMap.ContainsKey(pd.id)) continue;
            var go = idMap[pd.id];
            if (go.GetComponent<ILinkable>() is ILinkable linkable)
            {
                linkable.LinkedMechanisms =
                    pd.linkedIds.Where(idMap.ContainsKey)
                                .Select(i=>idMap[i].GetComponent<MechanismBase>())
                                .ToArray();
            }
        }

        // 恢复 Properties
        foreach (var pd in items)
        {
            if (!compMap.ContainsKey(pd.id)) continue;
            foreach (var prop in pd.properties)
            {
                var comp = compMap[pd.id]
                    .FirstOrDefault(c=>c.GetType().FullName==prop.componentType);
                if (comp==null) continue;
                var f = comp.GetType().GetField(
                    prop.fieldName,
                    BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic
                );
                if (f==null) continue;
                if (f.FieldType==typeof(int) && int.TryParse(prop.value,out var iv))
                    f.SetValue(comp, iv);
                else if(f.FieldType==typeof(float)&&float.TryParse(prop.value,out var fv))
                    f.SetValue(comp, fv);
                else if(f.FieldType==typeof(string))
                    f.SetValue(comp, prop.value);
            }
        }
        Debug.Log("Load complete");
    }

    // 辅助：解析 id
    private int ParseId(string name)
    {
        var ss=name.Split('_');
        return (ss.Length>1 && int.TryParse(ss.Last(),out var x))? x : name.GetHashCode();
    }

    // 辅助：收集属性
    private List<PropertyData> CollectProperties(GameObject go)
    {
        var list=new List<PropertyData>();
        foreach(var comp in go.GetComponents<MonoBehaviour>())
        {
            var t=comp.GetType();
            var fields=t.GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic)
                        .Where(f=>f.IsPublic||f.GetCustomAttribute<SerializeField>()!=null)
                        .Where(f=>
                            f.FieldType==typeof(int)||
                            f.FieldType==typeof(float)||
                            f.FieldType==typeof(string)
                        );
            foreach(var f in fields)
            {
                list.Add(new PropertyData{
                    componentType=t.FullName,
                    fieldName=f.Name,
                    value=f.GetValue(comp)?.ToString()
                });
            }
        }
        return list;
    }

    /// <summary>获取所有存档名（不带后缀）</summary>
    public string[] GetSavedLevelNames()
    {
        string dir = Path.Combine(Application.persistentDataPath, saveFolder);
        if (!Directory.Exists(dir)) return new string[0];
        return Directory.GetFiles(dir, "*.json")
                        .Select(f=>Path.GetFileNameWithoutExtension(f))
                        .ToArray();
    }
}
