using System.Buffers.Binary;
using System.Text;
using ShroudStudio.Core;

namespace ShroudStudio.Infrastructure;

/// <summary>
/// Read-only decoder for the CTCB component schema embedded in an ESC2 definition.
/// It exposes only validated root fields; component values remain opaque until their
/// types and meanings are independently verified.
/// </summary>
public sealed class ComponentSchemaAnalyzer
{
    private const int HeaderSize = 40;
    private const int TypeSize = 32;
    private const int FieldSize = 8;
    private const uint StructKind = 18;

    public IReadOnlyList<ComponentSchemaFieldObservation> DescribeRootFields(byte[] schema)
    {
        try
        {
            if (schema.Length < HeaderSize || !schema.AsSpan(0, 4).SequenceEqual("CTCB"u8) ||
                ReadUInt32(schema, 4) != schema.Length)
            {
                return [];
            }

            var types = ReadSection(schema, 16, TypeSize);
            var pool = ReadSection(schema, 24, 1);
            var fields = ReadSection(schema, 32, FieldSize);
            if (types.Count == 0 || types.Count > 2_048)
                return [];

            var typeRecords = new TypeRecord[types.Count];
            for (var index = 0; index < types.Count; index++)
            {
                var offset = types.Offset + index * TypeSize;
                var nameReference = ReadUInt16(schema, offset);
                var innerType = ReadUInt16(schema, offset + 6);
                var size = ReadUInt32(schema, offset + 8);
                var itemCount = ReadUInt32(schema, offset + 12);
                var kind = ReadUInt32(schema, offset + 16);
                var start = ReadUInt32(schema, offset + 28);
                typeRecords[index] = new TypeRecord(
                    ReadString(schema, pool, nameReference),
                    innerType,
                    size,
                    itemCount,
                    kind,
                    start);
            }

            var root = typeRecords[0];
            if (root.Kind != StructKind || root.Start == 0 || root.Start - 1 >= fields.Count)
                return [];

            if (root.ItemCount > fields.Count - (root.Start - 1))
                return [];

            var fieldCount = checked((int)root.ItemCount);
            var result = new List<ComponentSchemaFieldObservation>();
            for (var index = 0; index < fieldCount; index++)
            {
                var offset = fields.Offset + checked(((int)root.Start - 1 + index) * FieldSize);
                var nameReference = ReadUInt16(schema, offset);
                var typeId = ReadUInt16(schema, offset + 2);
                var fieldOffset = ReadUInt32(schema, offset + 4);
                if (typeId == 0 || typeId > typeRecords.Length)
                    break;

                var type = typeRecords[typeId - 1];
                if (fieldOffset > root.Size || type.Size > root.Size - fieldOffset)
                    break;

                result.Add(DescribeField(
                    schema,
                    pool,
                    fields,
                    typeRecords,
                    ReadString(schema, pool, nameReference),
                    checked((int)fieldOffset),
                    type));
            }

            return result;
        }
        catch (InvalidDataException)
        {
            return [];
        }
    }

    private static ComponentSchemaFieldObservation DescribeField(
                byte[] schema,
                Section pool,
                Section fields,
                IReadOnlyList<TypeRecord> types,
                string name,
                int offset,
                TypeRecord type)
            {
                if (type.Kind == StructKind)
                {
                    return new ComponentSchemaFieldObservation(
                        name,
                        offset,
                        checked((int)type.Size),
                        type.Kind,
                        EvidenceStatus.Observed,
                        null,
                        DescribeStructureFields(schema, pool, fields, types, type));
                }

                if (type.Kind != 19 || type.InnerType == 0 || type.InnerType > types.Count)
                {
                    return new ComponentSchemaFieldObservation(
                        name, offset, checked((int)type.Size), type.Kind, EvidenceStatus.Observed);
                }

                var elementType = types[type.InnerType - 1];
                var elementFields = DescribeStructureFields(schema, pool, fields, types, elementType);
                return new ComponentSchemaFieldObservation(
                    name,
                    offset,
                    checked((int)type.Size),
                    type.Kind,
                    EvidenceStatus.Observed,
                    checked((int)type.ItemCount),
                    elementFields);
            }

    private static IReadOnlyList<ComponentSchemaFieldObservation> DescribeStructureFields(
                byte[] schema,
                Section pool,
                Section fields,
                IReadOnlyList<TypeRecord> types,
                TypeRecord structure)
            {
                if (structure.Kind != StructKind || structure.Start == 0 ||
                    structure.Start - 1 >= fields.Count ||
                    structure.ItemCount > fields.Count - (structure.Start - 1))
                {
                    return [];
                }

                var result = new List<ComponentSchemaFieldObservation>();
                for (var index = 0; index < structure.ItemCount; index++)
                {
                    var offset = fields.Offset + checked(((int)structure.Start - 1 + index) * FieldSize);
                    var nameReference = ReadUInt16(schema, offset);
                    var typeId = ReadUInt16(schema, offset + 2);
                    var fieldOffset = ReadUInt32(schema, offset + 4);
                    if (typeId == 0 || typeId > types.Count)
                        return [];

                    var type = types[typeId - 1];
                    if (fieldOffset > structure.Size || type.Size > structure.Size - fieldOffset)
                        return [];

                    result.Add(DescribeField(
                        schema,
                        pool,
                        fields,
                        types,
                        ReadString(schema, pool, nameReference),
                        checked((int)fieldOffset),
                        type));
                }

        return result;
    }

    private static Section ReadSection(byte[] data, int headerOffset, int itemWidth)
    {
        var relativeOffset = ReadUInt32(data, headerOffset);
        var count = ReadUInt32(data, headerOffset + 4);
        var offset = checked(headerOffset + (int)relativeOffset);
        if (offset < HeaderSize || count > int.MaxValue / itemWidth ||
            offset > data.Length - checked((int)count * itemWidth))
        {
            throw new InvalidDataException("Ungültiger CTCB-Abschnitt.");
        }

        return new Section(offset, checked((int)count));
    }

    private static string ReadString(byte[] data, Section pool, ushort reference)
    {
        if (reference == 0 || reference > pool.Count)
            throw new InvalidDataException("Ungültige CTCB-Stringreferenz.");

        var offset = pool.Offset + reference - 1;
        var byteLength = data[offset] + 1;
        if (offset + 1 + byteLength > pool.Offset + pool.Count)
            throw new InvalidDataException("CTCB-String liegt außerhalb des Pools.");

        return Encoding.UTF8.GetString(data, offset + 1, byteLength);
    }

    private static ushort ReadUInt16(byte[] data, int offset) =>
        BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(offset, 2));

    private static uint ReadUInt32(byte[] data, int offset) =>
        BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(offset, 4));

    private sealed record Section(int Offset, int Count);
    private sealed record TypeRecord(
        string Name,
        ushort InnerType,
        uint Size,
        uint ItemCount,
        uint Kind,
        uint Start);
}
