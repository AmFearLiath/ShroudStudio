using System.Buffers.Binary;
using System.Text;
using ShroudStudio.Core;

namespace ShroudStudio.Infrastructure;

/// <summary>
/// Bounded reader for the EHD0 entity archive embedded in a character's BDB1 data array.
/// It describes archive framing and components only; no component values are interpreted here.
/// </summary>
public sealed class SavedEntityArchiveAnalyzer
{
    private const int HeaderSize = 43;
    private const int MaxEntities = 100_000;
    private const int MaxComponents = 4_096;

    public SavedEntityArchiveReport Analyze(byte[] payload)
    {
        try
        {
            if (payload.Length < HeaderSize ||
                !payload.AsSpan(8, 4).SequenceEqual("EHD0"u8) ||
                ReadUInt32(payload, 12) != 1)
            {
                throw new InvalidDataException("Die gespeicherten Entitäten besitzen kein unterstütztes EHD0-Archiv.");
            }

            var schemaOffset = CheckedOffset(ReadUInt32(payload, 0), payload.Length);
            var entitySectionSize = ReadUInt32(payload, 37);
            if (schemaOffset < HeaderSize || entitySectionSize != schemaOffset - 41)
                throw new InvalidDataException("Die EHD0-Abschnittsgrenzen sind ungültig.");

            var definitions = ReadDefinitions(payload, schemaOffset);
            var entities = ReadEntities(payload, schemaOffset, definitions);
            return new SavedEntityArchiveReport(
                payload.Length,
                entities,
                definitions.Values.Select(definition => definition.Name)
                    .OrderBy(name => name, StringComparer.Ordinal).ToArray(),
                [],
                EvidenceStatus.Observed);
        }
        catch (InvalidDataException ex)
        {
            return new SavedEntityArchiveReport(payload.Length, [], [], [ex.Message], EvidenceStatus.Detected);
        }
    }

    private static Dictionary<ushort, ComponentDefinition> ReadDefinitions(byte[] payload, int schemaOffset)
    {
        var reader = new Reader(payload, schemaOffset, payload.Length);
        if (!reader.Take(4).SequenceEqual("ESC2"u8))
            throw new InvalidDataException("Das EHD0-Archiv verwendet kein unterstütztes ESC2-Schema.");

        var definitionCount = reader.ReadUInt16();
        var templateCount = reader.ReadUInt16();
        if (definitionCount > MaxComponents)
            throw new InvalidDataException("Das ESC2-Schema überschreitet die Sicherheitsgrenze.");

        reader.Skip(12);
        var definitions = new Dictionary<ushort, ComponentDefinition>();
        for (var index = 0; index < definitionCount; index++)
        {
            var name = reader.ReadName(Encoding.ASCII, 255);
            reader.Skip(8);
            var componentId = reader.ReadUInt16();
            var declaredSize = reader.ReadUInt16();
            var schemaSize = reader.ReadUInt16();
            var schema = reader.Take(schemaSize);
            var fields = new ComponentSchemaAnalyzer().DescribeRootFields(schema);
            if (!definitions.TryAdd(componentId, new ComponentDefinition(name, declaredSize, fields)))
                throw new InvalidDataException("Das ESC2-Schema enthält doppelte Komponenten-IDs.");
        }

        for (var index = 0; index < templateCount; index++)
        {
            reader.Skip(16);
            _ = reader.ReadName(Encoding.UTF8, 1_048_576);
            reader.Skip(80);
        }

        if (!reader.AtEnd)
            throw new InvalidDataException("Die ESC2-Templates enden nicht am Archivende.");

        return definitions;
    }

    private static IReadOnlyList<SavedEntityObservation> ReadEntities(
        byte[] payload,
        int schemaOffset,
        IReadOnlyDictionary<ushort, ComponentDefinition> definitions)
    {
        var reader = new Reader(payload, 41, schemaOffset);
        var entityCount = reader.ReadUInt16();
        var table = new (ulong EntityId, ushort TemplateIndex)[entityCount];
        for (var index = 0; index < entityCount; index++)
            table[index] = (reader.ReadUInt64(), reader.ReadUInt16());

        var entities = new List<SavedEntityObservation>(entityCount);
        foreach (var entry in table)
        {
            var serverCount = reader.ReadUInt16();
            var clientCount = reader.ReadUInt16();
            if (serverCount + clientCount > MaxComponents)
                throw new InvalidDataException("Eine EHD0-Entität enthält zu viele Komponenten.");

            var components = new List<SavedEntityComponentObservation>(serverCount + clientCount);
            ReadComponents(reader, serverCount, false, definitions, components);
            ReadComponents(reader, clientCount, true, definitions, components);
            entities.Add(new SavedEntityObservation(entry.EntityId, entry.TemplateIndex, components,
                EvidenceStatus.Observed));
        }

        if (!reader.AtEnd)
            throw new InvalidDataException("Die EHD0-Entitätsdaten enden nicht an der Schema-Grenze.");

        return entities;
    }

    private static void ReadComponents(
        Reader reader,
        int count,
        bool isClient,
        IReadOnlyDictionary<ushort, ComponentDefinition> definitions,
        List<SavedEntityComponentObservation> output)
    {
        for (var index = 0; index < count; index++)
        {
            var componentId = reader.ReadUInt16();
            var size = reader.ReadUInt16();
            var data = reader.Take(size);
            var definition = definitions.GetValueOrDefault(componentId);
            output.Add(new SavedEntityComponentObservation(
                definition?.Name ?? $"Komponente {componentId}",
                componentId,
                size,
                isClient,
                definition is not null && definition.DeclaredSize == size
                    ? EvidenceStatus.Observed
                    : EvidenceStatus.Detected,
                definition?.Fields,
                data));
        }
    }

    private static uint ReadUInt32(byte[] payload, int offset) =>
        BinaryPrimitives.ReadUInt32LittleEndian(payload.AsSpan(offset, 4));

    private static int CheckedOffset(uint value, int length) =>
        value <= length ? (int)value : throw new InvalidDataException("Ungültiger EHD0-Schemaoffset.");

    private sealed class Reader(byte[] data, int offset, int end)
    {
        private int _offset = offset;

        public bool AtEnd => _offset == end;

        public byte[] Take(int count)
        {
            if (count < 0 || count > end - _offset)
                throw new InvalidDataException("Ein EHD0-Abschnitt wurde unerwartet abgeschnitten.");

            var result = data.AsSpan(_offset, count).ToArray();
            _offset += count;
            return result;
        }

        public void Skip(int count) => _ = Take(count);
        public ushort ReadUInt16() => BinaryPrimitives.ReadUInt16LittleEndian(Take(2));
        public ulong ReadUInt64() => BinaryPrimitives.ReadUInt64LittleEndian(Take(8));

        public string ReadName(Encoding encoding, int maximum)
        {
            var length = BinaryPrimitives.ReadUInt32LittleEndian(Take(4));
            if (length == 0 || length > maximum)
                throw new InvalidDataException("Ein EHD0-Name besitzt eine ungültige Länge.");

            try
            {
                var name = encoding.GetString(Take((int)length));
                if (name.IndexOf('\0') >= 0)
                    throw new InvalidDataException("Ein EHD0-Name enthält ein ungültiges Nullzeichen.");
                return name;
            }
            catch (DecoderFallbackException ex)
            {
                throw new InvalidDataException("Ein EHD0-Name ist nicht gültig kodiert.", ex);
            }
        }
    }

    private sealed record ComponentDefinition(
        string Name,
        ushort DeclaredSize,
        IReadOnlyList<ComponentSchemaFieldObservation> Fields);
}
