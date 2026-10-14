using System.Diagnostics;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text.Json;
using ShroudStudio.Core;

namespace ShroudStudio.Infrastructure;

public sealed class EnshroudedInventoryScanner
{
    private const string SavegameHeader = "KSC1";

    public InventoryReport Collect(string installationPath, string savegamePath)
    {
        var errors = new List<string>();
        var processes = FindProcesses(errors);
        var emlPath = Path.Combine(installationPath, "eml.json");
        var emlConfigurationPath = File.Exists(emlPath) ? emlPath : null;
        var savegames = InspectSavegames(savegamePath, errors);

        if (!Directory.Exists(installationPath))
        {
            errors.Add($"Installation directory was not found: {installationPath}");
        }

        return new InventoryReport(
            DateTimeOffset.UtcNow,
            Directory.Exists(installationPath) ? installationPath : null,
            emlConfigurationPath,
            processes,
            savegames,
            errors);
    }

    public static bool HasKscHeader(ReadOnlySpan<byte> bytes) =>
        bytes.Length >= SavegameHeader.Length &&
        bytes[..SavegameHeader.Length].SequenceEqual("KSC1"u8);

    private static IReadOnlyList<ProcessObservation> FindProcesses(List<string> errors)
    {
        var observations = new List<ProcessObservation>();
        foreach (var process in Process.GetProcessesByName("enshrouded"))
        {
            try
            {
                observations.Add(new ProcessObservation(
                    process.ProcessName,
                    process.Id,
                    TryReadPath(process),
                    TryReadStartTime(process),
                    process.Responding,
                    EvidenceStatus.Verified));
            }
            catch (InvalidOperationException ex)
            {
                errors.Add($"Process changed while it was inspected ({process.Id}): {ex.Message}");
            }
            finally
            {
                process.Dispose();
            }
        }

        return observations;
    }

    private static IReadOnlyList<SavegameObservation> InspectSavegames(
        string savegamePath,
        List<string> errors)
    {
        if (!Directory.Exists(savegamePath))
        {
            errors.Add($"Savegame directory was not found: {savegamePath}");
            return [];
        }

        var observations = new List<SavegameObservation>();
        foreach (var file in Directory.EnumerateFiles(savegamePath).OrderBy(path => path))
        {
            try
            {
                observations.Add(InspectFile(file));
            }
            catch (IOException ex)
            {
                errors.Add($"Could not inspect {Path.GetFileName(file)}: {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                errors.Add($"Access denied for {Path.GetFileName(file)}: {ex.Message}");
            }
            catch (JsonException ex)
            {
                errors.Add($"Invalid JSON index {Path.GetFileName(file)}: {ex.Message}");
            }
        }

        return observations;
    }

    private static SavegameObservation InspectFile(string path)
    {
        var info = new FileInfo(path);
        var headerBytes = new byte[4];
        using (var stream = File.OpenRead(path))
        {
            _ = stream.Read(headerBytes, 0, headerBytes.Length);
        }

        var header = HasKscHeader(headerBytes) ? SavegameHeader : "unknown";
        string? latest = null;
        long? unixTime = null;
        var deleted = false;

        if (path.EndsWith("-index", StringComparison.OrdinalIgnoreCase))
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            var root = document.RootElement;
            latest = root.TryGetProperty("latest", out var latestElement)
                ? latestElement.ToString()
                : null;
            unixTime = root.TryGetProperty("time", out var timeElement) &&
                       timeElement.TryGetInt64(out var parsedTime)
                ? parsedTime
                : null;
            deleted = root.TryGetProperty("deleted", out var deletedElement) &&
                      deletedElement.ValueKind == JsonValueKind.True;
        }

        var sha256 = info.Length <= 20 * 1024 * 1024
            ? Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))
            : null;

        return new SavegameObservation(
            info.Name,
            info.Length,
            header,
            latest,
            unixTime,
            deleted,
            sha256,
            header == SavegameHeader || path.EndsWith("-index", StringComparison.OrdinalIgnoreCase)
                ? EvidenceStatus.Detected
                : EvidenceStatus.Unknown);
    }

    private static string? TryReadPath(Process process)
    {
        try
        {
            return process.MainModule?.FileName;
        }
        catch (Win32Exception)
        {
            return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private static DateTime? TryReadStartTime(Process process)
    {
        try
        {
            return process.StartTime;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
        catch (Win32Exception)
        {
            return null;
        }
    }
}
