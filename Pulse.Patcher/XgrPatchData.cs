using System;
using System.Collections.Generic;
using System.IO;
using Pulse.Core;

namespace Pulse.Patcher
{
    public sealed class XgrPatchData : Dictionary<string, SafeUnmanagedArray>, IDisposable
    {
        public readonly string XgrArchiveName;

        public XgrPatchData(string xgrArchiveName, int count)
            : base(count)
        {
            XgrArchiveName = xgrArchiveName;
        }

        public void Dispose()
        {
            foreach (SafeUnmanagedArray value in Values)
                value.SafeDispose();
        }

        public static XgrPatchData ReadFrom(BinaryReader br)
        {
            string xgrArchiveName = br.ReadString();
            int count = br.ReadInt32();

            XgrPatchData result = new XgrPatchData(xgrArchiveName, count);

            try
            {
                for (int i = 0; i < count; i++)
                {
                    string fileName = br.ReadString();
                    int size = br.ReadInt32();
                    result[fileName] = br.BaseStream.ReadBuff(size);
                }
            }
            catch
            {
                result.SafeDispose();
                throw;
            }

            return result;
        }

        public static void WriteTo(string xgrArchiveName, string sourceDir, BinaryWriter bw)
        {
            bw.Write(xgrArchiveName);

            string[] files = Directory.GetFiles(sourceDir, "*", SearchOption.TopDirectoryOnly);
            bw.Write(files.Length);

            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                using (Stream input = File.OpenRead(file))
                {
                    bw.Write(fileName);
                    bw.Write((int)input.Length);
                    input.CopyTo(bw.BaseStream);
                }
            }
        }
    }
}