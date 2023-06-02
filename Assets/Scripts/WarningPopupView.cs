using UnityEngine;
using UnityEngine.UI;

public class WarningPopupView : MonoBehaviour
{
    [SerializeField] private Button _okButton;
    [SerializeField] private Transform _recordsRoot;
    [SerializeField] private GameObject _recordPrefab;
    private WarningPopup _popup;

    public Transform RecordsRoot => _recordsRoot;
    public GameObject RecordPrefab => _recordPrefab;


    public void Init(WarningPopup popup)
    {
        _popup = popup;
        Debug.Log("Popup View Init");
        if (popup != null && _okButton)
        {
            Debug.Log("Ok Button");
            _okButton.onClick.AddListener(_popup.OnOkButtonPressed);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
