using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DragDropManager : MonoBehaviour
{
    public static DragDropManager Instance { get; private set; }

    [Header("拖拽预览")]
    public Canvas dragCanvas;
    public Image dragIcon;

    public RectTransform[] uiBlockRects;
    private MechanismType draggingType;
    private AsyncOperationHandle<GameObject> loadHandle;

    void Awake()
    {
        Instance = this;
        dragIcon.gameObject.SetActive(false);
    }

    void Update()
    {
        if (dragIcon.gameObject.activeSelf)
        {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                dragCanvas.transform as RectTransform,
                Input.mousePosition,
                null,
                out pos
            );
            dragIcon.rectTransform.anchoredPosition = pos;
        }
    }

    public void BeginDrag(MechanismType type, Sprite iconSprite)
    {
        if(type.allowOnlyOne)
        {
            bool exists = LevelEditor.Instance.placementRoot
            .GetComponentsInChildren<MechanismTypeReference>(true)
            .Any(mtr => mtr.prefabReference.RuntimeKey.ToString()
                        == type.prefabReference.RuntimeKey.ToString());
            if (exists)
            {
                Debug.LogWarning($"本关只允许一个“{type.displayName}”");
                return;
            }
        }
        draggingType = type;
        dragIcon.sprite = iconSprite;
        dragIcon.SetNativeSize();
        dragIcon.color = new Color(1,1,1,0.7f);
        dragIcon.gameObject.SetActive(true);
    }

    public void EndDrag()
    {
        dragIcon.gameObject.SetActive(false);

        if (draggingType == null)
            return;

        if (IsPointerOverBlockedUI())
        {
            draggingType = null;
            return;
        }

        float grid = 2f;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 spawnPos;

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, LevelEditor.Instance.groundLayer))
        {
            Vector3 raw = hit.point + Vector3.up * 0.5f;
            spawnPos = new Vector3(
                Mathf.Round(raw.x / grid) * grid,
                raw.y,
                Mathf.Round(raw.z / grid) * grid
            );
        }
        else
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            if (plane.Raycast(ray, out float enter))
            {
                Vector3 raw = ray.GetPoint(enter) + Vector3.up * 0.5f;
                spawnPos = new Vector3(
                    Mathf.Round(raw.x / grid) * grid,
                    raw.y,
                    Mathf.Round(raw.z / grid) * grid
                );
            }
            else
            {
                draggingType = null;
                return;
            }
        }
        var type = draggingType;
        var prefabRef = type.prefabReference;

        // 异步实例化
        var handle = prefabRef.InstantiateAsync(
            spawnPos,
            Quaternion.identity,
            LevelEditor.Instance.placementRoot
        );
        handle.Completed += op =>
        {
            GameObject go = op.Result;

            var mtr = go.GetComponent<MechanismTypeReference>()
                    ?? go.AddComponent<MechanismTypeReference>();
            mtr.prefabReference = prefabRef;

            mtr.instanceId = LevelEditor.Instance.AllocateInstanceId();

            int displayIndex = LevelEditor.Instance.AllocateDisplayIndex(type.displayName);

            go.name = type.allowOnlyOne ? type.displayName : $"{type.displayName}_{displayIndex}";

            if (go.GetComponent<Selectable>() == null)
                go.AddComponent<Selectable>();

            LevelEditor.Instance.SelectObject(go);
        };
        

        draggingType = null;
    }

    private bool IsPointerOverBlockedUI()
    {
        foreach (var rect in uiBlockRects)
            if (RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition))
                return true;
        return false;
    }
}
