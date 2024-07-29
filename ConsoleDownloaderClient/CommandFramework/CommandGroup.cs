using System;
using System.Collections.Generic;

namespace ConsoleDownloaderClient.CommandFramework;
public record CommandGroup(Type InnerType, List<Command> Commands);