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

        // 填充完 TextField 之后立即应用变换
        posXInput.onEndEdit.AddListener(_ => ApplyAll());
        posYInput.onEndEdit.AddListener(_ => ApplyAll());
        posZInput.onEndEdit.AddListener(_ => ApplyAll());
        rotXInput.onEndEdit.AddListener(_ => ApplyAll());
        rotYInput.onEndEdit.AddListener(_ => ApplyAll());
        rotZInput.onEndEdit.AddListener(_ => ApplyAll());
        scaleXInput.onEndEdit.AddListener(_ => ApplyAll());
        scaleYInput.onEndEdit.AddListener(_ => ApplyAll());
        scaleZInput.onEndEdit.AddListener(_ => ApplyAll());
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
            // 刷新两部分列表
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
        // 只在 Linkable 对象上显示
        bool linkable = currentLinkable != null;
        candidatesContent.parent.gameObject.SetActive(linkable);
        if (!linkable) return;

        // 清空旧候选
        foreach (Transform c in candidatesContent) Destroy(c.gameObject);

        // 场景里所有 MechanismBase
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

        // 立即刷新两侧列表
        RefreshCandidates();
        RefreshLinks();
    }

    private void RefreshLinks()
    {
        // 只在 Linkable 对象上显示
        bool linkable = currentLinkable != null;
        linksContent.parent.gameObject.SetActive(linkable);
        if (!linkable) return;

        // 清空旧链接
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

    /// <summary>
    /// 扫描所有 MonoBehaviour 脚本，找出可序列化字段，动态生成 PropertyItem
    /// 返回：是否找到了至少一个字段
    /// </summary>
    private bool RefreshProperties(GameObject go)
    {
        // 清空旧项
        foreach (Transform c in propertiesContent)
            Destroy(c.gameObject);

        bool found = false;
        // 遍历所有脚本组件（排除 Transform、Selectable、InspectorPanel 本身等）
        var comps = go.GetComponents<MonoBehaviour>();
        foreach (var comp in comps)
        {
            var type = comp.GetType();
            if (type == typeof(Selectable) || type == typeof(LevelEditor) ||
                type == typeof(InspectorPanel))
                continue;

            // 获取 public 字段 或 有 [SerializeField] 标记的私有字段
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var f in fields)
            {
                if (!f.IsPublic && f.GetCustomAttribute<SerializeField>() == null)
                    continue;

                // 仅支持简单类型：float/int/string
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
