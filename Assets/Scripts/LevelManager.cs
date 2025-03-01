using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 
using TMPro;
using System.IO;
using System.Text;
using System;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    // 使用静态变量保存当前关卡索引
    public static int savedLevelIndex = 0;
    // 静态数组保存每一关的统计数据
    private static LevelStatistics[] levelStats;

    private static float[] levelAccumulatedTimes;

    private static bool[] tutorialPopupShown;

    [Header("所有关卡的容器（按关卡顺序排列）")]
    public GameObject[] levels;

    private int currentLevelIndex = 0;

    public Transform playerTransform;

    // 缓存每个关卡容器对应的生成点引用
    private Dictionary<GameObject, Transform> levelSpawnPoints = new Dictionary<GameObject, Transform>();

    public CinemachineBrain cinemachineBrain;

    public FullTutorial tutorialPopup;

    // 当前关卡的开始时间（本次运行）
    private float levelStartTime;

    public GameObject summaryCanvas;
    public TextMeshProUGUI summaryText;

    private bool isResetting = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // 恢复当前关卡索引
        currentLevelIndex = savedLevelIndex;

        // 如果统计数据数组为空，则根据关卡数量初始化
        if (levelStats == null || levelStats.Length != levels.Length)
        {
            levelStats = new LevelStatistics[levels.Length];
            for (int i = 0; i < levels.Length; i++)
            {
                levelStats[i] = new LevelStatistics();
            }
        }

        if (levelAccumulatedTimes == null || levelAccumulatedTimes.Length != levels.Length)
        {
            levelAccumulatedTimes = new float[levels.Length];
            for (int i = 0; i < levels.Length; i++)
            {
                levelAccumulatedTimes[i] = 0f;
            }
        }

        if (tutorialPopupShown == null || tutorialPopupShown.Length != levels.Length)
        {
            tutorialPopupShown = new bool[levels.Length];
            for (int i = 0; i < levels.Length; i++)
            {
                tutorialPopupShown[i] = false;
            }
        }

        if (playerTransform == null)
        {
            Debug.LogError("请设置玩家的 Transform");
        }

        if (cinemachineBrain == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                cinemachineBrain = mainCam.GetComponent<CinemachineBrain>();
            }
        }

        // 缓存每个关卡的生成点
        foreach (GameObject level in levels)
        {
            if (level == null)
                continue;

            Transform spawnPoint = level.transform.Find("PlayerSpawn");
            if (spawnPoint != null)
            {
                levelSpawnPoints[level] = spawnPoint;
            }
            else
            {
                Debug.LogWarning("关卡 " + level.name + " 没有找到生成点 (PlayerSpawn)！");
            }
        }
    }

    private IEnumerator Start()
    {
        // 仅激活当前关卡，其他关卡禁用
        for (int i = 0; i < levels.Length; i++)
        {
            levels[i].SetActive(i == currentLevelIndex);
        }

        // 重新设置玩家位置为当前关卡的生成点
        GameObject currentLevel = levels[currentLevelIndex];
        if (levelSpawnPoints.TryGetValue(currentLevel, out Transform spawnPoint))
        {
            CharacterController controller = playerTransform.GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false;
                playerTransform.position = spawnPoint.position;
                controller.enabled = true;
            }
            else
            {
                playerTransform.position = spawnPoint.position;
            }
        }
        else
        {
            Debug.LogWarning("当前关卡未找到生成点！");
        }

        levelStartTime = Time.time - levelAccumulatedTimes[currentLevelIndex];

        yield return null;

        // 如果是 Full Tutorial 模式且当前关卡需要显示说明，则显示弹窗
        LevelData levelData = currentLevel.GetComponent<LevelData>();
        if (levelData != null &&
            levelData.introducesNewMechanism &&
            !tutorialPopupShown[currentLevelIndex])
        {
            if (tutorialPopup != null)
            {
                tutorialPopup.ShowPopup(levelData.mechanismTutorialMessage);
                while (!tutorialPopup.IsClosed)
                {
                    yield return null;
                }
                tutorialPopupShown[currentLevelIndex] = true;
            }
        }
    }

    private void Update()
    {        
        if (Input.GetKeyDown(KeyCode.R) && currentLevelIndex < levels.Length)
        {
            ResetCurrentLevel();
        }

        if (playerTransform.position.y < -10 && currentLevelIndex < levels.Length && !isResetting)
        {
            isResetting = true;
            ResetCurrentLevel();
        }
    }


    /// 复原当前关卡：按 R 键时，记录重置次数，然后重载当前 Scene
    public void ResetCurrentLevel()
    {
        // 增加当前关卡的重置次数
        levelStats[currentLevelIndex].resetCount++;
        // 重新加载整个 Scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// 外部调用 NextLevel() 触发关卡切换
    public void NextLevel()
    {
        StartCoroutine(NextLevelCoroutine());
    }

    private IEnumerator NextLevelCoroutine()
    {
        // 禁用当前关卡
        if (currentLevelIndex < levels.Length)
        {
            levels[currentLevelIndex].SetActive(false);
        }

        // 在切换关卡前，将当前关卡的用时记录下来
        if (currentLevelIndex < levels.Length)
        {
            levelStats[currentLevelIndex].finalTime = Time.time - levelStartTime;
        }

        currentLevelIndex++;
        savedLevelIndex = currentLevelIndex;

        if (currentLevelIndex < levels.Length)
        {
            levelAccumulatedTimes[currentLevelIndex] = 0f;
            GameObject nextLevel = levels[currentLevelIndex];
            nextLevel.SetActive(true);
            Debug.Log("切换到关卡：" + (currentLevelIndex + 1));

            if (levelSpawnPoints.TryGetValue(nextLevel, out Transform spawnPoint))
            {
                if (playerTransform != null)
                {
                    Vector3 oldPos = playerTransform.position;
                    CharacterController controller = playerTransform.GetComponent<CharacterController>();
                    if (controller != null)
                    {
                        controller.enabled = false;
                        playerTransform.position = spawnPoint.position;
                        controller.enabled = true;
                    }
                    else
                    {
                        playerTransform.position = spawnPoint.position;
                    }

                    Vector3 delta = spawnPoint.position - oldPos;
                    CinemachineVirtualCamera vcam = cinemachineBrain.ActiveVirtualCamera as CinemachineVirtualCamera;
                    if (vcam != null)
                    {
                        vcam.OnTargetObjectWarped(playerTransform, delta);
                    }
                    else
                    {
                        Debug.LogWarning("未能获取 CinemachineVirtualCamera！");
                    }
                }
            }
            else
            {
                Debug.LogWarning("下一关没有找到生成点！");
            }

            // 重置本次关卡计时
            levelStartTime = Time.time;

            // 检查是否需要显示新机制介绍弹窗
            LevelData levelData = nextLevel.GetComponent<LevelData>();
            if (levelData != null &&
                levelData.introducesNewMechanism)
            {
                if (tutorialPopup != null)
                {
                    tutorialPopup.ShowPopup(levelData.mechanismTutorialMessage);
                    while (!tutorialPopup.IsClosed)
                    {
                        yield return null;
                    }
                }
            }
        }
        else
        {
            Debug.Log("所有关卡完成！");
            // 通关后显示每一关的统计数据
            ShowSummary();
            Time.timeScale = 0;
        }
    }

    /// 通关后调用，显示每一关的重置次数和用时
    private void ShowSummary()
    {
        string summary = "Game Summary:\n";
        for (int i = 0; i < levelStats.Length; i++)
        {
            summary += string.Format("Level {0}: Reset Count {1} , Time {2:F2} s\n",
                i + 1,
                levelStats[i].resetCount,
                levelStats[i].finalTime);
        }
        if (GameManager.Instance != null && GameManager.Instance.selectedCollectible == CollectibleState.HasCollectible)
        {
            summary += "\nCollectible Count: " + GameManager.Instance.collectedCount;
        }
        Debug.Log(summary);

        if (summaryCanvas != null)
        {
            summaryCanvas.SetActive(true);
            if (summaryText != null)
            {
                summaryText.text = summary;
            }
        }
    }

    public void generateCSVFile()
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filePath = Path.Combine(Application.persistentDataPath, "summary_"+ timestamp+".csv");
        StringBuilder csvContent = new StringBuilder();
        csvContent.AppendLine("Level, ResetCount, Time");

        for(int i = 0; i < levelStats.Length; i++)
        {
            csvContent.AppendLine(string.Format("{0},{1},{2:F2}", 
                i + 1, 
                levelStats[i].resetCount,
                levelStats[i].finalTime));
        }
        try
        {
            File.WriteAllText(filePath, csvContent.ToString());
            Debug.Log("Export csv file to: " + filePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error writing CSV file: " + e.Message);
        }
        ExitGame();
    }

    public void ExitGame()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }
}

[System.Serializable]
public class LevelStatistics {
    public int resetCount = 0;
    public float finalTime = 0f;
}