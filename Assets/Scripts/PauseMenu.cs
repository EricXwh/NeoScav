using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using MaskTransitions;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;           // 暂停菜单面板
    public TextMeshProUGUI collectibleText;   // 显示收集物数量的文本
    public Transform levelButtonContainer;    // 放置关卡按钮的容器
    public GameObject levelButtonPrefab;      // 关卡按钮预制体
    public PlayerController playerController;

    private bool isPaused = false;

    void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            LevelManager.savedLevelIndex = LevelManager.Instance.levels.Length - 1;
            BuildLevelButtons();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    // 切换暂停状态
    public void TogglePause()
    {
        isPaused = !isPaused;
        if (isPaused)
        {
            BuildLevelButtons();

            if (GameManager.Instance.selectedCollectible == CollectibleState.HasCollectible)
            {
                collectibleText.text = "Collectible Count: " + GameManager.Instance.collectedCount;
            }
            else
            {
                collectibleText.text = "";
            }
            
            pausePanel.SetActive(true);
            if (playerController != null)
            {
                playerController.canMove = false;
            }
            //Time.timeScale = 0;
        }
        else
        {
            pausePanel.SetActive(false);
            if (playerController != null)
            {
                playerController.canMove = true;
            }
            // Time.timeScale = 1;
        }
    }


    private void BuildLevelButtons()
    {
        foreach (Transform child in levelButtonContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < LevelManager.Instance.levels.Length; i++)
        {
            GameObject btnObj = Instantiate(levelButtonPrefab, levelButtonContainer);
            Button btn = btnObj.GetComponent<Button>();
            TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            btnText.text = "Level " + (i + 1);

            if (i > LevelManager.savedLevelIndex)
            {
                btn.interactable = false;
                btnText.color = Color.gray;
            }
            else
            {
                btn.interactable = true;
                int index = i; // 记录当前索引，防止闭包问题
                btn.onClick.AddListener(() =>
                {
                    ResumeAndLoadLevel(index);
                });
            }
        }
    }

    public void ResumeAndLoadLevel(int levelIndex)
    {
        if (playerController != null)
        {
            playerController.canMove = true;
        }
        // Time.timeScale = 1;
        LevelManager.savedLevelIndex = levelIndex;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackMenu()
    {
        TransitionManager.Instance.LoadLevel("InitialScene");
    }
}
