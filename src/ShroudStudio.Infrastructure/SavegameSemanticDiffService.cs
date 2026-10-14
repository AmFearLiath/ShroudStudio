using System.Buffers.Binary;
using ShroudStudio.Core;

namespace ShroudStudio.Infrastructure;

/// <summary>
/// Compares decoded EHD0 entities instead of treating a savegame as an opaque byte stream.
/// This is the foundation for controlled before/after field attribution.
/// </summary>
public sealed class SavegameSemanticDiffService
{
    public SavegameSemanticDiff Compare(
        SavedEntityArchiveReport before,
        SavedEntityArchiveReport after)
    {
        var changes = new List<SavegameComponentChange>();
        var beforeEntities = before.Entities.ToDictionary(entity => entity.EntityId);
        var afterEntities = after.Entities.ToDictionary(entity => entity.EntityId);
        var entityIds = beforeEntities.Keys
            .Concat(afterEntities.Keys)
            .Distinct()
            .OrderBy(id => id);

        foreach (var entityId in entityIds)
        {
            beforeEntities.TryGetValue(entityId, out var beforeEntity);
            afterEntities.TryGetValue(entityId, out var afterEntity);
            var beforeComponents = beforeEntity?.Components.ToDictionary(component => component.Name)
                                   ?? new Dictionary<string, SavedEntityComponentObservation>();
            var afterComponents = afterEntity?.Components.ToDictionary(component => component.Name)
                                  ?? new Dictionary<string, SavedEntityComponentObservation>();

            foreach (var componentName in beforeComponents.Keys
                         .Concat(afterComponents.Keys)
                         .Distinct(StringComparer.Ordinal)
                         .OrderBy(name => name, StringComparer.Ordinal))
            {
                beforeComponents.TryGetValue(componentName, out var beforeComponent);
                afterComponents.TryGetValue(componentName, out var afterComponent);
                if (beforeComponent is null)
                {
                    changes.Add(CreateComponentChange(
                        entityId, afterComponent!, SavegameChangeKind.Added, null, afterComponent!.Size));
                    continue;
                }

                if (afterComponent is null)
                {
                    changes.Add(CreateComponentChange(
                        entityId, beforeComponent, SavegameChangeKind.Removed, beforeComponent.Size, null));
                    continue;
                }

                var fields = CompareFields(beforeComponent, afterComponent);
                var bytesChanged = !beforeComponent.Data.AsSpan().SequenceEqual(afterComponent.Data);
                if (bytesChanged || fields.Count > 0 || beforeComponent.Size != afterComponent.Size)
                {
                    changes.Add(new SavegameComponentChange(
                        entityId,
                        componentName,
                        SavegameChangeKind.Changed,
                        beforeComponent.Size,
                        afterComponent.Size,
                        fields,
                        beforeComponent.Status == EvidenceStatus.Observed &&
                        afterComponent.Status == EvidenceStatus.Observed
                            ? EvidenceStatus.Observed
                            : EvidenceStatus.Detected));
                }
            }
        }

        var errors = before.Errors.Concat(after.Errors).Distinct().ToArray();
        var status = changes.Count == 0 && errors.Length == 0
            ? EvidenceStatus.Observed
            : changes.Count > 0
                ? EvidenceStatus.Observed
                : EvidenceStatus.Detected;
        return new(before.Entities.Count, after.Entities.Count, changes, errors, status);
    }

    public SavegameSemanticDiff Compare(
        string beforeSavegameDirectory,
        CharacterSavegameOption beforeCharacter,
        string afterSavegameDirectory,
        CharacterSavegameOption afterCharacter)
    {
        var service = new CharacterPayloadService();
        var before = service.AnalyzeSavedEntities(beforeSavegameDirectory, beforeCharacter);
        var after = service.AnalyzeSavedEntities(afterSavegameDirectory, afterCharacter);
        return Compare(before, after);
    }

    private static SavegameComponentChange CreateComponentChange(
        ulong entityId,
        SavedEntityComponentObservation component,
        SavegameChangeKind kind,
        int? beforeSize,
        int? afterSize)
    {
        var fields = kind == SavegameChangeKind.Added
            ? FlattenFields(component).Select(field => new SavegameFieldChange(
                component.Name,
                field.Path,
                null,
                ReadValue(component.Data, field.Offset, field.Size),
                SavegameChangeKind.Added,
                component.Status)).ToArray()
            : [];
        return new(entityId, component.Name, kind, beforeSize, afterSize, fields, component.Status);
    }

    private static IReadOnlyList<SavegameFieldChange> CompareFields(
        SavedEntityComponentObservation before,
        SavedEntityComponentObservation after)
    {
        var beforeFields = FlattenFields(before).ToDictionary(field => field.Path);
        var afterFields = FlattenFields(after).ToDictionary(field => field.Path);
        var changes = new List<SavegameFieldChange>();
        foreach (var path in beforeFields.Keys
                     .Concat(afterFields.Keys)
                     .Distinct(StringComparer.Ordinal)
                     .OrderBy(path => path, StringComparer.Ordinal))
        {
            beforeFields.TryGetValue(path, out var beforeField);
            afterFields.TryGetValue(path, out var afterField);
            var beforeValue = beforeField is null ? null : ReadValue(before.Data, beforeField.Offset, beforeField.Size);
            var afterValue = afterField is null ? null : ReadValue(after.Data, afterField.Offset, afterField.Size);
            if (beforeValue == afterValue)
                continue;

            var kind = beforeField is null
                ? SavegameChangeKind.Added
                : afterField is null
                    ? SavegameChangeKind.Removed
                    : SavegameChangeKind.Changed;
            changes.Add(new(
                after.Name,
                path,
                beforeValue,
                afterValue,
                kind,
                before.Status == EvidenceStatus.Observed &&
                after.Status == EvidenceStatus.Observed
                    ? EvidenceStatus.Observed
                    : EvidenceStatus.Detected));
        }

        return changes;
    }

    private static IReadOnlyList<FieldLocation> FlattenFields(
        SavedEntityComponentObservation component)
    {
        var result = new List<FieldLocation>();
        foreach (var field in component.Fields ?? [])
            FlattenField(result, field, field.Offset, field.Name);
        return result;
    }

    private static void FlattenField(
        List<FieldLocation> output,
        ComponentSchemaFieldObservation field,
        int offset,
        string path)
    {
        if (field.ElementFields is { Count: > 0 } children &&
            field.ElementCount is { } count && count > 0)
        {
            var elementSize = field.Size / count;
            if (elementSize <= 0)
                return;

            for (var index = 0; index < count; index++)
            {
                foreach (var child in children)
                    FlattenField(output, child, offset + index * elementSize + child.Offset,
                        $"{path}[{index + 1}].{child.Name}");
            }
        }
        else if (field.Size is 1 or 2 or 4 or 8)
        {
            output.Add(new FieldLocation(path, offset, field.Size));
        }
    }

    private static string? ReadValue(byte[]? data, int offset, int size)
    {
        if (data is null || offset < 0 || offset > data.Length - size)
            return null;

        var bytes = data.AsSpan(offset, size);
        var numeric = size switch
        {
            1 => bytes[0].ToString(),
            2 => BinaryPrimitives.ReadUInt16LittleEndian(bytes).ToString(),
            4 => BinaryPrimitives.ReadUInt32LittleEndian(bytes).ToString(),
            8 => BinaryPrimitives.ReadUInt64LittleEndian(bytes).ToString(),
            _ => null
        };
        return numeric is null ? null : $"{numeric} (0x{Convert.ToHexString(bytes)})";
    }

    private sealed record FieldLocation(string Path, int Offset, int Size);
}
