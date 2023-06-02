using System;

public class SubModuleBase
{
    public string Id { get; private set; }
    public bool IsOptional { get; private set; }
    public Version Version { get; private set; }


    public SubModuleBase(string id, bool isOptional, Version version)
    {
        Id = id;
        IsOptional = isOptional;
        Version = version;
    }
}
