using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ConsoleDownloaderClient.CommandFramework;
public static class AsyncCommandRegistry
{
    static AsyncCommandRegistry()
    {
        var cmds = GetDefaultCommands();
        commandGroups = cmds;
    }

    static readonly List<CommandGroup> commandGroups;

    static List<CommandGroup> GetDefaultCommands()
    {
        var asm = Assembly.GetCallingAssembly();
        var groups = new List<CommandGroup>(GetCommandGroups(asm));
        FillCommandGroups(groups);
        return groups;
    }

    static IEnumerable<CommandGroup> GetCommandGroups(Assembly asm)
    {
        foreach (var type in asm.GetTypes())
        {
            if (!type.IsClass) continue;

            bool hasGroupCommandAttr = false;
            foreach (var typeAttr in type.CustomAttributes)
            {
                if (typeAttr.AttributeType != typeof(CommandGroupAttribute))
                    continue;
                hasGroupCommandAttr = true;
                break;
            }

            if (hasGroupCommandAttr) yield return new CommandGroup(type, []);
        }
    }

    static T GetAttributeArgumentValue<T>(CustomAttributeTypedArgument arg)
    {
        var argVal = arg.Value;
        if (argVal is ReadOnlyCollection<CustomAttributeTypedArgument> arrayArgs) // Arg is array
        {
            var elemType = arrayArgs[0].ArgumentType;
            var values = Array.CreateInstance(elemType, arrayArgs.Count);
            for (int i = 0; i < values.Length; i++)
            {
                var val = GetAttributeArgumentValue<object>(arrayArgs[i]);
                values.SetValue(val, i);
            }
            return (T)(object)values; // This is a little sneaky
        }
        else // Arg is single value
        {
            return (T)argVal!;
        }
    }

    static void FillCommandGroups(IEnumerable<CommandGroup> commandGroups)
    {
        foreach (var commandGroup in commandGroups)
        {
            var groupType = commandGroup.InnerType;
            var commands = commandGroup.Commands;
            foreach (var method in groupType.GetMethods())
            {
                CommandAttributeValues? commandAttr = null;
                foreach (var attr in method.CustomAttributes)
                {
                    if (attr.AttributeType != typeof(AsyncCommandAttribute)) continue;
                    var attrArgs = attr.ConstructorArguments;
                    var name = GetAttributeArgumentValue<string>(attrArgs[0]) ?? throw new NullReferenceException("Command name was null!");
                    var description = GetAttributeArgumentValue<string>(attrArgs[1]) ?? throw new NullReferenceException("Command description was null!");
                    var aliases = GetAttributeArgumentValue<string[]?>(attrArgs[2]);
                    var switchDescriptions = GetAttributeArgumentValue<string[]?>(attrArgs[3]);
                    commandAttr = new(name, description, aliases, switchDescriptions);
                    break;
                }
                if (commandAttr is null) continue;
                var command = new Command(method, commandAttr);
                commands.Add(command);
            }
        }
    }

    public static Task ExecuteCommandAsync(string name, string[] args)
    {
        // It would be optimal to use a hashmap with the names and aliases or something like that
        // but the amount of commands is not expected to be large enough for this to matter.
        foreach (var commandGroup in commandGroups)
        {
            foreach (var command in commandGroup.Commands)
            {
                var cmdAttrValues = command.AttributeValues;
                bool isNotName = cmdAttrValues.Name != name;
                bool isNotAlias = !(cmdAttrValues.Aliases is not null && cmdAttrValues.Aliases.Any(x => x == name));
                if (isNotName && isNotAlias) 
                    continue;
                return command.ExecuteAsync(args);
            }
        }
        throw new NullReferenceException("Command not found!");
    }

    public static IEnumerable<Command> GetCommands() => commandGroups.SelectMany(x => x.Commands.Select(x => x));
}