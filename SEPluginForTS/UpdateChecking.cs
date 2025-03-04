using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using SharedPluginClasses;

namespace SEPluginForTS;

partial class Plugin
{
    async Task CheckForUpdateAsync()
    {
        WriteConsoleAndLog(LogLevel.LogLevel_INFO, "Checking for latest version.");

        using var client = new HttpClient();

        client.DefaultRequestHeaders.Add("User-Agent", $"SE-TS-Bridge/{currentVersion}");

        try
        {
            var response = await client.GetAsync("https://api.github.com/repos/mmusu3/SE-TS-Bridge/releases/latest");

            if (!response.IsSuccessStatusCode)
            {
                WriteConsoleAndLog(LogLevel.LogLevel_WARNING, $"GitHub request error. StatusCode: {response.StatusCode}, ReasonPhrase: {response.ReasonPhrase}");
                return;
            }

            var responseBody = await response.Content.ReadAsStringAsync();

            ParseReleaseInfo(responseBody, out var latestVersion, out var releaseUrl);

            if (latestVersion is not { } v || releaseUrl == null)
            {
                WriteConsoleAndLog(LogLevel.LogLevel_WARNING, "Failed to fetch latest plugin version.");
                return;
            }

            if (v > currentVersion)
            {
                WriteConsoleAndLog(LogLevel.LogLevel_INFO, $"New version available: {v}");
                PrintOrQueueMessageForCurrentTab($"[SE-TS Bridge] - [url={releaseUrl}]New version {v} is available.[/url]");
            }
            else
            {
                WriteConsoleAndLog(LogLevel.LogLevel_INFO, $"Current version is latest.");
            }
        }
        catch (HttpRequestException ex)
        {
            WriteConsoleAndLog(LogLevel.LogLevel_WARNING, $"GitHub request error: {ex.Message}");
        }
    }

    static void ParseReleaseInfo(string releaseInfo, out PluginVersion? version, out string? url)
    {
        version = null;
        url = null;

        var doc = JsonDocument.Parse(releaseInfo);

        if (doc.RootElement.TryGetProperty("tag_name", out var tagProp))
        {
            var tagName = tagProp.GetString();

            if (tagName != null)
                version = ParseReleaseVersion(tagName);
        }

        if (doc.RootElement.TryGetProperty("html_url", out var urlProp))
            url = urlProp.GetString();
    }

    static PluginVersion? ParseReleaseVersion(string tagName)
    {
        if (!tagName.StartsWith('v'))
            return null;

        var versionStr = tagName.AsSpan(1);

        int i = 0;
        uint minor = 0, patch = 0;

        foreach (var range in versionStr.Split('.'))
        {
            var part = versionStr[range];

            if (!uint.TryParse(part, out uint num))
                return null;

            switch (i)
            {
            case 0:
                if (num != 1)
                    return null;
                break;
            case 1:
                minor = num;
                break;
            case 2:
                patch = num;
                break;
            default:
                return null;
            }

            i++;
        }

        return new PluginVersion(minor, patch);
    }
}
