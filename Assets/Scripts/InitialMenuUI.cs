using UnityEngine;
using UnityEngine.UI;

public class InitialMenuUI : MonoBehaviour
{
    [SerializeField] private Button noCollectibleButton;
    [SerializeField] private Button hasCollectibleButton;

    [SerializeField] private Button levelEditorButton;

    private void Start()
    {
        noCollectibleButton.onClick.AddListener(() =>
            GameManager.Instance.SetCollectibleOption((int)CollectibleState.None)
        );

        hasCollectibleButton.onClick.AddListener(() =>
            GameManager.Instance.SetCollectibleOption((int)CollectibleState.HasCollectible)
        );

        levelEditorButton.onClick.AddListener(GameManager.Instance.LevelEditorButton);
    }
}