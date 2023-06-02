using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LauncherView : MonoBehaviour
{
    private LauncherService _service;
    [SerializeField] private GameObject _fader;
    [SerializeField] private GameObject _logMessagePrefab;
    [SerializeField] private ScrollRect _logScroller;
    [SerializeField] private ScrollRect _modulesScroller;
    [SerializeField] private Button _debugButton;
    [SerializeField] private TMP_Text _versionText;
    [SerializeField] private SubModuleView _subModulePrefab;
    [SerializeField] private WarningPopupView _warningPopupPrefab;

    public GameObject LogMessagePrefab => _logMessagePrefab;
    public ScrollRect LogScroller => _logScroller;
    public ScrollRect ModulesScroller => _modulesScroller;
    public SubModuleView ModulePrefab => _subModulePrefab;
    public WarningPopupView WarningPopupPrefab => _warningPopupPrefab;


    private void Awake()
    {
        _service = new LauncherService(this);
        _debugButton.onClick.AddListener(_service.OnDebugButtonClicked);
    }

    public void SetFaderActive(bool isActive)
    {
        _fader?.SetActive(isActive);
    }

    public void SetVersionText(string text)
    {
        if (_versionText != null)
        {
            _versionText.text = text;
        }
    }

    public void SetDebugButtonActive(bool isActive)
    {
        _debugButton?.gameObject.SetActive(isActive);
    }

    public void SetDebugPanelActive(bool isActive)
    {
        _logScroller!.gameObject.SetActive(isActive);
    }
}
