using System.Buffers.Binary;
using ShroudStudio.Core;

namespace ShroudStudio.Infrastructure;

/// <summary>
/// Reads verified Inventory slot references without changing any save data.
/// Their semantic relationship to ItemState entities is not yet confirmed.
/// </summary>
public sealed class CharacterInventoryService
{
    public CharacterInventoryReport Analyze(
        string savegameDirectory,
        CharacterSavegameOption character,
        ItemCatalogReport? catalog = null)
    {
        var archive = new CharacterPayloadService().AnalyzeSavedEntities(savegameDirectory, character);
        if (archive.Status != EvidenceStatus.Observed)
            return new([], [], archive.Errors, archive.Status);

        var slots = new List<CharacterInventorySlotObservation>();
        var containers = new List<CharacterInventoryContainerObservation>();
        var inventoryEntities = archive.Entities.ToDictionary(entity => entity.EntityId);
        var assignedInventoryEntities = new HashSet<ulong>();
        var groupIndex = 0;

        foreach (var setupEntity in archive.Entities)
        {
            foreach (var setup in setupEntity.Components.Where(component =>
                         string.Equals(component.Name, "InventorySetup", StringComparison.Ordinal)))
            {
                var entityLinks = ReadEntityLinks(setup);
                var linkedInventories = entityLinks
                    .Distinct()
                    .Where(inventoryEntities.ContainsKey)
                    .Select(id => (Id: id, Entity: inventoryEntities[id]))
                    .Where(entry => entry.Entity.Components.Any(component =>
                        string.Equals(component.Name, "Inventory", StringComparison.Ordinal)))
                    .ToArray();
                if (linkedInventories.Length == 0)
                {
                    continue;
                }

                groupIndex++;
                var name = $"Inventargruppe {groupIndex}";
                var groupSlots = 0;
                foreach (var (_, entity) in linkedInventories)
                {
                    assignedInventoryEntities.Add(entity.EntityId);
                    foreach (var inventory in entity.Components.Where(component =>
                                 string.Equals(component.Name, "Inventory", StringComparison.Ordinal)))
                    {
                        groupSlots += ReadSlots(slots, name, inventory, catalog, inventoryEntities);
                    }
                }

                containers.Add(new CharacterInventoryContainerObservation(
                    name,
                    setupEntity.EntityId,
                    linkedInventories.Length,
                    groupSlots,
                    ReadByteField(setup, "availableSlotCount"),
                    EvidenceStatus.Observed));
            }
        }

        var unassigned = archive.Entities
            .Where(entity => !assignedInventoryEntities.Contains(entity.EntityId))
            .SelectMany(entity => entity.Components.Where(component =>
                string.Equals(component.Name, "Inventory", StringComparison.Ordinal)))
            .ToArray();
        if (unassigned.Length > 0)
        {
            var name = "Nicht zugeordnete Inventargruppe";
            var groupSlots = 0;
            foreach (var inventory in unassigned)
                groupSlots += ReadSlots(slots, name, inventory, catalog, inventoryEntities);
            containers.Add(new CharacterInventoryContainerObservation(
                name, 0, unassigned.Length, groupSlots, null, EvidenceStatus.Detected));
        }

        return new(
            slots,
            containers,
            [],
            slots.Count > 0 ? EvidenceStatus.Observed : EvidenceStatus.Detected);
    }

    private static IReadOnlyList<ulong> ReadEntityLinks(SavedEntityComponentObservation setup)
    {
        var field = setup.Fields?.SingleOrDefault(candidate =>
            string.Equals(candidate.Name, "linksEntities", StringComparison.Ordinal));
        if (field?.ElementCount is not { } elementCount ||
            elementCount <= 0 ||
            field.Size != elementCount * sizeof(uint) ||
            setup.Data is null ||
            field.Offset > setup.Data.Length - field.Size)
        {
            return [];
        }

        var links = new List<ulong>(elementCount);
        for (var index = 0; index < elementCount; index++)
        {
            var offset = checked(field.Offset + index * sizeof(uint));
            var link = BinaryPrimitives.ReadUInt32LittleEndian(setup.Data.AsSpan(offset, sizeof(uint)));
            if (link != 0)
                links.Add(link);
        }

        return links;
    }

    private static byte? ReadByteField(SavedEntityComponentObservation component, string name)
    {
        var field = component.Fields?.SingleOrDefault(candidate =>
            string.Equals(candidate.Name, name, StringComparison.Ordinal) && candidate.Size == sizeof(byte));
        if (field is null || component.Data is null || field.Offset >= component.Data.Length)
            return null;

        return component.Data[field.Offset];
    }

