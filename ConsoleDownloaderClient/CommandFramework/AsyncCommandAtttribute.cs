using System;

namespace ConsoleDownloaderClient.CommandFramework;
[AttributeUsage(AttributeTargets.Method)]
public class AsyncCommandAttribute(string Name, string Description, string[]? Aliases = null, string[]? SwitchDescriptions = null) : Attribute
{
    public string Name { get; } = Name;
    public string[]? Aliases { get; } = Aliases;
    public string Description { get; } = Description;

    public string[]? SwitchDescriptions { get; } = SwitchDescriptions;
}