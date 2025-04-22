using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class DragNumericLabel : MonoBehaviour,
    IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("关联的输入框")]
    public TMP_InputField targetInputField;
    [Header("每像素拖动改变的数值")]
    public float sensitivity = 0.02f;

    bool isDragging = false;
    Vector2 lastMousePos;

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        lastMousePos = eventData.position;
        GetComponent<TMP_Text>().color = Color.yellow;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || targetInputField == null) return;

        Vector2 curr = eventData.position;
        float deltaX = curr.x - lastMousePos.x;
        lastMousePos = curr;

        if (Mathf.Abs(deltaX) < 1f) return;  // 防抖

        // 读取当前数值
        if (float.TryParse(targetInputField.text, out float value))
        {
            value += deltaX * sensitivity;
            // 更新输入框，不触发 OnEndEdit 以免互相循环
            targetInputField.SetTextWithoutNotify(value.ToString("F2"));
            // 手动调用一次结束输入，用来应用变换
            targetInputField.onEndEdit.Invoke(targetInputField.text);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        GetComponent<TMP_Text>().color = Color.white;
    }
}
