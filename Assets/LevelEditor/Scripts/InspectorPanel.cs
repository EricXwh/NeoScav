using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Reflection;
using System.Linq;

public class InspectorPanel : MonoBehaviour
{
    [Header("Name Display")]
    public TMP_Text nameText;
    [Header("Position Inputs")] 
    public TMP_InputField posXInput, posYInput, posZInput;
    [Header("Rotation Inputs")]
    public TMP_InputField rotXInput, rotYInput, rotZInput;
    [Header("Scale Inputs")]
    public TMP_InputField scaleXInput, scaleYInput, scaleZInput;

    public GameObject linksContainer;

    [Header("已链接 Mechanisms")] 
    public Transform linksContent;
    public GameObject linkItemTemplate;      

    [Header("候选目标 (Click to Link)")] 
    public Transform candidatesContent;
    public GameObject candidateItemTemplate;

    [Header("Custom Properties")]
    public GameObject propertyItemTemplate; 
    public Transform propertiesContent;
    public GameObject propertiesContainer;

    
    private GameObject current;
    private ILinkable currentLinkable;


    void Start()
    {
        LevelEditor.Instance.OnSelectionChanged += OnSelectionChanged;

        posXInput.onEndEdit.AddListener(_ => ApplyAll());
        posYInput.onEndEdit.AddListener(_ => ApplyAll());
        posZInput.onEndEdit.AddListener(_ => ApplyAll());
        rotXInput.onEndEdit.AddListener(_ => ApplyAll());
        rotYInput.onEndEdit.AddListener(_ => ApplyAll());
        rotZInput.onEndEdit.AddListener(_ => ApplyAll());
        scaleXInput.onEndEdit.AddListener(_ => ApplyAll());
        scaleYInput.onEndEdit.AddListener(_ => ApplyAll());
        scaleZInput.onEndEdit.AddListener(_ => ApplyAll());
        gameObject.SetActive(false);
    }

    private void OnSelectionChanged(GameObject go)
    {
        current = go;
        gameObject.SetActive(go != null);
        if (go == null) return;

        nameText.text = go.name;
        //Position
        var t = go.transform;
        var p = t.position;
        posXInput.SetTextWithoutNotify(p.x.ToString("F2"));
        posYInput.SetTextWithoutNotify(p.y.ToString("F2"));
        posZInput.SetTextWithoutNotify(p.z.ToString("F2"));

        //Rotation
        var r = t.eulerAngles;
        rotXInput.SetTextWithoutNotify(r.x.ToString("F1"));
        rotYInput.SetTextWithoutNotify(r.y.ToString("F1"));
        rotZInput.SetTextWithoutNotify(r.z.ToString("F1"));

        //Scale
        var s = t.localScale;
        scaleXInput.SetTextWithoutNotify(s.x.ToString("F2"));
        scaleYInput.SetTextWithoutNotify(s.y.ToString("F2"));
        scaleZInput.SetTextWithoutNotify(s.z.ToString("F2"));
        bool hasCustomProps = RefreshProperties(go);
        propertiesContainer.SetActive(hasCustomProps);
        // Linkable 接口
        currentLinkable = go.GetComponent<ILinkable>();
        linksContainer.SetActive(currentLinkable != null);
        if (currentLinkable != null)
        {
            RefreshCandidates();
            RefreshLinks();
        }
        
        
    }

    private void ApplyAll()
    {
        if (current == null) return;
        if (float.TryParse(posXInput.text, out float x) &&
            float.TryParse(posYInput.text, out float y) &&
            float.TryParse(posZInput.text, out float z) &&
            float.TryParse(rotXInput.text, out float rx) &&
            float.TryParse(rotYInput.text, out float ry) &&
            float.TryParse(rotZInput.text, out float rz) &&
            float.TryParse(scaleXInput.text, out float sx) &&
            float.TryParse(scaleYInput.text, out float sy) &&
            float.TryParse(scaleZInput.text, out float sz))
        {
            var t = current.transform;
            t.position = new Vector3(x, y, z);
            t.eulerAngles = new Vector3(rx, ry, rz);
            t.localScale = new Vector3(sx, sy, sz);
        }
    }

    private void RefreshCandidates()
    {
        bool linkable = currentLinkable != null;
        candidatesContent.parent.gameObject.SetActive(linkable);
        if (!linkable) return;

        foreach (Transform c in candidatesContent) Destroy(c.gameObject);

        var all = FindObjectsOfType<MechanismBase>();
        // 排除自己、也排除已经链接过的
        var linked = currentLinkable.LinkedMechanisms ?? new MechanismBase[0];
        var cands = all.Where(m => m.gameObject != current && !linked.Contains(m));

        foreach (var mech in cands)
        {
            var go = Instantiate(candidateItemTemplate, candidatesContent);
            go.GetComponent<CandidateItem>()
              .Setup(mech, OnCandidateClicked);
            go.SetActive(true);
        }
    }

    private void OnCandidateClicked(MechanismBase mech)
    {
        // 将 mech 加入 LinkedMechanisms 数组
        var arr = currentLinkable.LinkedMechanisms ?? new MechanismBase[0];
        var newArr = new MechanismBase[arr.Length + 1];
        arr.CopyTo(newArr, 0);
        newArr[arr.Length] = mech;
        currentLinkable.LinkedMechanisms = newArr;

        RefreshCandidates();
        RefreshLinks();
    }

    private void RefreshLinks()
    {
        bool linkable = currentLinkable != null;
        linksContent.parent.gameObject.SetActive(linkable);
        if (!linkable) return;

        foreach (Transform c in linksContent) Destroy(c.gameObject);

        // 遍历并展示
        foreach (var mech in currentLinkable.LinkedMechanisms ?? new MechanismBase[0])
        {
            var go = Instantiate(linkItemTemplate, linksContent);
            var txt = go.GetComponent<TMP_Text>();
            if (txt != null) txt.text = mech.name;
            go.SetActive(true);
        }
    }

    private bool RefreshProperties(GameObject go)
    {
        // 清空旧项
        foreach (Transform c in propertiesContent)
            Destroy(c.gameObject);

        bool found = false;
        // 遍历所有脚本组件
        var comps = go.GetComponents<MonoBehaviour>();
        foreach (var comp in comps)
        {
            var type = comp.GetType();
            if (type == typeof(Selectable) || type == typeof(LevelEditor) ||
                type == typeof(InspectorPanel))
                continue;

            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var f in fields)
            {
                if (!f.IsPublic && f.GetCustomAttribute<SerializeField>() == null || f.GetCustomAttribute<HideInInspector>() != null)
                    continue;

                if (f.FieldType != typeof(float) &&
                    f.FieldType != typeof(int)   &&
                    f.FieldType != typeof(string))
                    continue;

                // 创建一个 PropertyItem
                var item = Instantiate(propertyItemTemplate, propertiesContent);
                item.GetComponent<PropertyItem>().Setup(comp, f);
                item.SetActive(true);
                found = true;
            }
        }

        return found;
    }

    private void OnDestroy()
    {
        if (LevelEditor.Instance != null)
            LevelEditor.Instance.OnSelectionChanged -= OnSelectionChanged;
    }
}
