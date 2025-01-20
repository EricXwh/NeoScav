using UnityEngine;
using UnityEngine.UI; 
using TMPro;

public class TutorialLevelDisplay : MonoBehaviour
{

    public TMPro.TextMeshProUGUI tutorialText;

    void Start()
    {
        UpdateTutorialLevel();
    }

    public void UpdateTutorialLevel()
    {
        if (GameManager.Instance != null)
        {
            TutorialLevel currentLevel = GameManager.Instance.selectedTutorial;
            tutorialText.text = "Tutorial Level: " + currentLevel.ToString();
        }
        else
        {
            Debug.LogWarning("GameManager instance is not found.");
        }
    }
}
