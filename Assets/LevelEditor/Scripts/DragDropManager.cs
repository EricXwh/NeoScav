using UnityEngine;
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

        // if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        // {
        //     draggingType = null;
        //     return;
        // }

        // 否则射线检测地面
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, 100f, LevelEditor.Instance.groundLayer))
        {
            var type = draggingType;
            var prefabRef = type.prefabReference;

            Vector3 spawnPos = hit.point + Vector3.up * 0.5f;

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

                go.name = $"{type.displayName}_{displayIndex}";

                if (go.GetComponent<Selectable>() == null)
                    go.AddComponent<Selectable>();

                LevelEditor.Instance.SelectObject(go);
            };
        }

        draggingType = null;
    }
}
