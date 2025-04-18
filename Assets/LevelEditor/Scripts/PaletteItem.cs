using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PaletteItem : MonoBehaviour
{
    public Image     iconImage;
    public TMP_Text  nameLabel;

    private MechanismType type;

    public void Setup(MechanismType mechanismType)
    {
        type = mechanismType;
        iconImage.sprite = type.icon;
        if (nameLabel != null)
            nameLabel.text = type.displayName;

        var btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        // 将整个 MechanismType 传给 LevelEditor
        LevelEditor.Instance.BeginPlacing(type);
    }
}
