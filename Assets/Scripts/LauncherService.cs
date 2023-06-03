using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using TMPro;
using UnityEngine;

public class LauncherService
{
    private const string MODULES_FOLDER_NAME = "Modules";
    private const string SUBMODULE_FILE_NAME = "SubModule.xml";

    private readonly string[] _requiredModuleIDs = new string[9]
    {
        "Bannerlord.Harmony",
        "Native",
        "SandBoxCore",
        "CustomBattle",
        "Sandbox",
        "StoryMode",
        "TOR_Core",
        "TOR_Armory",
        "TOR_Environment"
    };

    private bool _isDebugMode = true;
    private bool _isDebugPanelActive = false;
    private bool _isSteam;
    private LauncherView _view;
    private Version _gameVersion;
    private Version _requiredGameVersion = new Version(1, 0, 3);
    private DirectoryInfo _gameFolder;
    private DirectoryInfo _steamFolder;
    private Dictionary<string, SubModule> _modules;


    public LauncherService(LauncherView view)
    {
        _view = view;
        _view.SetDebugButtonActive(_isDebugMode);
        Init();
    }

    private void Init()
    {
        SearchGameFolder();
        CheckGameVersion();
        InitModules();
        CheckRequiredModules();
    }

    public void OnDebugButtonClicked()
    {
        _isDebugPanelActive = !_isDebugPanelActive;
        _view.SetDebugPanelActive(_isDebugPanelActive);
    }

    private void SearchGameFolder()
    {
        try
        {
            string launcherPath = Process.GetCurrentProcess().MainModule.FileName;
            DirectoryInfo folder = Directory.GetParent(launcherPath);

            _isSteam = ContainsSteam(folder, 10, out _steamFolder);
            if (_isSteam)
            {
                string gamePath = Path.Combine(_steamFolder.FullName, "steamapps", "common", "Mount & Blade II Bannerlord");
                if (Directory.Exists(gamePath))
                {
                    _gameFolder = new DirectoryInfo(gamePath);
                    Log($"[SearchGameFolder] game folder {_gameFolder.FullName}");
                }
                Log($"[SearchGameFolder] steam folder {_steamFolder.FullName}");
            }
            else
            {
                _gameFolder = folder.Parent.Parent;
                Log($"[SearchGameFolder] game folder {_gameFolder.FullName}");
            }
        }
        catch (Exception ex)
        {
            LogException(ex);
        }
    }

    private bool ContainsSteam(DirectoryInfo folder, int counts, out DirectoryInfo steamFolder)
    {
        counts--;
        if (counts < 0)
        {
            steamFolder = null;
            Log($"[ContainsSteam] attempts are ended");
            return false;
        }
        else
        {
            if (folder == null)
            {
                steamFolder = null;
                Log($"[ContainsSteam] folder is null");
                return false;
            }
            else if (folder.Name == "Steam")
            {
                bool containsGame = Directory.Exists(Path.Combine(folder.FullName, "steamapps", "common", "Mount & Blade II Bannerlord"));
                bool containsContent = Directory.Exists(Path.Combine(folder.FullName, "steamapps", "workshop", "content"));
                steamFolder = folder;
                Log($"[ContainsSteam] contains steam {folder.FullName} (game {containsGame} content {containsContent})");
                return true;
            }
            else
            {
                Log($"[ContainsSteam] doesn't contain steam {folder.FullName}");
                return ContainsSteam(folder.Parent, counts, out steamFolder);
            }
        }
    }

