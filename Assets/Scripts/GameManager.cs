using UnityEngine;
using UnityEngine.SceneManagement;

public enum TutorialLevel
{
    None,
    Minimal,
    Full
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public TutorialLevel selectedTutorial = TutorialLevel.None;

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

    public void SetTutorialLevel(int level)
    {
        selectedTutorial = (TutorialLevel)level;
        Debug.Log("Selected Tutorial Level: " + selectedTutorial);
        SceneManager.LoadScene("MainGame"); 
    }
}
