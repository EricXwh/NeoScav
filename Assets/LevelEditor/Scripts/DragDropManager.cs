using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DragDropManager : MonoBehaviour
{
    public static DragDropManager Instance { get; private set; }

    [Header("UI 拦截区块")]
    public RectTransform[] uiBlockRects;

    [Header("地板层 (Floor Tile) LayerMask")]
    public LayerMask floorLayer;

    [Header("拖拽预览材质")]
    public Material previewGreen;
    public Material previewRed;

    private MechanismType draggingType;
    private AsyncOperationHandle<GameObject> loadHandle;
    private GameObject previewObject;
    private bool previewValid;
    private float previewHalfHeight = 0.5f;

    void Awake()
    {
        Instance = this;
    }

    public void BeginDrag(MechanismType type)
    {
         if (type.allowOnlyOne)
        {
            var myKey = type.prefabReference.RuntimeKey;
            bool exists = LevelEditor.Instance.placementRoot
                .GetComponentsInChildren<MechanismTypeReference>(true)
                .Any(mtr => mtr.prefabReference != null
                        && mtr.prefabReference.RuntimeKey.Equals(myKey));
            if (exists)
            {
                Debug.LogWarning($"本关只允许一个“{type.displayName}”");
                return;
            }
        }

        if (loadHandle.IsValid())
            Addressables.Release(loadHandle);
        if (previewObject != null)
            Destroy(previewObject);

        draggingType = type;
        loadHandle = type.prefabReference.LoadAssetAsync<GameObject>();
        loadHandle.Completed += OnPreviewLoaded;
    }

    private void OnPreviewLoaded(AsyncOperationHandle<GameObject> op)
    {
        if (op.Status != AsyncOperationStatus.Succeeded || draggingType == null)
            return;
        previewObject = Instantiate(op.Result);
        previewObject.name = draggingType.displayName + "_Preview";

        foreach (var mb in previewObject.GetComponentsInChildren<MonoBehaviour>())
            Destroy(mb);
        foreach (var col in previewObject.GetComponentsInChildren<Collider>())
            Destroy(col);
            var renderers = previewObject.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                b.Encapsulate(renderers[i].bounds);
            previewHalfHeight = b.extents.y;
        }
        else
        {
            previewHalfHeight = 0.5f;
        }

        previewValid = true;
        foreach (var r in previewObject.GetComponentsInChildren<Renderer>())
            r.material = previewGreen;
    }

    void Update()
    {
        if (previewObject == null || draggingType == null)
            return;

        if (IsPointerOverBlockedUI())
        {
            previewObject.SetActive(false);
            previewValid = false;
            return;
        }

        previewObject.SetActive(true);

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 targetPos;
        bool valid = false;

        if (draggingType.displayName == "FloorTile")
        {
            Plane p = new Plane(Vector3.up, Vector3.zero);
            if (p.Raycast(ray, out float enter))
            {
                targetPos = ray.GetPoint(enter);
                valid = true;
            }
            else
            {
                return;
            }
        }
        else
        {
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, floorLayer))
            {
                targetPos = hit.point + Vector3.up * previewHalfHeight;
                valid = true;
            }
            else
            {
                Plane p = new Plane(Vector3.up, Vector3.zero);
                if (!p.Raycast(ray, out float enter)) return;
                targetPos = ray.GetPoint(enter) + Vector3.up * previewHalfHeight;
            }
        }

        float grid = 2f;
        targetPos = new Vector3(
            Mathf.Round(targetPos.x / grid) * grid,
            targetPos.y,
            Mathf.Round(targetPos.z / grid) * grid
        );

        previewObject.transform.position = targetPos;

        if (valid != previewValid)
        {
            var mat = valid ? previewGreen : previewRed;
            foreach (var r in previewObject.GetComponentsInChildren<Renderer>())
                r.material = mat;
            previewValid = valid;
        }
    }

    public void EndDrag()
    {
        if (previewObject != null)
            Destroy(previewObject);

        if (draggingType == null || !previewValid)
        {
            draggingType = null;
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 spawnPos;
        if (draggingType.displayName == "Floor Tile")
        {
            Plane p = new Plane(Vector3.up, Vector3.zero);
            p.Raycast(ray, out float enter);
            spawnPos = ray.GetPoint(enter);
        }
        else
        {
            Physics.Raycast(ray, out RaycastHit hit, 100f, floorLayer);
            spawnPos = hit.point + Vector3.up * previewHalfHeight;
        }
        float grid = 2f;
        spawnPos = new Vector3(
            Mathf.Round(spawnPos.x / grid) * grid,
            spawnPos.y,
            Mathf.Round(spawnPos.z / grid) * grid
        );

        var type      = draggingType;
        var prefabRef = type.prefabReference;

        prefabRef.InstantiateAsync(
        spawnPos, Quaternion.identity, LevelEditor.Instance.placementRoot
        ).Completed += handle =>
        {
            GameObject go = handle.Result;

            var mtr = go.GetComponent<MechanismTypeReference>()
                ?? go.AddComponent<MechanismTypeReference>();
            mtr.prefabReference = prefabRef;

            mtr.instanceId = LevelEditor.Instance.AllocateInstanceId();
            int idx = type.allowOnlyOne
                ? 0
                : LevelEditor.Instance.AllocateDisplayIndex(type.displayName);
            go.name = type.allowOnlyOne
                ? type.displayName
                : $"{type.displayName}_{idx}";

            if (go.GetComponent<Selectable>() == null)
                go.AddComponent<Selectable>();

            LevelEditor.Instance.SelectObject(go);
        };

        if (loadHandle.IsValid())
            Addressables.Release(loadHandle);

        draggingType = null;
        previewValid = false;
    }

    private bool IsPointerOverBlockedUI()
    {
        foreach (var rect in uiBlockRects)
            if (RectTransformUtility.RectangleContainsScreenPoint(
                    rect, Input.mousePosition))
                return true;
        return false;
    }
}
