using System;

public class SubModule : SubModuleBase
{
    private SubModuleView _view;

    public string Name { get; private set; }
    public SubModuleBase[] DependedModules { get; private set; }


    public SubModule(string id, string name, bool isOptional, Version version, SubModuleBase[] dependedModules) : base(id, isOptional, version)
    {
        Name = name;
        DependedModules = dependedModules;
    }

    public void SetView(SubModuleView view)
    {
        _view = view;
        view.Init(this);
        view.SetName(Name);
        view.SetVersion(Version.ToString());
    }

    public void OnTogglePressed(bool isOn)
    {

    }
}
