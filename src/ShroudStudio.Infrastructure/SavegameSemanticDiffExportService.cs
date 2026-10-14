using System.Text.Json;
using ShroudStudio.Core;

namespace ShroudStudio.Infrastructure;

public sealed class SavegameSemanticDiffExportService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public SavegameSemanticDiff WriteCharacterDiff(
        string beforeSavegameDirectory,
        CharacterSavegameOption beforeCharacter,
        string afterSavegameDirectory,
        CharacterSavegameOption afterCharacter,
        string outputPath)
    {
        var fullOutputPath = Path.GetFullPath(outputPath);
        EnsureOutsideSavegame(fullOutputPath, beforeSavegameDirectory);
        EnsureOutsideSavegame(fullOutputPath, afterSavegameDirectory);

        var diff = new SavegameSemanticDiffService().Compare(
            beforeSavegameDirectory,
            beforeCharacter,
            afterSavegameDirectory,
            afterCharacter);
        var directory = Path.GetDirectoryName(fullOutputPath);
        if (string.IsNullOrWhiteSpace(directory))
            throw new InvalidOperationException("Der Exportpfad enthält kein gültiges Zielverzeichnis.");

        Directory.CreateDirectory(directory);
        File.WriteAllText(fullOutputPath, JsonSerializer.Serialize(diff, JsonOptions));
        return diff;
    }

    private static void EnsureOutsideSavegame(string outputPath, string savegameDirectory)
    {
        var directory = Path.GetFullPath(savegameDirectory)
            .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (outputPath.StartsWith(directory, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Der semantische Diff darf nicht in einem Savegame-Verzeichnis geschrieben werden.");
        }
    }
}
