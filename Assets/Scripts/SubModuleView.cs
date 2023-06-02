using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SubModuleView : MonoBehaviour
{
    [SerializeField] private Toggle _toggle;
    [SerializeField] private TMP_Text _name;
    [SerializeField] private TMP_Text _version;

    private SubModule _subModule;


    public void Init(SubModule subModule)
    {
        _subModule = subModule;
        _toggle.onValueChanged.AddListener(_subModule.OnTogglePressed);
    }

    public void SetName(string name)
    {
        _name.text = name;
    }

    public void SetVersion(string version)
    {
        _version.text = version;
    }
}
