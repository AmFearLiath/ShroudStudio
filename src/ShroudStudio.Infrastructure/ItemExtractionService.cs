using System.Text.RegularExpressions;
using ShroudStudio.Core;

namespace ShroudStudio.Infrastructure;

/// <summary>
/// Read-only extraction of item-shaped objects from the nested BDB1 data graph.
/// No value is treated as an item unless its field names or component schema provide evidence.
/// </summary>
public sealed class ItemExtractionService
{
    private static readonly string[] Roots =
        ["items", "itemLastUsageOrders", "itemFavoritesOrders", "data", "setup"];
    private static readonly string[] Schemas = ["EHD0", "ESC2", "CTCB"];
    private static readonly Regex Number = new(@"\b(?:uint32|raw)\s+(\d+)\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex StringValue = new(@"^string\s+(.*)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public ItemExtractionReport Extract(string savegameDirectory, CharacterSavegameOption character)
    {
        var report = new CharacterPayloadService().Analyze(savegameDirectory);
        var blob = report.Characters.FirstOrDefault(item => item.BlobIndex == character.BlobIndex);
        if (blob is null)
        {
            return new(Path.GetFileName(report.ContainerFile), null, [], 
                ["Kein passender CHAR-Blob für den ausgewählten Charakter gefunden."],
                EvidenceStatus.Unknown);
        }

        var fields = blob.Fields;
        var rootFields = fields.Where(field => field.ParentNode == 1).ToArray();
        var candidates = new List<ItemCandidate>();
        var entities = new List<ItemEntity>();
        var containers = new List<ItemContainerObservation>();
        var unassigned = new List<string>();

        foreach (var root in rootFields.Where(field => Roots.Contains(field.Name, StringComparer.OrdinalIgnoreCase)))
        {
            var rootPath = root.Name;
            if (root.Name.Contains("item", StringComparison.OrdinalIgnoreCase))
            {
                containers.Add(new ItemContainerObservation(
                    root.Name,
                    ContainerDisplayName(root.Name),
                    rootPath,
                    root.ChildNode,
                    root.Value,
                    ToCandidateStatus(root.Status)));
            }

            if (root.Name.Contains("item", StringComparison.OrdinalIgnoreCase) &&
                ExtractValue(root.Value) is { } rootReference &&
                !rootReference.StartsWith("node:", StringComparison.OrdinalIgnoreCase))
            {
                candidates.Add(new ItemCandidate(
                    rootReference, null, rootPath, root.Value,
                    ToCandidateStatus(root.Status), ComponentFor(rootPath)));
                entities.Add(new ItemEntity(
                    null, rootReference, null, null, ComponentFor(rootPath), rootPath,
                    [$"{rootPath} (nested item fields not exposed by this BDB1 node type)"],
                    ToCandidateStatus(root.Status)));
            }

            if (root.Value.StartsWith("object ", StringComparison.OrdinalIgnoreCase))
            {
                WalkObject(root.ChildNode, rootPath, fields, candidates, entities, containers, unassigned);
            }
            else
            {
                unassigned.Add(rootPath);
            }
        }

        var status = candidates.Count > 0 || entities.Count > 0
            ? (unassigned.Count == 0 ? EvidenceStatus.Observed : EvidenceStatus.Detected)
            : (rootFields.Any(field => Roots.Contains(field.Name, StringComparer.OrdinalIgnoreCase))
                ? EvidenceStatus.Detected
                : EvidenceStatus.Unknown);
        return new(report.ContainerFile, blob.BlobIndex,
            candidates.Where(candidate => !candidate.IdOrReference.StartsWith("node:", StringComparison.OrdinalIgnoreCase)).ToArray(),
            unassigned, status, entities, containers);
    }

    private static void WalkObject(
        int parentNode,
        string parentPath,
        IReadOnlyList<CharacterFieldObservation> fields,
        List<ItemCandidate> candidates,
        List<ItemEntity> entities,
        List<ItemContainerObservation> containers,
        List<string> unassigned)
    {
        var children = fields.Where(field => field.ParentNode == parentNode).ToArray();
        var primitive = children.Where(field =>
            !field.Value.StartsWith("object ", StringComparison.OrdinalIgnoreCase)).ToArray();
        var component = ComponentFor(parentPath) ??
                        children.Select(field => ComponentFor(field.Name)).FirstOrDefault(value => value is not null);
        var itemId = primitive.FirstOrDefault(field => IsItemId(field.Name));
        var reference = primitive.FirstOrDefault(field => IsReference(field.Name));
        var quantity = primitive.FirstOrDefault(field => IsQuantity(field.Name));
        var slot = primitive.FirstOrDefault(field => IsSlot(field.Name));
        var unknown = primitive
            .Where(field => field.Name.StartsWith("hash:", StringComparison.OrdinalIgnoreCase))
            .Select(field => $"{parentPath}.{field.Name}")
            .ToArray();
        var isItemObject = itemId is not null || reference is not null || quantity is not null ||
                           slot is not null || component is not null;

        if (isItemObject)
        {
            var id = ExtractValue(itemId?.Value) ?? ExtractValue(reference?.Value) ??
                     component ?? $"node:{parentNode}";
            var quantityValue = ParseInt(quantity?.Value);
            var slotValue = ExtractValue(slot?.Value);
            var evidence = children.Any(field => field.Status == EvidenceStatus.Observed)
                ? EvidenceStatus.Observed
                : EvidenceStatus.Detected;
            entities.Add(new ItemEntity(
                ExtractValue(itemId?.Value),
                ExtractValue(reference?.Value),
                quantityValue,
                slotValue,
                component,
                parentPath,
                unknown,
                evidence));
            candidates.Add(new ItemCandidate(id, quantityValue, parentPath,
                string.Join(", ", primitive.Select(field => $"{field.Name}={field.Value}")),
                evidence, slotValue, component, unknown));
        }

        foreach (var child in children)
        {
            var path = $"{parentPath}.{child.Name}";
            if (child.Value.StartsWith("array raw ", StringComparison.OrdinalIgnoreCase) &&
                parentPath.Contains("item", StringComparison.OrdinalIgnoreCase))
            {
                containers.Add(new ItemContainerObservation(
                    child.Name,
                    ContainerDisplayName(child.Name),
                    path,
                    child.ChildNode,
                    child.Value,
                    ToCandidateStatus(child.Status)));
            }

            if (child.Value.StartsWith("object ", StringComparison.OrdinalIgnoreCase))
            {
                WalkObject(child.ChildNode, path, fields, candidates, entities, containers, unassigned);
            }
            else if (!isItemObject || !IsMappedField(child.Name))
            {
                unassigned.Add(path);
            }
        }
    }

    private static bool IsMappedField(string name) =>
        IsItemId(name) || IsReference(name) || IsQuantity(name) || IsSlot(name) ||
        ComponentFor(name) is not null;

    private static string? ExtractValue(string? value)
    {
        if (value is null) return null;
        var number = Number.Match(value);
        if (number.Success) return number.Groups[1].Value;
        var text = StringValue.Match(value);
        return text.Success && !string.IsNullOrWhiteSpace(text.Groups[1].Value)
            ? text.Groups[1].Value
            : null;
    }

    private static int? ParseInt(string? value) =>
        ExtractValue(value) is { } text && int.TryParse(text, out var parsed) ? parsed : null;

    private static bool IsItemId(string name) =>
        name.Contains("itemid", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("id", StringComparison.OrdinalIgnoreCase) ||
        name.Contains("prefab", StringComparison.OrdinalIgnoreCase) ||
        name.Contains("template", StringComparison.OrdinalIgnoreCase);

    private static bool IsReference(string name) =>
        name.Contains("reference", StringComparison.OrdinalIgnoreCase) ||
        name.Contains("guid", StringComparison.OrdinalIgnoreCase) ||
        name.Contains("ref", StringComparison.OrdinalIgnoreCase);

    private static bool IsQuantity(string name) =>
        name.Contains("count", StringComparison.OrdinalIgnoreCase) ||
        name.Contains("quantity", StringComparison.OrdinalIgnoreCase) ||
        name.Contains("amount", StringComparison.OrdinalIgnoreCase) ||
        name.Contains("stack", StringComparison.OrdinalIgnoreCase);

    private static bool IsSlot(string name) =>
        name.Contains("slot", StringComparison.OrdinalIgnoreCase) ||
        name.Contains("index", StringComparison.OrdinalIgnoreCase);

    private static string? ComponentFor(string value) =>
        Schemas.FirstOrDefault(schema => value.Contains(schema, StringComparison.OrdinalIgnoreCase));

    private static string ContainerDisplayName(string name) =>
        name switch
        {
            "itemLastUsageOrders" => "Zuletzt verwendete Items",
            "itemFavoritesOrders" => "Favorisierte Items",
            "items" => "Itemdaten",
            _ => name
        };

    private static EvidenceStatus ToCandidateStatus(EvidenceStatus status) =>
        status == EvidenceStatus.Observed ? EvidenceStatus.Observed : EvidenceStatus.Detected;
}
