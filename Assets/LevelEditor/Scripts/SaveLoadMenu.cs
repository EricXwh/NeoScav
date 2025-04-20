using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveLoadMenu : MonoBehaviour
{
    [Header("切换按钮")]
    public Button saveTabButton;
    public Button loadTabButton;

    [Header("子面板")]
    public GameObject savePanel;
    public GameObject loadPanel;

    [Header("存档 UI")]
    public TMP_InputField nameInput;
    public Button        saveButton;
    public Button        saveCloseButton;

    [Header("读取 UI")]
    public Transform     listContent;
    public GameObject    saveItemTemplate;
    public Button        refreshButton;
    public Button        loadCloseButton;

    [Header("存读管理")]
    public LevelSaveLoad saveLoadManager;

    private void Awake()
    {
        saveTabButton.onClick.AddListener(ShowSavePanel);
        loadTabButton.onClick.AddListener(ShowLoadPanel);

        saveButton.onClick.AddListener(OnSaveClicked);
        saveCloseButton.onClick.AddListener(() => savePanel.SetActive(false));

        refreshButton.onClick.AddListener(RefreshList);
        loadCloseButton.onClick.AddListener(() => loadPanel.SetActive(false)); 
    }

    private void ShowSavePanel()
    {
        savePanel.SetActive(true);
        loadPanel.SetActive(false);
    }

    private void ShowLoadPanel()
    {
        savePanel.SetActive(false);
        loadPanel.SetActive(true);
        RefreshList();
    }

    private void OnSaveClicked()
    {
        string lvlName = nameInput.text.Trim();
        if (string.IsNullOrEmpty(lvlName)) return;
        saveLoadManager.SaveLevel(lvlName);
        nameInput.SetTextWithoutNotify("");
        savePanel.SetActive(false);
    }

    public void RefreshList()
    {
        foreach (Transform c in listContent) Destroy(c.gameObject);
        foreach (var lvl in saveLoadManager.GetSavedLevelNames())
        {
            var go = Instantiate(saveItemTemplate, listContent);
            var txt = go.GetComponentInChildren<TMP_Text>();
            txt.text = lvl;
            var btn = go.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => {
                saveLoadManager.LoadLevel(lvl);
                loadPanel.SetActive(false);
            });
        }
    }
}