    private static int ReadSlots(
        List<CharacterInventorySlotObservation> output,
        string containerName,
        SavedEntityComponentObservation inventory,
        ItemCatalogReport? catalog,
        IReadOnlyDictionary<ulong, SavedEntityObservation> entities)
    {
        var slotField = inventory.Fields?.SingleOrDefault(field =>
            string.Equals(field.Name, "slots", StringComparison.Ordinal));
        if (slotField?.ElementCount is not { } elementCount ||
            slotField.ElementFields is null ||
            inventory.Data is null ||
            elementCount <= 0 ||
            inventory.Data.Length != slotField.Size)
        {
            return 0;
        }

        var idField = slotField.ElementFields.SingleOrDefault(field =>
            string.Equals(field.Name, "id", StringComparison.Ordinal) && field.Size == 4);
        var dataField = slotField.ElementFields.SingleOrDefault(field =>
            string.Equals(field.Name, "data", StringComparison.Ordinal));
        var countField = dataField?.ElementFields?.SingleOrDefault(field =>
            string.Equals(field.Name, "count", StringComparison.Ordinal) && field.Size == 4);
        var stateField = dataField?.ElementFields?.SingleOrDefault(field =>
            string.Equals(field.Name, "pide", StringComparison.Ordinal) && field.Size == 4);
        var elementSize = slotField.Size / elementCount;
        if (idField is null || elementSize * elementCount != inventory.Data.Length ||
            idField.Offset > elementSize - sizeof(uint))
        {
            return 0;
        }

        for (var index = 0; index < elementCount; index++)
        {
            var offset = checked(slotField.Offset + index * elementSize + idField.Offset);
            var reference = BinaryPrimitives.ReadUInt32LittleEndian(inventory.Data.AsSpan(offset, 4));
            uint? quantity = countField is not null && dataField is not null &&
                             dataField.Offset <= elementSize - countField.Offset - sizeof(uint)
                ? BinaryPrimitives.ReadUInt32LittleEndian(inventory.Data.AsSpan(
                    checked(slotField.Offset + index * elementSize + dataField.Offset + countField.Offset), 4))
                : null;
            uint? itemStateReference = stateField is not null && dataField is not null &&
                                       dataField.Offset <= elementSize - stateField.Offset - sizeof(uint)
                ? BinaryPrimitives.ReadUInt32LittleEndian(inventory.Data.AsSpan(
                    checked(slotField.Offset + index * elementSize + dataField.Offset + stateField.Offset), 4))
                : null;
            var definition = catalog?.Definitions.FirstOrDefault(item => item.NumericId == reference);
            var instance = ReadItemInstance(reference, itemStateReference, entities);
            output.Add(new CharacterInventorySlotObservation(
                containerName,
                output.Count(slot => string.Equals(slot.ContainerName, containerName, StringComparison.Ordinal)),
                reference,
                reference == 0
                    ? EvidenceStatus.Observed
                    : definition is not null
                        ? EvidenceStatus.Verified
                        : EvidenceStatus.Detected,
                quantity,
                itemStateReference,
                definition?.DisplayName,
                definition?.Category,
                FormatDetails(definition, instance),
                instance,
                definition));
        }

        return elementCount;
    }

    private static ItemInstanceObservation? ReadItemInstance(
        uint itemReference,
        uint? entityReference,
        IReadOnlyDictionary<ulong, SavedEntityObservation> entities)
    {
        if (entityReference is not { } reference ||
            !entities.TryGetValue(reference, out var entity))
        {
            return null;
        }

        var itemState = entity.Components.SingleOrDefault(component =>
            string.Equals(component.Name, "ItemState", StringComparison.Ordinal));
        if (itemState is null || ReadUInt32Field(itemState, "itemId") != itemReference)
            return null;

        return new ItemInstanceObservation(
            entity.EntityId,
            itemReference,
            ReadUInt16Field(itemState, "baseDamageUi"),
            ReadByteField(itemState, "itemRarityUi"),
            ReadByteField(itemState, "itemLevelUi"),
            entity.Components.Any(component => string.Equals(component.Name, "Durability", StringComparison.Ordinal)),
            entity.Components.Any(component => string.Equals(component.Name, "PerkContainerNew", StringComparison.Ordinal)),
            entity.Components.Any(component => string.Equals(component.Name, "WeaponGemSlot", StringComparison.Ordinal)),
            entity.Components.Any(component => string.Equals(component.Name, "ColorPaletteItem", StringComparison.Ordinal)),
            EvidenceStatus.Observed,
            ReadProperties(entity.Components),
            entity.Components.Select(component => component.Name).ToArray());
    }

