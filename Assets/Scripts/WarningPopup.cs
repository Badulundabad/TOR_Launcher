using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WarningPopup
{
    private Action _closeCallback;
    private WarningPopupView _view;


    public WarningPopup(WarningPopupView view, Dictionary<string, Version> warnings, Action closeCallback)
    {
        _view = view;
        _view.Init(this);
        _closeCallback = closeCallback;
        foreach (KeyValuePair<string, Version> kvp in warnings)
        {
            AddRecord(kvp.Key, kvp.Value);
        }
    }

    public void OnOkButtonPressed()
    {
        Debug.Log("Ok Button Pressed");
        GameObject.Destroy(_view.gameObject);
        _closeCallback?.Invoke();
    }

    private void AddRecord(string module, Version version)
    {
        if (!string.IsNullOrWhiteSpace(module) && _view.RecordPrefab)
        {
            GameObject record = GameObject.Instantiate(_view.RecordPrefab, _view.RecordsRoot);
            TMP_Text text = record.GetComponentInChildren<TMP_Text>();
            if (text)
            {
                text.text = $"{module} v{version}";
            }
            else
            {
                GameObject.Destroy(record);
            }
        }
    }
}
