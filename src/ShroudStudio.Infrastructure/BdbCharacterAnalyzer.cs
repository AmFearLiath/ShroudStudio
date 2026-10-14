using System.Buffers.Binary;
using System.Text;
using ShroudStudio.Core;

namespace ShroudStudio.Infrastructure;

public sealed class BdbCharacterAnalyzer
{
    private const int HeaderSize = 112;
    private const int MaxNodes = 2_000_000;

    public CharacterBlobObservation Analyze(
        string containerFile,
        Ksc1Blob blob,
        byte[] payload)
    {
        try
        {
            if (!payload.AsSpan().StartsWith("BDB1"u8))
            {
                throw new InvalidDataException("CHAR payload is not a BDB1 object archive.");
            }

            var archive = new Archive(payload);
            var fields = archive.DescribeFields();
            var name = archive.TryStringField("name");
            var level = archive.TryUInt32Field("level");
            return new CharacterBlobObservation(
                containerFile,
                blob.Index,
                blob.OwnerId,
                blob.Tag,
                payload.Length,
                name,
                level,
                archive.NodeCount,
                archive.ObjectCount,
                fields,
                EvidenceStatus.Observed);
        }
        catch (InvalidDataException ex)
        {
            return new CharacterBlobObservation(
                containerFile, blob.Index, blob.OwnerId, blob.Tag, payload.Length,
                null, null, 0, 0, [], EvidenceStatus.Detected, ex.Message);
        }
    }

