using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class LevelManager : MonoBehaviour
{
    [Header("所有关卡的容器（按关卡顺序排列）")]
    public GameObject[] levels;

    private int currentLevelIndex = 0;

    public Transform playerTransform;

    // 缓存每个关卡容器对应的生成点引用
    private Dictionary<GameObject, Transform> levelSpawnPoints = new Dictionary<GameObject, Transform>();

    public CinemachineBrain cinemachineBrain;

    private void Awake()
    {
        if (playerTransform == null)
        {
            Debug.LogError("请设置玩家的 Transform！");
        }

        // 如果没有手动赋值摄像机，则尝试自动查找
        if (cinemachineBrain == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                cinemachineBrain = mainCam.GetComponent<CinemachineBrain>();
            }
        }

        // 为每个关卡容器缓存生成点引用
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

    private void Start()
    {
        // 初始时只激活第一个关卡，其余关卡禁用
        for (int i = 0; i < levels.Length; i++)
        {
            levels[i].SetActive(i == currentLevelIndex);
        }
    }

    /// 切换到下一关，并将玩家移动到下一关的生成点位置，同时强制刷新摄像机状态。
    public void NextLevel()
    {
        // 禁用当前关卡
        if (currentLevelIndex < levels.Length)
        {
            levels[currentLevelIndex].SetActive(false);
        }

        currentLevelIndex++;

        if (currentLevelIndex < levels.Length)
        {
            GameObject nextLevel = levels[currentLevelIndex];
            nextLevel.SetActive(true);
            Debug.Log("切换到关卡：" + (currentLevelIndex + 1));

            if (levelSpawnPoints.TryGetValue(nextLevel, out Transform spawnPoint))
            {
                if (playerTransform != null)
                {
                    // 在传送前，记录玩家原来的位置
                    Vector3 oldPos = playerTransform.position;

                    CharacterController controller = playerTransform.GetComponent<CharacterController>();
                    if (controller != null)
                    {
                        // 临时禁用 CharacterController，以便直接设置位置
                        controller.enabled = false;
                        playerTransform.position = spawnPoint.position;
                        controller.enabled = true;
                    }
                    else
                    {
                        playerTransform.position = spawnPoint.position;
                    }

                    // 计算玩家瞬移的偏移量
                    Vector3 delta = spawnPoint.position - oldPos;

                    // 立即刷新虚拟摄像机的状态，使摄像机瞬间对齐玩家
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
        }
        else
        {
            Debug.Log("所有关卡完成！");
            // 在这里可以添加结算逻辑或返回主菜单等处理
        }
    }
}
