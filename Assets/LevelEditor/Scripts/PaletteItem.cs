using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class PaletteItem : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IInitializePotentialDragHandler
{
    public Image    iconImage;
    public TMP_Text nameLabel;

    private MechanismType type;

    public void Setup(MechanismType mechanismType)
    {
        type = mechanismType;
        iconImage.sprite = mechanismType.icon;
        if (nameLabel) nameLabel.text = mechanismType.displayName;

        var btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => LevelEditor.Instance.BeginPlacing(type));
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        eventData.useDragThreshold = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        DragDropManager.Instance.BeginDrag(type, iconImage.sprite);
    }

    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DragDropManager.Instance.EndDrag();
    }
}
