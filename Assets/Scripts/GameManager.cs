using UnityEngine;
using UnityEngine.SceneManagement;
using MaskTransitions;

public enum CollectibleState
{
    None,       // 无收集物
    HasCollectible  // 有收集物
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public CollectibleState selectedCollectible = CollectibleState.None;
    public int collectedCount = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetCollectibleOption(int option)
    {
        selectedCollectible = (CollectibleState)option;
        Debug.Log("Selected Collectible Option: " + selectedCollectible);
        TransitionManager.Instance.LoadLevel("MainGame");
        //SceneManager.LoadScene("MainGame");
    }
}
