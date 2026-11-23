using System.Diagnostics;

namespace Installer;

static class PluginInstaller
{
    const string SpaceEngineersPlugin = "TSPluginForSE.dll";
    const string TeamSpeakPlugin = "SEPluginForTS.dll";

    static readonly string[] ProcessNames = [
        "SpaceEngineers", "Legacy", "Interim", "ts3client_win64"
    ];

    internal static List<string> Install(string packageDirectory, string spaceEngineersDirectory, string teamSpeakDirectory)
    {
        var plan = Prepare(packageDirectory, spaceEngineersDirectory, teamSpeakDirectory);
        var runningPrograms = GetRunningPrograms();

        if (runningPrograms.Count == 0)
            plan.Install();

        return runningPrograms;
    }

    internal static InstallationPlan Prepare(string packageDirectory, string spaceEngineersDirectory, string teamSpeakDirectory)
    {
        string seDirectory = ValidateDirectory(spaceEngineersDirectory, "Space Engineers");
        string tsDirectory = ValidateDirectory(teamSpeakDirectory, "TeamSpeak");

        string seSource = Path.Combine(packageDirectory, SpaceEngineersPlugin);
        string tsSource = Path.Combine(packageDirectory, TeamSpeakPlugin);

        if (!File.Exists(seSource))
            throw new FileNotFoundException($"The Space Engineers plugin was not found next to the installer: {seSource}");

        if (!File.Exists(tsSource))
            throw new FileNotFoundException($"The TeamSpeak plugin was not found next to the installer: {tsSource}");

        if (File.Exists(Path.Combine(seDirectory, "Interim.exe")) || File.Exists(Path.Combine(seDirectory, "Legacy.exe")))
            seDirectory = Path.Combine(seDirectory, "Legacy", "Local");

        return new InstallationPlan(seSource, Path.Combine(seDirectory, SpaceEngineersPlugin),
            tsSource, Path.Combine(tsDirectory, TeamSpeakPlugin));
    }

    internal static List<string> GetRunningPrograms()
    {
        var runningPrograms = new List<string>();

        foreach (string processName in ProcessNames)
        {
            var processes = Process.GetProcessesByName(processName);

            foreach (var process in processes)
                process.Dispose();

            if (processes.Length != 0)
                runningPrograms.Add(processName + ".exe");
        }

        return runningPrograms;
    }

    static string ValidateDirectory(string path, string name)
    {
        path = path.Trim();

        if (!IsFullyQualified(path))
            throw new ArgumentException($"Enter a full path for the {name} folder.");

        path = Path.GetFullPath(path);

        if (File.Exists(path))
            throw new IOException($"The {name} folder path is an existing file: {path}");

        return path;
    }

    static bool IsFullyQualified(string path)
    {
        if (path.Length < 2)
            return false;

        if (IsDirectorySeparator(path[0]))
            return IsDirectorySeparator(path[1]) || path[1] == '?';

        return path.Length >= 3 && path[1] == ':' && IsDirectorySeparator(path[2])
            && ((path[0] >= 'A' && path[0] <= 'Z') || (path[0] >= 'a' && path[0] <= 'z'));
    }

    static bool IsDirectorySeparator(char character) => character == '\\' || character == '/';
}

sealed class InstallationPlan(string SpaceEngineersSource, string SpaceEngineersTarget, string TeamSpeakSource, string TeamSpeakTarget)
{
    public void Install()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SpaceEngineersTarget)!);
        Directory.CreateDirectory(Path.GetDirectoryName(TeamSpeakTarget)!);
        Copy(SpaceEngineersSource, SpaceEngineersTarget);
        Copy(TeamSpeakSource, TeamSpeakTarget);
    }

    static void Copy(string source, string target)
    {
        if (!string.Equals(Path.GetFullPath(source), Path.GetFullPath(target), StringComparison.OrdinalIgnoreCase))
            File.Copy(source, target, overwrite: true);
    }
}
