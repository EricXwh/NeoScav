using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Reflection;

public class PropertyItem : MonoBehaviour
{
    public TMP_Text   label;
    public TMP_InputField input;

    private object      targetComponent;
    private FieldInfo   field;

    // 用反射把 targetComponent.field 绑定到这个 UI 上
    public void Setup(object comp, FieldInfo fi)
    {
        targetComponent = comp;
        field = fi;
        label.text = fi.Name;

        // 读当前值
        object val = fi.GetValue(comp);
        input.SetTextWithoutNotify(val?.ToString());

        // 编辑结束，写回字段
        input.onEndEdit.AddListener( str =>
        {
            Type t = fi.FieldType;
            try
            {
                if (t == typeof(float) && float.TryParse(str, out var fv))
                    fi.SetValue(comp, fv);
                else if (t == typeof(int) && int.TryParse(str, out var iv))
                    fi.SetValue(comp, iv);
                else if (t == typeof(string))
                    fi.SetValue(comp, str);
            }
            catch(Exception e)
            {
                Debug.LogError($"字段赋值失败：{fi.Name} ← {str}\n{e}");
            }
        });
    }
}