    private void CheckGameVersion()
    {
        string filePath = Path.Combine(_gameFolder.FullName, "bin", "Win64_Shipping_Client", "Version.xml");
        if (File.Exists(filePath))
        {
            try
            {
                Log($"[CheckGameVersion] version file exists {filePath}");
                StreamReader streamReader = new StreamReader(filePath);
                XmlDocument subModuleDoc = new XmlDocument();
                subModuleDoc.LoadXml(streamReader.ReadToEnd());
                streamReader.Close();

                XmlElement element = subModuleDoc.DocumentElement;
                string version = element.SelectSingleNode("Singleplayer").Attributes["Value"].InnerText;
                _gameVersion = GetVersion(version);
                _view.SetVersionText(version);
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }
        else
        {
            Log($"[CheckGameVersion] version file doesn't exist {filePath}");
        }
    }

    private void InitModules()
    {
        _modules = new Dictionary<string, SubModule>();

        if (_isSteam)
        {
            string contentPath = Path.Combine(_steamFolder.FullName, "steamapps", "workshop", "content");
            if (Directory.Exists(contentPath))
            {
                Log($"[InitModules] content folder exists {contentPath}");
                DirectoryInfo contentFolder = new DirectoryInfo(contentPath);
                foreach (DirectoryInfo folder in contentFolder.GetDirectories())
                {
                    CheckFolder(folder);
                }
            }
            else
            {
                Log($"[InitModules] content folder doesn't exist {contentPath}");
            }

            string modulesPath = Path.Combine(_gameFolder.FullName, "Modules");
            if (Directory.Exists(modulesPath))
            {
                Log($"[InitModules] modules folder exists {modulesPath}");
                DirectoryInfo modulesFolder = new DirectoryInfo(modulesPath);
                CheckFolder(modulesFolder);
            }
            else
            {
                Log($"[InitModules] modules folder doesn't exist {modulesPath}");
            }
        }
        else
        {
            string modulesPath = Path.Combine(_gameFolder.FullName, "Modules");
            if (Directory.Exists(modulesPath))
            {
                Log($"[InitModules] modules folder exists {modulesPath}");
                DirectoryInfo modulesFolder = new DirectoryInfo(modulesPath);
                CheckFolder(modulesFolder);
            }
            else
            {
                Log($"[InitModules] modules folder doesn't exist {modulesPath}");
            }
        }
    }

    private void CheckRequiredModules()
    {
        Dictionary<string, Version> missingModules = new Dictionary<string, Version>();
        foreach (string id in _requiredModuleIDs)
        {
            if (_modules.TryGetValue(id, out SubModule module))
            {
                foreach (SubModuleBase dependedModule in module.DependedModules)
                {
                    if (!_modules.TryGetValue(dependedModule.Id, out SubModule module2) || module2.Version != dependedModule.Version)
                    {
                        if (!missingModules.TryGetValue(id, out Version version))
                        {
                            missingModules.Add(id, dependedModule.Version);
                        }
                    }
                }
            }
            else
            {
                missingModules.Add(id, null);
            }
        }

        if (missingModules.Count > 0)
        {
            ShowWarningPopup(missingModules);
        }
    }

    private void CheckFolder(DirectoryInfo folder)
    {
        if (folder == null)
        {
            return;
        }

        DirectoryInfo[] modDirectories = folder.GetDirectories();
        foreach (DirectoryInfo directory in modDirectories)
        {
            Log($"[CheckFolder] {directory.FullName}");
            foreach (FileInfo file in directory.GetFiles())
            {
                if (file.Name == SUBMODULE_FILE_NAME && TryGetSubmodule(file, out SubModule module))
                {
                    _modules.Add(module.Id, module);
                    Log($"[CheckFolder] added {module.Id} sub module");
                    break;
                }
            }
        }
    }

    private bool TryGetSubmodule(FileInfo file, out SubModule module)
    {
        try
        {
            StreamReader streamReader = new StreamReader(file.FullName);
            XmlDocument subModuleDoc = new XmlDocument();
            subModuleDoc.LoadXml(streamReader.ReadToEnd());
            streamReader.Close();

            XmlElement element = subModuleDoc.DocumentElement;
            string id = element.SelectSingleNode("Id").Attributes["value"].InnerText;
            string name = element.SelectSingleNode("Name").Attributes["value"].InnerText;
            string versionText = element.SelectSingleNode("Version").Attributes["value"].InnerText;
            Version version = GetVersion(versionText);
            XmlNode node = element.SelectSingleNode("DependedModules");
            XmlNodeList dependedModuleNodes = node.ChildNodes;
            SubModuleBase[] dependedModules = new SubModuleBase[dependedModuleNodes.Count];
            for (int i = 0; i < dependedModules.Length; i++)
            {
                XmlNode childNode = dependedModuleNodes[i];
                dependedModules[i] = GetDependedModule(childNode);
            }

            module = new SubModule(id, name, false, version, dependedModules);
            SubModuleView view = GameObject.Instantiate(_view.ModulePrefab, _view.ModulesScroller.content);
            module.SetView(view);
            return true;
        }
        catch (Exception ex)
        {
            LogException(ex);
            module = null;
            return false;
        }
    }

    private SubModuleBase GetDependedModule(XmlNode node)
    {
        string id = node.Attributes["Id"].InnerText;

        bool isOptional = true;
        XmlAttribute isOptionalAtt = node.Attributes["Optional"];
        if (isOptionalAtt != null)
        {
            bool.TryParse(isOptionalAtt.InnerText, out isOptional);
        }

        Version version = null;
        XmlAttribute dependentVersionAtt = node.Attributes["DependentVersion"];
        if (dependentVersionAtt != null)
        {
            string versionText = dependentVersionAtt.InnerText;
            version = GetVersion(versionText);
        }

        return new SubModuleBase(id, isOptional, version);
    }

    private Version GetVersion(string versionText)
    {
        int[] numbers = new int[3];
        if (!string.IsNullOrWhiteSpace(versionText))
        {
            string[] strings = versionText.Remove(0, 1).Split('.');
            if (strings.Length >= 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (int.TryParse(strings[i], out int number))
                    {
                        numbers[i] = number;
                    }
                }
            }
        }
        return new Version(numbers[0], numbers[1], numbers[2]);
    }

    private void ShowWarningPopup(Dictionary<string, Version> missingModules)
    {
        if (_view.WarningPopupPrefab)
        {
            _view.SetFaderActive(true);
            WarningPopupView popupView = GameObject.Instantiate(_view.WarningPopupPrefab, _view.transform);
            WarningPopup warningPopup = new WarningPopup(popupView, missingModules, () =>
            {
                _view.SetFaderActive(false);
            });
        }
    }

    private void Log(string msg)
    {
        if (msg != null && _view.LogMessagePrefab && _view.LogScroller)
        {
            GameObject message = GameObject.Instantiate(_view.LogMessagePrefab, _view.LogScroller.content);
            TMP_Text text = message.GetComponentInChildren<TMP_Text>();
            if (text)
            {
                text.text = msg;
            }
        }
        UnityEngine.Debug.Log(msg);
    }

    private void LogException(Exception ex)
    {
        Log($"{ex.Message}\n{ex.StackTrace}");
    }
}