    public byte[] ReadByteArrayField(byte[] payload, string fieldName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName);
        return new Archive(payload).ReadByteArrayField(fieldName);
    }

    private sealed class Archive
    {
        private readonly byte[] _data;
        private readonly int _types;
        private readonly int _values;
        private readonly int _pool;
        private readonly int _poolSize;
        private readonly int _arrayDescriptors;
        private readonly int _arrayDescriptorCount;
        private readonly int _arrayChildren;
        private readonly int _arrayChildCount;
        private readonly int _keys;
        private readonly int _links;
        private readonly Dictionary<int, Dictionary<uint, int>> _edges = [];
        private readonly Dictionary<uint, string> _labels = [];

        public Archive(byte[] data)
        {
            _data = data;
            if (data.Length < HeaderSize || !data.AsSpan().StartsWith("BDB1"u8))
            {
                throw new InvalidDataException("Invalid BDB1 header.");
            }

            NodeCount = ReadCount(4, 1, out _types);
            var valueCount = ReadCount(12, 4, out _values);
            _poolSize = ReadCount(60, 1, out _pool);
            _arrayDescriptorCount = ReadCount(28, 8, out _arrayDescriptors);
            _arrayChildCount = ReadCount(76, 4, out _arrayChildren);
            var keyCount = ReadCount(100, 8, out _keys);
            var linkCount = ReadCount(108, 4, out _links);
            if (NodeCount == 0 || NodeCount > MaxNodes ||
                NodeCount != valueCount || keyCount != linkCount)
            {
                throw new InvalidDataException("Inconsistent BDB1 node tables.");
            }

            if (ReadType(0) != 20)
            {
                throw new InvalidDataException("BDB1 root node is not an object.");
            }

            BuildLabels();
            for (var i = 0; i < keyCount; i++)
            {
                var keyHash = ReadUInt32(_keys + i * 8);
                var parent = ReadUInt32(_keys + i * 8 + 4);
                var child = ReadUInt32(_links + i * 4);
                if (parent == 0 || parent > NodeCount || child == 0 || child > NodeCount ||
                    ReadType((int)parent - 1) != 20)
                {
                    throw new InvalidDataException("BDB1 object link is invalid.");
                }

                var fields = _edges.GetValueOrDefault((int)parent - 1);
                if (fields is null)
                {
                    fields = [];
                    _edges[(int)parent - 1] = fields;
                }

                if (!fields.TryAdd(keyHash, (int)child - 1))
                {
                    throw new InvalidDataException("BDB1 contains a duplicate field hash.");
                }
            }
        }

        public int NodeCount { get; }
        public int ObjectCount => Enumerable.Range(0, NodeCount).Count(index => ReadType(index) == 20);

        public string? TryStringField(string name)
        {
            return TryField(name, expectedType: 14, out var node)
                ? ReadString(node)
                : null;
        }

        public uint? TryUInt32Field(string name)
        {
            return TryField(name, expectedType: 8, out var node)
                ? ReadValue(node)
                : null;
        }

        public IReadOnlyList<CharacterFieldObservation> DescribeFields()
        {
            var result = new List<CharacterFieldObservation>();
            foreach (var (parent, fields) in _edges.OrderBy(pair => pair.Key))
            {
                foreach (var (hash, child) in fields.OrderBy(pair => pair.Key))
                {
                    var name = _labels.GetValueOrDefault(hash, $"hash:{hash:x8}");
                    result.Add(new CharacterFieldObservation(
                        parent + 1,
                        name,
                        child + 1,
                        DescribeNode(child),
                        _labels.ContainsKey(hash)
                            ? EvidenceStatus.Observed
                            : EvidenceStatus.Detected));
                }

            }

            return result;
        }

        public byte[] ReadByteArrayField(string fieldName)
        {
            if (!_edges.TryGetValue(0, out var fields) ||
                !fields.TryGetValue(Fnv1A(fieldName), out var node) ||
                ReadType(node) != 21)
            {
                throw new InvalidDataException($"BDB1 field '{fieldName}' is not an array.");
            }

            var descriptorId = ReadValue(node);
            if (descriptorId == 0 || descriptorId > _arrayDescriptorCount)
            {
                throw new InvalidDataException($"BDB1 array '{fieldName}' has an invalid descriptor.");
            }

            var descriptorOffset = checked(_arrayDescriptors + ((int)descriptorId - 1) * 8);
            var itemCount = ReadUInt32(descriptorOffset);
            var firstChild = ReadUInt32(descriptorOffset + 4);
            if (itemCount > _arrayChildCount || firstChild == 0 ||
                firstChild - 1 > _arrayChildCount - itemCount)
            {
                throw new InvalidDataException($"BDB1 array '{fieldName}' has an invalid child range.");
            }

            var result = new byte[itemCount];
            for (var index = 0; index < itemCount; index++)
            {
                var childId = ReadUInt32(_arrayChildren + checked(((int)firstChild - 1 + index) * 4));
                if (childId == 0 || childId > NodeCount)
                    throw new InvalidDataException($"BDB1 array '{fieldName}' references an invalid node.");

                var child = (int)childId - 1;
                if (ReadType(child) != 6 || ReadValue(child) > byte.MaxValue)
                    throw new InvalidDataException($"BDB1 array '{fieldName}' is not stored as bytes.");

                result[index] = (byte)ReadValue(child);
            }

            return result;
        }

        private bool TryField(string name, int expectedType, out int node)
        {
            node = -1;
            if (!_edges.TryGetValue(0, out var fields) ||
                !fields.TryGetValue(Fnv1A(name), out node) ||
                ReadType(node) != expectedType)
            {
                return false;
            }

            return true;
        }

        private void BuildLabels()
        {
            var keyStrings = ReadCount(68, 8, out var start);
            for (var i = 0; i < keyStrings; i++)
            {
                var reference = ReadUInt32(start + i * 8);
                if (reference == 0 || reference >= _poolSize)
                {
                    throw new InvalidDataException("BDB1 key string reference is invalid.");
                }

                var text = ReadStringAt(_pool + (int)reference - 1);
                _labels[Fnv1A(text)] = text;
            }
        }

        private string DescribeNode(int node)
        {
            return ReadType(node) switch
            {
                8 => $"uint32 {ReadValue(node)}",
                14 => $"string {ReadString(node)}",
                20 => $"object ({_edges.GetValueOrDefault(node)?.Count ?? 0} fields)",
                21 => $"array raw 0x{ReadValue(node):x8}",
                _ => $"type {ReadType(node)}, raw 0x{ReadValue(node):x8}"
            };
        }

        private string ReadString(int node)
        {
            var reference = ReadValue(node);
            if (reference == 0 || reference >= _poolSize)
            {
                throw new InvalidDataException("BDB1 string reference is invalid.");
            }

            return ReadStringAt(_pool + (int)reference - 1);
        }

        private string ReadStringAt(int offset)
        {
            var length = ReadUInt16(offset);
            if (length > _data.Length - offset - 2)
            {
                throw new InvalidDataException("BDB1 string exceeds the pool.");
            }

            try
            {
                return Encoding.UTF8.GetString(_data, offset + 2, length);
            }
            catch (DecoderFallbackException ex)
            {
                throw new InvalidDataException("BDB1 string is not valid UTF-8.", ex);
            }
        }

        private int ReadCount(int fieldOffset, int width, out int start)
        {
            var offset = ReadUInt32(fieldOffset);
            var count = ReadUInt32(fieldOffset + 4);
            if (count > int.MaxValue / width)
            {
                throw new InvalidDataException("BDB1 section is too large.");
            }

            start = checked(fieldOffset + (int)offset);
            var length = checked((int)count * width);
            if (start < HeaderSize || start > _data.Length - length)
            {
                throw new InvalidDataException("BDB1 section is outside the payload.");
            }

            return checked((int)count);
        }

        private byte ReadType(int node) => _data[_types + node];
        private uint ReadValue(int node) => ReadUInt32(_values + node * 4);
        private uint ReadUInt32(int offset) => BinaryPrimitives.ReadUInt32LittleEndian(_data.AsSpan(offset, 4));
        private ushort ReadUInt16(int offset) => BinaryPrimitives.ReadUInt16LittleEndian(_data.AsSpan(offset, 2));

        private static uint Fnv1A(string value)
        {
            var hash = 2166136261u;
            foreach (var character in Encoding.UTF8.GetBytes(value))
            {
                hash = (hash ^ character) * 16777619u;
            }

            return hash;
        }
    }
}
