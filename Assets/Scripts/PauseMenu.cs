using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;           // 暂停菜单面板
    public TextMeshProUGUI collectibleText;   // 显示收集物数量的文本
    public Transform levelButtonContainer;    // 放置关卡按钮的容器
    public GameObject levelButtonPrefab;      // 关卡按钮预制体

    private bool isPaused = false;

    void Start()
    {
        // 开始时关闭暂停菜单
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    void Update()
    {
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
            // 在激活面板前先生成按钮并更新 UI
            BuildLevelButtons();

            if (GameManager.Instance.selectedCollectible == CollectibleState.HasCollectible)
            {
                collectibleText.text = "Collectible Count: " + GameManager.Instance.collectedCount;
            }
            else
            {
                collectibleText.text = "";
            }
            
            // 然后再激活暂停面板，并暂停游戏
            pausePanel.SetActive(true);
            Time.timeScale = 0;
        }
        else
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1;
        }
    }


    // 构建关卡选择按钮，只允许当前及已通关关卡可选
    private void BuildLevelButtons()
    {
        // 清除容器中原有按钮
        foreach (Transform child in levelButtonContainer)
        {
            Destroy(child.gameObject);
        }

        // 遍历所有关卡，让所有关卡按钮都生成出来
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

    // 选择关卡后恢复游戏并加载指定关卡
    public void ResumeAndLoadLevel(int levelIndex)
    {
        Time.timeScale = 1;
        // 如果你希望用 LevelManager 来管理关卡切换，这里可以调用 LevelManager 的方法，
        // 或者直接加载当前场景并更新 LevelManager.savedLevelIndex
        LevelManager.savedLevelIndex = levelIndex;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
