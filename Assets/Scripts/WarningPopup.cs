using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WarningPopup
{
    private Action _closeCallback;
    private WarningPopupView _view;


    public WarningPopup(WarningPopupView view, IEnumerable<string> warnings, Action closeCallback)
    {
        _view = view;
        _view.Init(this);
        _closeCallback = closeCallback;
        foreach (string warning in warnings)
        {
            AddRecord(warning);
        }
    }

    public void OnOkButtonPressed()
    {
        Debug.Log("Ok Button Pressed");
        GameObject.Destroy(_view.gameObject);
        _closeCallback?.Invoke();
    }

    private void AddRecord(string warning)
    {
        if (!string.IsNullOrWhiteSpace(warning) && _view.RecordPrefab)
        {
            GameObject record = GameObject.Instantiate(_view.RecordPrefab, _view.RecordsRoot);
            TMP_Text text = record.GetComponentInChildren<TMP_Text>();
            if (text)
            {
                text.text = warning;
            }
            else
            {
                GameObject.Destroy(record);
            }
        }
    }
}
