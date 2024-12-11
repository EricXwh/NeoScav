using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseClickIndicatorController : MonoBehaviour
{
    public static MouseClickIndicatorController Instance;

    private Animator animator;
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

        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found on MouseClickIndicatorBackground.");
        }
    }

    /// <summary>
    /// 显示鼠标点击标识
    /// </summary>
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

    /// <summary>
    /// 隐藏鼠标点击标识
    /// </summary>
    public void HideIndicator()
    {
        if (isIndicatorActive)
        {
            isIndicatorActive = false;
            gameObject.SetActive(false);
        }
    }
}