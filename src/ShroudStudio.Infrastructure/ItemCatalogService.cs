using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using ShroudStudio.Core;

namespace ShroudStudio.Infrastructure;

/// <summary>
/// Performs bounded, read-only discovery of item descriptors in a local installation.
/// Binary KFC data is intentionally not guessed: only validated descriptor properties
/// from parseable static data receive a player-facing name.
/// </summary>
public sealed class ItemCatalogService
{
    private const long MaxFileSize = 128 * 1024 * 1024;
    private static readonly string[] AllowedExtensions = [".kfc", ".kfc_resources", ".exe", ".json", ".xml", ".bin"];
    private static readonly Regex Numeric = new(@"^\d+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly string[] ReferenceKeys =
        ["id", "itemId", "itemID", "item_id", "reference", "itemReference", "prefabId"];
    private static readonly string[] NameKeys =
        ["displayName", "display_name", "localizedName", "name_text", "debug_name", "internal_name", "name"];
    private static readonly string[] CategoryKeys = ["category", "type", "itemType"];
    private static readonly string[] RarityKeys = ["rarity"];
    private static readonly string[] SlotKeys = ["equipment_slot", "equipmentSlot", "slot"];
    private static readonly string[] DescriptionKeys = ["description_text", "description", "caption_text"];
    private static readonly Regex Uuid = new(
        @"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    public ItemCatalogReport Discover(string installationPath)
    {
        var fullPath = Path.GetFullPath(installationPath);
        if (!Directory.Exists(fullPath))
            return new(fullPath, [], [], ["Enshrouded-Installation nicht gefunden."], EvidenceStatus.Unknown);

        var files = new List<string>();
        var definitions = new List<ItemDefinition>();
        var errors = new List<string>();
        var localizations = ReadLocalizationMap(fullPath, errors);
        foreach (var file in EnumerateFiles(fullPath))
        {
            files.Add(file);
            try
            {
                var info = new FileInfo(file);
                if (info.Length > MaxFileSize) continue;
                if (IsTextDescriptor(file))
                    ReadJsonDefinitions(file, definitions, localizations);
                // KFC/resource/EXE are recorded as inspected, but never heuristically decoded.
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or XmlException)
            {
                errors.Add($"{Path.GetFileName(file)}: {ex.Message}");
            }
        }

        var unique = definitions
            .GroupBy(definition => definition.Reference, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.OrderByDescending(definition => definition.DisplayName is not null).First())
            .ToArray();
        return new(fullPath, unique, files, errors,
            unique.Length > 0 ? EvidenceStatus.Verified :
            files.Count > 0 ? EvidenceStatus.Detected : EvidenceStatus.Unknown);
    }

    public ItemResolution Resolve(string? input, ItemCatalogReport catalog)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new(input ?? string.Empty, null, EvidenceStatus.Unknown, "Keine Item-Referenz vorhanden.");
        var normalized = Normalize(input);
        var definition = catalog.Definitions.FirstOrDefault(item =>
            string.Equals(Normalize(item.Reference), normalized, StringComparison.OrdinalIgnoreCase) ||
            item.NumericId?.ToString(CultureInfo.InvariantCulture) == normalized);
        return definition is null
            ? new(input, null, catalog.Status == EvidenceStatus.Unknown ? EvidenceStatus.Unknown : EvidenceStatus.Detected,
                "Keine validierte lokale ItemDefinition gefunden. Name nicht aufgelöst.")
            : new(input, definition, definition.EvidenceStatus, definition.Evidence);
    }

    public ItemResolution Resolve(ItemCandidate candidate, ItemCatalogReport catalog) =>
        Resolve(candidate.IdOrReference, catalog);

    public ItemResolution Resolve(ItemEntity entity, ItemCatalogReport catalog) =>
        Resolve(entity.ItemId ?? entity.Reference, catalog);

    private static IEnumerable<string> EnumerateFiles(string root)
    {
        var pending = new Stack<string>();
        pending.Push(root);
        while (pending.Count > 0)
        {
            var directory = pending.Pop();
            IEnumerable<string> files;
            try { files = Directory.EnumerateFiles(directory); }
            catch (UnauthorizedAccessException) { continue; }
            foreach (var file in files)
                if (AllowedExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
                    yield return file;
            IEnumerable<string> children;
            try { children = Directory.EnumerateDirectories(directory); }
            catch (UnauthorizedAccessException) { continue; }
            foreach (var child in children) pending.Push(child);
        }
    }

    private static bool IsTextDescriptor(string file) =>
        string.Equals(Path.GetExtension(file), ".json", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(Path.GetExtension(file), ".xml", StringComparison.OrdinalIgnoreCase);

    private static void ReadJsonDefinitions(
        string path,
        List<ItemDefinition> output,
        IReadOnlyDictionary<string, string> localizations)
    {
        if (string.Equals(Path.GetExtension(path), ".xml", StringComparison.OrdinalIgnoreCase))
        {
            ReadXmlDefinitions(path, output);
            return;
        }

        using var document = JsonDocument.Parse(File.ReadAllBytes(path));
        Visit(document.RootElement, path, output, localizations);
    }

    private static void ReadXmlDefinitions(string path, List<ItemDefinition> output)
    {
        var document = XDocument.Load(path, LoadOptions.PreserveWhitespace);
        foreach (var element in document.Descendants())
        {
            var reference = FindXmlValue(element, ReferenceKeys);
            var name = FindXmlValue(element, NameKeys);
            if (string.IsNullOrWhiteSpace(reference) || string.IsNullOrWhiteSpace(name))
                continue;

            long? numericId = long.TryParse(reference, NumberStyles.Integer,
                CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
            if (numericId is null && !Numeric.IsMatch(reference))
                continue;

            output.Add(new(reference, numericId, name.Trim(), path, EvidenceStatus.Verified,
                "Validierte XML-Descriptor-Eigenschaften (id/reference + name)."));
        }
    }

    private static string? FindXmlValue(XElement element, IReadOnlyList<string> keys)
    {
        foreach (var key in keys)
        {
            var attribute = element.Attributes()
                .FirstOrDefault(attribute => string.Equals(attribute.Name.LocalName, key,
                    StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(attribute?.Value))
                return attribute.Value;

            var child = element.Elements()
                .FirstOrDefault(child => string.Equals(child.Name.LocalName, key,
                    StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(child?.Value))
                return child.Value;
        }

        return null;
    }

    private static string? ScalarText(JsonElement value) =>
        value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => value.ToString(),
            _ => null
        };

    private static int? ScalarInt(JsonElement value) =>
        value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var parsed) ? parsed :
        int.TryParse(ScalarText(value), NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed)
            ? parsed : null;

    private static bool IsUsableName(string? name) =>
        !string.IsNullOrWhiteSpace(name) && !Uuid.IsMatch(name);

    private static IReadOnlyDictionary<string, string> ReadLocalizationMap(
        string installationPath,
        List<string> errors)
    {
        var path = Path.Combine(installationPath, "export", "localization_dump", "localization_map.json");
        if (!File.Exists(path))
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllBytes(path));
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            return document.RootElement.EnumerateObject()
                .Where(property => Uuid.IsMatch(property.Name) &&
                                   property.Value.ValueKind == JsonValueKind.String &&
                                   IsUsableName(property.Value.GetString()))
                .ToDictionary(
                    property => property.Name,
                    property => property.Value.GetString()!,
                    StringComparer.OrdinalIgnoreCase);
        }
        catch (JsonException ex)
        {
            errors.Add($"localization_map.json: {ex.Message}");
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
        catch (IOException ex)
        {
            errors.Add($"localization_map.json: {ex.Message}");
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static void Visit(
        JsonElement element,
        string path,
        List<ItemDefinition> output,
        IReadOnlyDictionary<string, string> localizations)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            string? reference = null;
            long? numericId = null;
            string? localizedName = null;
            string? internalName = null;
            string? category = null;
            string? rarity = null;
            string? equipmentSlot = null;
            string? description = null;
            var localizationKeys = new List<string>();
            int? minLevel = null;
            int? maxLevel = null;
            string? damageSummary = null;
            var buffs = new List<string>();
            foreach (var property in element.EnumerateObject())
            {
                if (ReferenceKeys.Contains(property.Name, StringComparer.OrdinalIgnoreCase))
                {
                    if (property.Value.ValueKind == JsonValueKind.Number && property.Value.TryGetInt64(out var id))
                    { numericId = id; reference = id.ToString(CultureInfo.InvariantCulture); }
                    else if (property.Value.ValueKind == JsonValueKind.String)
                    { reference = property.Value.GetString(); if (long.TryParse(reference, out var parsed)) numericId = parsed; }
                }
                else if (NameKeys.Contains(property.Name, StringComparer.OrdinalIgnoreCase))
                {
                    var candidateName = ScalarText(property.Value);
                    if (IsUsableName(candidateName))
                    {
                        if (property.Name is "displayName" or "display_name" or "localizedName" or "name_text")
                            localizedName ??= candidateName;
                        else if (property.Name is "debug_name" or "internal_name")
                            internalName ??= candidateName;
                        else
                            internalName ??= candidateName;
                    }

                    if (Uuid.IsMatch(candidateName ?? string.Empty))
                        localizationKeys.Add(candidateName!);
                }
                else if (property.Name.EndsWith("_key", StringComparison.OrdinalIgnoreCase) ||
                         property.Name.EndsWith("Key", StringComparison.OrdinalIgnoreCase))
                {
                    var localizationKey = ScalarText(property.Value);
                    if (Uuid.IsMatch(localizationKey ?? string.Empty))
                        localizationKeys.Add(localizationKey!);
                }
                else if (CategoryKeys.Contains(property.Name, StringComparer.OrdinalIgnoreCase))
                    category = ScalarText(property.Value);
                else if (RarityKeys.Contains(property.Name, StringComparer.OrdinalIgnoreCase))
                    rarity = ScalarText(property.Value);
                else if (SlotKeys.Contains(property.Name, StringComparer.OrdinalIgnoreCase))
                    equipmentSlot = ScalarText(property.Value);
                else if (DescriptionKeys.Contains(property.Name, StringComparer.OrdinalIgnoreCase))
                    description = ScalarText(property.Value);
                else if (string.Equals(property.Name, "level_min", StringComparison.OrdinalIgnoreCase))
                    minLevel = ScalarInt(property.Value);
                else if (string.Equals(property.Name, "level_max", StringComparison.OrdinalIgnoreCase))
                    maxLevel = ScalarInt(property.Value);
                else if (string.Equals(property.Name, "buffs", StringComparison.OrdinalIgnoreCase))
                    ReadBuffs(property.Value, buffs);
                else if (string.Equals(property.Name, "values", StringComparison.OrdinalIgnoreCase))
                    damageSummary = ReadDamageSummary(property.Value);
            }
            localizedName ??= localizationKeys
                .Select(key => localizations.GetValueOrDefault(key))
                .FirstOrDefault(IsUsableName);
            var name = localizedName ?? internalName;
            category ??= InferCategory(name);
            rarity ??= InferRarity(name);
            equipmentSlot ??= InferEquipmentSlot(name);
            if (!string.IsNullOrWhiteSpace(reference) && !string.IsNullOrWhiteSpace(name) &&
                (numericId is not null || Numeric.IsMatch(reference)))
                output.Add(new(reference, numericId, name.Trim(), path, EvidenceStatus.Verified,
                    "Validierte lokale Spieldaten (Item-ID + Name).",
                    category, rarity, minLevel, maxLevel, equipmentSlot, description,
                    damageSummary, buffs.Count == 0 ? null : buffs.ToArray()));
            foreach (var property in element.EnumerateObject()) Visit(property.Value, path, output, localizations);
        }
        else if (element.ValueKind == JsonValueKind.Array)
            foreach (var child in element.EnumerateArray()) Visit(child, path, output, localizations);
    }

    private static void ReadBuffs(JsonElement value, List<string> output)
    {
        if (value.ValueKind != JsonValueKind.Object)
            return;

        foreach (var property in value.EnumerateObject())
        {
            if (property.Name is "applied_buff" or "applied_debuff")
            {
                var text = ScalarText(property.Value);
                if (!string.IsNullOrWhiteSpace(text))
                    output.Add(text);
            }
            else if (property.Name.Equals("perk_references", StringComparison.OrdinalIgnoreCase) &&
                     property.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var reference in property.Value.EnumerateArray())
                {
                    var text = ScalarText(reference);
                    if (!string.IsNullOrWhiteSpace(text))
                        output.Add($"Perk-Referenz {text}");
                }
            }
        }
    }

    private static string? ReadDamageSummary(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Object ||
            !value.TryGetProperty("damage", out var damage) ||
            damage.ValueKind != JsonValueKind.Object ||
            !damage.TryGetProperty("distribution", out var distribution) ||
            distribution.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var values = distribution.EnumerateObject()
            .Select(property => (Name: property.Name, Value: ScalarText(property.Value)))
            .Where(item => double.TryParse(item.Value, NumberStyles.Float,
                CultureInfo.InvariantCulture, out var number) && number != 0)
            .Select(item => $"{item.Name}: {item.Value}")
            .ToArray();
        return values.Length == 0 ? null : string.Join(", ", values);
    }

    private static string Normalize(string value) => value.Trim().Trim('"');

    private static string? InferCategory(string? name)
    {
        if (name is null)
            return null;
        if (name.StartsWith("Armor_", StringComparison.OrdinalIgnoreCase))
            return "Rüstung";
        if (name.StartsWith("Weapon_", StringComparison.OrdinalIgnoreCase))
            return "Waffe";
        if (name.StartsWith("Food_", StringComparison.OrdinalIgnoreCase))
            return "Nahrung";
        if (name.StartsWith("Potion_", StringComparison.OrdinalIgnoreCase))
            return "Trank";
        if (name.StartsWith("Ammo_", StringComparison.OrdinalIgnoreCase))
            return "Munition";
        return null;
    }

    private static string? InferRarity(string? name)
    {
        if (name is null)
            return null;
        foreach (var rarity in new[] { "Legendary", "Epic", "Rare", "Uncommon", "Common" })
        {
            if (name.Contains(rarity, StringComparison.OrdinalIgnoreCase))
                return rarity;
        }

        return null;
    }

    private static string? InferEquipmentSlot(string? name)
    {
        if (name is null)
            return null;
        if (name.Contains("Boots", StringComparison.OrdinalIgnoreCase))
            return "Stiefel";
        if (name.Contains("Chest", StringComparison.OrdinalIgnoreCase))
            return "Brust";
        if (name.Contains("Arms", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Gloves", StringComparison.OrdinalIgnoreCase))
            return "Handschuhe";
        if (name.Contains("Head", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Helmet", StringComparison.OrdinalIgnoreCase))
            return "Kopf";
        if (name.Contains("Shield", StringComparison.OrdinalIgnoreCase))
            return "Nebenhand";
        if (name.Contains("Bow", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Staff", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Wand", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Sword", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Axe", StringComparison.OrdinalIgnoreCase))
            return "Haupthand";
        return null;
    }
}
