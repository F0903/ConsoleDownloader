using System;

namespace ConsoleDownloaderClient;
internal static class UtilExtensions
{
    public static string FlattenStrings(this string[] strings, string? seperator = null)
    {
        var totalLen = 0;
        for (var i = 0; i < strings.Length; i++)
        {
            var str = strings[i];
            totalLen += str.Length;
            if (seperator is not null && i != strings.Length - 1)
                totalLen += seperator.Length;
        }

        return string.Create(totalLen, (strings, seperator), static (charBuf, state) =>
        {
            var (strings, seperator) = state;
            var totalCopied = 0;
            for (int i = 0; i < strings.Length; i++)
            {
                var str = strings[i].AsSpan();
                var strLen = str.Length;
                str.CopyTo(charBuf[totalCopied..]);
                totalCopied += strLen;
                if (seperator is not null && i != strings.Length - 1)
                {
                    seperator.AsSpan().CopyTo(charBuf[totalCopied..]);
                    totalCopied += seperator.Length;
                }
            }
        });
    }
}
