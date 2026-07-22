using System;
using System.Text.RegularExpressions;

namespace EasySocialMediaPlugin;

public static partial class TwitterResolver
{
    private const string BaseUrl = "https://x.com/";

    public static bool TryResolve(string searchComment, out string url)
    {
        url = string.Empty;

        var match = HandleRegex().Match(searchComment);
        if (!match.Success)
        {
            return false;
        }

        var handle = match.Groups["handle"].Value;
        url = BaseUrl + Uri.EscapeDataString(handle);
        return true;
    }

    [GeneratedRegex(@"(?<![A-Za-z0-9_])@(?<handle>[A-Za-z0-9_]{1,15})(?![A-Za-z0-9_])")]
    private static partial Regex HandleRegex();
}