    private static IReadOnlyList<ItemPropertyObservation> ReadProperties(
        IReadOnlyList<SavedEntityComponentObservation> components)
    {
        var properties = new List<ItemPropertyObservation>();
        foreach (var component in components)
        {
            if (component.Data is null || component.Fields is null)
                continue;

            foreach (var field in component.Fields)
                ReadField(properties, component.Name, field, component.Data, field.Offset, field.Name);
        }

        return properties;
    }

    private static void ReadField(
        List<ItemPropertyObservation> output,
        string componentName,
        ComponentSchemaFieldObservation field,
        byte[] data,
        int offset,
        string displayName)
    {
        if (field.ElementFields is { Count: > 0 } children &&
            field.ElementCount is { } count && count > 0)
        {
            var elementSize = field.Size / count;
            if (elementSize <= 0)
                return;

            for (var index = 0; index < count && output.Count < 256; index++)
            {
                foreach (var child in children)
                {
                    var childOffset = checked(offset + index * elementSize + child.Offset);
                    ReadField(output, componentName, child, data, childOffset,
                        $"{displayName}[{index + 1}].{child.Name}");
                }
            }

            return;
        }

        if (output.Count >= 256 ||
            offset < 0 ||
            field.Size is not (1 or 2 or 4 or 8) ||
            offset > data.Length - field.Size)
        {
            return;
        }

        var value = field.Size switch
        {
            1 => data[offset].ToString(),
            2 => BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(offset, 2)).ToString(),
            4 => BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(offset, 4)).ToString(),
            8 => BinaryPrimitives.ReadUInt64LittleEndian(data.AsSpan(offset, 8)).ToString(),
            _ => string.Empty
        };
        output.Add(new(
            componentName,
            displayName,
            value,
            EvidenceStatus.Observed,
            $"Komponente {componentName}, Offset 0x{offset:X}"));
    }

    private static uint? ReadUInt32Field(SavedEntityComponentObservation component, string name)
    {
        var field = component.Fields?.SingleOrDefault(candidate =>
            string.Equals(candidate.Name, name, StringComparison.Ordinal) && candidate.Size == sizeof(uint));
        if (field is null || component.Data is null || field.Offset > component.Data.Length - sizeof(uint))
            return null;

        return BinaryPrimitives.ReadUInt32LittleEndian(component.Data.AsSpan(field.Offset, sizeof(uint)));
    }

    private static ushort? ReadUInt16Field(SavedEntityComponentObservation component, string name)
    {
        var field = component.Fields?.SingleOrDefault(candidate =>
            string.Equals(candidate.Name, name, StringComparison.Ordinal) && candidate.Size == sizeof(ushort));
        if (field is null || component.Data is null || field.Offset > component.Data.Length - sizeof(ushort))
            return null;

        return BinaryPrimitives.ReadUInt16LittleEndian(component.Data.AsSpan(field.Offset, sizeof(ushort)));
    }

    private static string? FormatDetails(ItemDefinition? definition, ItemInstanceObservation? instance)
    {
        var details = new List<string>();
        if (!string.IsNullOrWhiteSpace(definition?.Category))
            details.Add(definition.Category);
        if (!string.IsNullOrWhiteSpace(definition?.Rarity))
            details.Add(definition.Rarity);
        if (definition?.MinLevel is not null || definition?.MaxLevel is not null)
            details.Add($"Stufe {definition?.MinLevel?.ToString() ?? "?"}-{definition?.MaxLevel?.ToString() ?? "?"}");
        if (!string.IsNullOrWhiteSpace(definition?.DamageSummary))
            details.Add("Schadenswerte");
        if (definition?.Buffs is { Count: > 0 })
            details.Add($"{definition.Buffs.Count} Boni/Buffs");
        if (instance?.Level is { } level)
            details.Add($"Itemstufe {level}");
        if (instance?.BaseDamage is { } damage)
            details.Add($"Basisschaden {damage}");
        if (instance?.HasDurability == true)
            details.Add("Haltbarkeit");
        if (instance?.HasPerks == true)
            details.Add("Perks");
        if (instance?.HasWeaponGem == true)
            details.Add("Sockel");
        if (instance?.HasColorPalette == true)
            details.Add("Färbbar");
        return details.Count == 0 ? null : string.Join(" · ", details);
    }
}
