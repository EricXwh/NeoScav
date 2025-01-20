using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseClickIndicatorController : MonoBehaviour
{
    public static MouseClickIndicatorController Instance;

    private bool isIndicatorActive = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// 显示鼠标点击标识
    public void ShowIndicator(Vector3 screenPosition)
    {
        if (!isIndicatorActive)
        {
            isIndicatorActive = true;
            gameObject.SetActive(true);
        }

        // 设置位置
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.position = screenPosition;
    }

    /// 隐藏鼠标点击标识
    public void HideIndicator()
    {
        if (isIndicatorActive)
        {
            isIndicatorActive = false;
            gameObject.SetActive(false);
        }
    }
}