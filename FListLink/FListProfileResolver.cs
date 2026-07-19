using System.Text.RegularExpressions;
using System;

namespace FListLink;

internal static partial class FListProfileResolver
{
    private const string ProfileBaseUrl = "https://f-list.net/c/";

    [GeneratedRegex(
        @"(?ix)(?<![a-z0-9/])(?:https?://(?:www\.)?f-list\.net/)?/?c/(?<name>\p{L}[\p{L}_\-]{0,79})(?![\p{L}\p{N}._~%'\-])",
        RegexOptions.CultureInvariant)]
    private static partial Regex ExplicitProfileRegex();

    [GeneratedRegex(
        @"(?ix)(?<![a-z0-9/])(?:(?:https?://(?:www\.)?f-list\.net/)?/?c/|/c)",
        RegexOptions.CultureInvariant)]
    private static partial Regex MarkerRegex();

    public static bool TryResolve(string searchComment, string playerName, out string url)
    {
        url = string.Empty;

        var explicitMatch = ExplicitProfileRegex().Match(searchComment);
        if (explicitMatch.Success)
        {
            var explicitName = DecodeOnce(explicitMatch.Groups["name"].Value);
            url = BuildUrl(explicitName);
            return true;
        }

        if (!MarkerRegex().IsMatch(searchComment))
        {
            return false;
        }

        url = BuildUrl(playerName);
        return true;
    }

    private static string BuildUrl(string profileName)
    {
        var normalized = Regex.Replace(profileName.Trim(), @"\s+", " ");
        return ProfileBaseUrl + Uri.EscapeDataString(normalized);
    }

    private static string DecodeOnce(string profileName)
    {
        try
        {
            return Uri.UnescapeDataString(profileName);
        }
        catch (UriFormatException)
        {
            return profileName;
        }
    }
}
