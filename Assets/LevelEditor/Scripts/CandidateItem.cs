using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CandidateItem : MonoBehaviour
{
    public Button button;
    public TMP_Text label;
    private MechanismBase mech;
    private System.Action<MechanismBase> onClick;

    public void Setup(MechanismBase m, System.Action<MechanismBase> callback)
    {
        mech = m;
        label.text = mech.name;
        onClick = callback;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick(mech));
    }
}
