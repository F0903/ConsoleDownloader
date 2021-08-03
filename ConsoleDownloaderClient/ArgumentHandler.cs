using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleDownloaderClient
{
    public static class ArgumentHandler
    {
        public delegate Task AsyncArgHandler(string argValue);

        public static AsyncArgHandler? DefaultHandler { private get; set; }
        public static IDictionary<char, AsyncArgHandler>? Handlers { private get; set; }

        const char ArgSpecifier = '-';

        public static Task HandleAsync(ReadOnlySpan<char> input)
        {
            if (Handlers == null)
                throw new NullReferenceException("Handlers were not initialized.");
            for (int i = 1; i < 2; i++)
            {
                if (input[i - 1] != ArgSpecifier)
                    continue;
                if (input[i + 1] != ' ')
                    continue;

                if (Handlers.TryGetValue(input[i], out var f))
                    return f(input[(i + 2)..].ToString());
            }
            return DefaultHandler?.Invoke(input.ToString()) ?? Task.CompletedTask;
        }
    }
}
