using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using MaskTransitions;

public class LevelEditor : MonoBehaviour
{
    public static LevelEditor Instance { get; private set; }

    [Header("放置设置")]
    public Transform placementRoot;
    public LayerMask groundLayer;
    [Header("试玩设置")]
    public GameObject playerPrefab;
    public Canvas    editorCanvas;

    public RectTransform[] uiBlockRects;
    private EditorCameraController ecam;

    // 选中与放置
    private MechanismType placingType;
    private GameObject selected;
    // 链接模式开关
    private bool isLinkMode = false;
    public event Action<GameObject> OnSelectionChanged;

    private Dictionary<string, int> nameCounters = new Dictionary<string, int>();
    private int nextInstanceId = 1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        ecam = Camera.main.GetComponent<EditorCameraController>();
    }

    private void Update()
    {
        if (isLinkMode && Input.GetMouseButtonDown(0) && selected != null)
        {
            TryLinkToAnother();
            return;
        }

        if (placingType != null)
        {
            HandlePlacement();
        }
    }

    public void BeginPlacing(MechanismType type)
    {
        if (type.allowOnlyOne)
        {
            bool exists = placementRoot
                .GetComponentsInChildren<MechanismTypeReference>(true)
                .Any(mtr => mtr.prefabReference.RuntimeKey.ToString()
                        == type.prefabReference.RuntimeKey.ToString());
            if (exists)
            {
                Debug.LogWarning($"本关只允许一个“{type.displayName}”");
                return;
            }
        }
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
        if (IsPointerOverBlockedUI() || !Input.GetMouseButtonDown(0) || placingType == null) return;

        float grid = 2f;
        var type = placingType;
        var prefabRef = type.prefabReference;
        placingType  = null;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 spawnPos;
        bool isFloor = type.displayName == "FloorTile";

        if (isFloor)
        {
            Plane p = new Plane(Vector3.up, Vector3.zero);
            if (!p.Raycast(ray, out float enter)) return;
            spawnPos = p.Raycast(ray, out enter)
                    ? ray.GetPoint(enter)
                    : Vector3.zero;
        }
        else
        {
            if (!Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
                return;

            spawnPos = hit.point;
        }

        spawnPos = new Vector3(
            Mathf.Round(spawnPos.x / grid) * grid,
            spawnPos.y,
            Mathf.Round(spawnPos.z / grid) * grid
        );

        prefabRef.InstantiateAsync(spawnPos, Quaternion.identity, placementRoot)
        .Completed += op =>
    {
        GameObject go = op.Result;

        float halfH = 0f;
        if (!isFloor)
        {
            var rends = go.GetComponentsInChildren<Renderer>();
            if (rends.Length > 0)
            {
                Bounds b = rends[0].bounds;
                for (int i = 1; i < rends.Length; i++)
                    b.Encapsulate(rends[i].bounds);
                halfH = b.extents.y;
            }
        }

        if (!isFloor)
        {
            var pos = go.transform.position;
            pos.y += halfH;
            go.transform.position = pos;
        }

        var mtr = go.GetComponent<MechanismTypeReference>()
               ?? go.AddComponent<MechanismTypeReference>();
        mtr.prefabReference = prefabRef;
        mtr.instanceId      = AllocateInstanceId();

        int displayIndex = isFloor || type.allowOnlyOne
                         ? 0
                         : AllocateDisplayIndex(type.displayName);
        go.name = isFloor || type.allowOnlyOne
               ? type.displayName
               : $"{type.displayName}_{displayIndex}";

        if (go.GetComponent<Selectable>() == null)
            go.AddComponent<Selectable>();
        SelectObject(go);
    };
}

    private void TryLinkToAnother()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit))
        {
            isLinkMode = false;
            return;
        }

        var target = hit.collider.GetComponent<Selectable>()?.gameObject;
        if (target != null && target != selected)
        {
            var sourceLinkable = selected.GetComponent<ILinkable>();
            if (sourceLinkable != null)
            {
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

    private Vector3 hitPoint()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, 100f, groundLayer))
            return hit.point;
        return Vector3.zero;
    }

    public void StartPlayTest()
    {
        if (editorCanvas) editorCanvas.gameObject.SetActive(false);
        if (ecam) ecam.enabled = false;
        if (ecam) ecam.gameObject.SetActive(false);

        var spawn = GameObject.FindWithTag("PlayerSpawn");
        Vector3 pos = spawn ? spawn.transform.position : Vector3.zero;
        Quaternion rot = spawn ? spawn.transform.rotation : Quaternion.identity;
        var player = Instantiate(playerPrefab, pos, rot);
        player.name = "Player_PlayTest";
    }

    public void StopPlayTest()
    {
        var pl = GameObject.Find("Player_PlayTest");
        if (pl) Destroy(pl);

        if (ecam) ecam.gameObject.SetActive(true);
        if (ecam) ecam.enabled = true;
        if (editorCanvas) editorCanvas.gameObject.SetActive(true);
    }

    private bool IsPointerOverBlockedUI()
    {
        foreach (var rect in uiBlockRects)
            if (RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition))
                return true;
        return false;
    }

    public int AllocateInstanceId()
    {
        return nextInstanceId++;
    }

    public int AllocateDisplayIndex(string baseName)
    {
        if (!nameCounters.ContainsKey(baseName))
            nameCounters[baseName] = 0;
        nameCounters[baseName]++;
        return nameCounters[baseName];
    }

    public void ClearSelected()
    {
        if (selected != null)
        {
            Destroy(selected);
            SelectObject(null);
        }
    }

    public void ClearAll()
    {
        foreach (var child in placementRoot.Cast<Transform>().ToArray())
            Destroy(child.gameObject);

        SelectObject(null);
    }

    public void BackMenu()
    {
        TransitionManager.Instance.LoadLevel("InitialScene");
    }
}
