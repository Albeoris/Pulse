using System;
using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public class WdbHeader : WpdHeader
    {
        private const String StringEntryTag = "!!string";
        private const String StrTypeListEntryTag = "!!strtypelist";
        private const String TypeListEntryTag = "!!typelist";
        private const String VersionEntryTag = "!!version";
        protected const Int32 SpecialEntriesCount = 4;

        private byte[] _stringData;
        private int[] _strTypeList;
        private int[] _typeList;
        private uint _version;

        public override void ReadFromStream(Stream input)
        {
            base.ReadFromStream(input);
            new Deserializer(this, input).Deserialize();
        }

        public String GetString(int offset)
        {
            unsafe
            {
                fixed (byte* ptr = &_stringData[offset])
                    return new String((sbyte*)ptr);
            }
        }

        private sealed class Deserializer
        {
            private readonly WdbHeader _header;
            private readonly Stream _input;

            public Deserializer(WdbHeader header, Stream input)
            {
                _header = header;
                _input = input;
            }

            public void Deserialize()
            {
                if (_header.Entries == null)
                    return;

                for (int i = 0, s = SpecialEntriesCount; s > 0 && i < _header.Entries.Length; i++)
                {
                    WpdEntry entry = _header.Entries[i];
                    if (entry.NameWithoutExtension.StartsWith("!!"))
                    {
                        if (TryHandleSpecialEntry(entry))
                        {
                            s--;
                            continue;
                        }

                        Log.Warning("An unexpected special entry occurred: {0}", entry.Name);
                    }
                }
            }

            private Boolean TryHandleSpecialEntry(WpdEntry entry)
            {
                switch (entry.NameWithoutExtension)
                {
                    case StringEntryTag:
                        HandleString(entry);
                        break;
                    case StrTypeListEntryTag:
                        HandleStrTypeList(entry);
                        break;
                    case TypeListEntryTag:
                        HandleTypeList(entry);
                        break;
                    case VersionEntryTag:
                        HandleVersion(entry);
                        break;
                    default:
                        return false;
                }

                return true;
            }

            private void HandleString(WpdEntry entry)
            {
                _input.SetPosition(entry.Offset);
                _header._stringData = _input.EnsureRead(entry.Length);
            }

            private void HandleStrTypeList(WpdEntry entry)
            {
                if (entry.Length % 4 != 0)
                    throw new InvalidDataException($"[HandleStrTypeList] Entry: {entry.Name}, Length: {entry.Length}, (entry.Length % 4 = {entry.Length % 4}) != 0");

                _input.SetPosition(entry.Offset);
                int[] data = _input.DungerousReadStructs<Int32>(entry.Length / 4);
                for (int i = 0; i < data.Length; i++)
                    data[i] = Endian.SwapInt32(data[i]);

                _header._strTypeList = data;
            }

            private void HandleTypeList(WpdEntry entry)
            {
                if (entry.Length % 4 != 0)
                    throw new InvalidDataException($"[HandleTypeList] Entry: {entry.Name}, Length: {entry.Length}, (entry.Length % 4 = {entry.Length % 4}) != 0");

                _input.SetPosition(entry.Offset);
                int[] data = _input.DungerousReadStructs<Int32>(entry.Length / 4);
                for (int i = 0; i < data.Length; i++)
                    data[i] = Endian.SwapInt32(data[i]);

                _header._typeList = data;
            }

            private void HandleVersion(WpdEntry entry)
            {
                if (entry.Length != 4)
                    throw new InvalidDataException($"[HandleVersion] Entry: {entry.Name}, Length: {entry.Length}, (entry.Length = {entry.Length}) != 4");

                _input.SetPosition(entry.Offset);
                byte[] buff = _input.EnsureRead(4);

                _header._version = Endian.ToBigUInt32(buff, 0);
            }
        }
    }
}