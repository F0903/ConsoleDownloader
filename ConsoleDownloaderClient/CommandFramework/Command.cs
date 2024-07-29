using System;
using System.Reflection;
using System.Threading.Tasks;

namespace ConsoleDownloaderClient.CommandFramework;

public record CommandAttributeValues(string Name, string Description, string[]? Aliases, string[]? SwitchDescriptions);

public record Command(MethodInfo InnerMethod, CommandAttributeValues AttributeValues)
{
    public Task ExecuteAsync(string[] args)
    {
        var containedType = InnerMethod.DeclaringType!;
        var instance = !containedType.IsAbstract ? Activator.CreateInstance(containedType) : null;
        return (Task)InnerMethod.Invoke(instance, [args])!;
    }
}