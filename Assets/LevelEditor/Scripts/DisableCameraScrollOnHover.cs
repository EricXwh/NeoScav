using UnityEngine;
using UnityEngine.EventSystems;

public class DisableCameraScrollOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static bool IsPointerOverScroll = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        IsPointerOverScroll = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsPointerOverScroll = false;
    }
}
