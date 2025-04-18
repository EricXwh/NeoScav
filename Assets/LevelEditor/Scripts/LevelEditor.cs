using System;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;

public class LevelEditor : MonoBehaviour
{
    public static LevelEditor Instance { get; private set; }

    [Header("放置设置")]
    public Transform placementRoot;
    public LayerMask groundLayer;

    // 选中与放置
    private MechanismType placingType;
    private GameObject selected;
    // 链接模式开关
    private bool isLinkMode = false;
    public event Action<GameObject> OnSelectionChanged;

    private Dictionary<string, int> nameCounters = new Dictionary<string, int>();
    
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        // 链接模式：点击目标机制 -> 建立关联
        if (isLinkMode && Input.GetMouseButtonDown(0) && selected != null)
        {
            TryLinkToAnother();
            return;
        }

        // 放置模式
        if (placingType != null)
        {
            HandlePlacement();
        }
    }

    public void BeginPlacing(MechanismType type)
    {
        placingType = type;
    }

    public void SelectObject(GameObject go)
    {
        placingType = null;
        selected = go;
        OnSelectionChanged?.Invoke(selected);
    }

    public void BeginLinkMode()
    {
        if (selected != null && selected.GetComponent<ILinkable>() != null)
            isLinkMode = true;
    }

    private void HandlePlacement()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (placingType == null) return;

        // 1) 缓存要用的值
        var type      = placingType;
        var prefabRef = type.prefabReference;
        var baseName  = type.displayName;

        // 2) 立即退出放置模式
        placingType = null;

        // 3) 发起异步实例化
        AsyncOperationHandle<GameObject> handle =
            prefabRef.InstantiateAsync(
                position: hitPoint(),              // 封装了射线获得的 hit.point
                rotation: Quaternion.identity,
                parent:   placementRoot
            );

        handle.Completed += op =>
        {
            GameObject go = op.Result;

            // 4) 命名
            if (!nameCounters.ContainsKey(baseName))
                nameCounters[baseName] = 0;
            nameCounters[baseName]++;
            go.name = $"{baseName}_{nameCounters[baseName]}";

            // 5) 确保可选中
            if (go.GetComponent<Selectable>() == null)
                go.AddComponent<Selectable>();

            // 6) 保存引用
            var mtr = go.AddComponent<MechanismTypeReference>();
            mtr.prefabReference = prefabRef;

            // 7) 自动选中
            SelectObject(go);
        };
    }

    private void TryLinkToAnother()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit)) { isLinkMode = false; return; }

        var target = hit.collider.GetComponent<Selectable>()?.gameObject;
        if (target != null && target != selected)
        {
            var sourceLinkable = selected.GetComponent<ILinkable>();
            if (sourceLinkable != null)
            {
                // 追加 target 上的 MechanismBase
                var mech = target.GetComponent<MechanismBase>();
                if (mech != null)
                {
                    var arr = sourceLinkable.LinkedMechanisms;
                    int len = arr?.Length ?? 0;
                    var newArr = new MechanismBase[len + 1];
                    if (arr != null) arr.CopyTo(newArr, 0);
                    newArr[len] = mech;
                    sourceLinkable.LinkedMechanisms = newArr;
                    OnSelectionChanged?.Invoke(selected);
                }
            }
        }
        isLinkMode = false;
    }

    /// <summary>
    /// 射线检测，返回地面击中点
    /// </summary>
    private Vector3 hitPoint()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, 100f, groundLayer))
            return hit.point;
        return Vector3.zero;
    }
}
