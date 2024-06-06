using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleDownloaderClient;
public static class AsyncCommandHandler
{
    static readonly Dictionary<string, Command> commands = [];

    public static void RegisterCommand(Command command)
    {
        commands.Add(command.Name, command);
    }

    public static void RegisterCommands(params Command[] commands)
    {
        foreach (var command in commands)
        {
            RegisterCommand(command);
        }
    }

    public static Task ExecuteCommandAsync(string name, string[] args)
    {
        if (!commands.TryGetValue(name, out var command))
            throw new NullReferenceException("Command not found!");
        return command.Fn(args);
    }

    public static IReadOnlyDictionary<string, Command> GetCommands() => commands;
}

public record Command(string Name, string Description, Func<string[], Task> Fn);