using ShroudStudio.Core;

namespace ShroudStudio.Infrastructure;

public sealed class InventorySemanticDiffService
{
    public InventorySemanticDiff Compare(
        string beforeSavegameDirectory,
        CharacterSavegameOption beforeCharacter,
        string afterSavegameDirectory,
        CharacterSavegameOption afterCharacter,
        ItemCatalogReport? catalog = null)
    {
        var before = new CharacterInventoryService().Analyze(
            beforeSavegameDirectory, beforeCharacter, catalog);
        var after = new CharacterInventoryService().Analyze(
            afterSavegameDirectory, afterCharacter, catalog);
        var beforeSlots = before.Slots.ToDictionary(
            slot => (slot.ContainerName, slot.SlotIndex));
        var afterSlots = after.Slots.ToDictionary(
            slot => (slot.ContainerName, slot.SlotIndex));
        var changes = new List<InventorySlotChange>();

        foreach (var key in beforeSlots.Keys
                     .Concat(afterSlots.Keys)
                     .Distinct()
                     .OrderBy(key => key.ContainerName, StringComparer.Ordinal)
                     .ThenBy(key => key.SlotIndex))
        {
            beforeSlots.TryGetValue(key, out var beforeSlot);
            afterSlots.TryGetValue(key, out var afterSlot);
            var beforeReference = beforeSlot?.ItemReference ?? 0;
            var afterReference = afterSlot?.ItemReference ?? 0;
            var beforeQuantity = beforeSlot?.Quantity;
            var afterQuantity = afterSlot?.Quantity;
            if (beforeReference == afterReference && beforeQuantity == afterQuantity)
                continue;

            changes.Add(new(
                key.ContainerName,
                key.SlotIndex,
                beforeReference,
                afterReference,
                beforeSlot?.ItemName,
                afterSlot?.ItemName,
                beforeQuantity,
                afterQuantity,
                before.Status == EvidenceStatus.Observed &&
                after.Status == EvidenceStatus.Observed
                    ? EvidenceStatus.Observed
                    : EvidenceStatus.Detected));
        }

        var status = before.Status == EvidenceStatus.Observed &&
                     after.Status == EvidenceStatus.Observed
            ? EvidenceStatus.Observed
            : changes.Count > 0
                ? EvidenceStatus.Detected
                : EvidenceStatus.Unknown;
        return new(changes, status);
    }
}
