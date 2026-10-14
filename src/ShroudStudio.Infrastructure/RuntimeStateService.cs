using System.Diagnostics;
using ShroudStudio.Core;

namespace ShroudStudio.Infrastructure;

public sealed class RuntimeStateService
{
    public RuntimeState Detect(string installationPath, string savegamePath)
    {
        var processRunning = Process.GetProcessesByName("enshrouded").Length > 0;
        var selectedSavegame = IsSelectedSavegame(savegamePath);

        return new RuntimeState(
            processRunning,
            selectedSavegame,
            selectedSavegame ? Path.GetFullPath(savegamePath) : null,
            processRunning ? "Spielprozess erkannt" : "Spielprozess nicht erkannt",
            selectedSavegame ? "Savegame-Arbeitsbereich erkannt" : "Kein gültiger Savegame-Arbeitsbereich ausgewählt");
    }

    private static bool IsSelectedSavegame(string path)
    {
        if (!Directory.Exists(path))
        {
            return false;
        }

        var hasIndex = Directory.EnumerateFiles(path, "*-index", SearchOption.TopDirectoryOnly).Any();
        var hasContainer = Directory.EnumerateFiles(path, "*", SearchOption.TopDirectoryOnly)
            .Any(file => EnshroudedInventoryScanner.HasKscHeader(ReadHeader(file)));
        return hasIndex && hasContainer;
    }

    private static byte[] ReadHeader(string path)
    {
        using var stream = File.OpenRead(path);
        var header = new byte[4];
        var read = stream.Read(header, 0, header.Length);
        return read == header.Length ? header : [];
    }
}
