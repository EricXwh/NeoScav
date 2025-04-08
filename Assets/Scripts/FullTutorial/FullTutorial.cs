using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FullTutorial : MonoBehaviour
{
    // 指向整个弹出页面的根物体（Panel）
    public GameObject popupPanel;
    // 显示介绍文本
    public Image tutorialImageUI;
    // 关闭按钮
    public Button closeButton;

    public PlayerController playerController;

    // 用于判断是否已经关闭
    private bool isClosed = false;

    private void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }
    }

    /// 显示弹出页面，并设置介绍内容
    public void ShowPopup(Sprite tutorialSprite)
    {
        if (popupPanel != null)
        {
            if (tutorialImageUI != null)
            {
                tutorialImageUI.sprite = tutorialSprite;
            }
            popupPanel.SetActive(true);
            isClosed = false;
            if (playerController != null)
            {
                playerController.canMove = false;
            }
        }
    }
    /// 当玩家点击关闭按钮时调用
    public void OnCloseButtonClicked()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
            isClosed = true;
            if (playerController != null)
            {
                playerController.canMove = true;
            }
        }
    }

    /// 外部可以查询该属性，判断弹窗是否已关闭
    public bool IsClosed
    {
        get { return isClosed; }
    }
}
