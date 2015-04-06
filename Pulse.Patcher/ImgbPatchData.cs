using System;
using System.Collections.Generic;
using System.IO;
using Pulse.Core;

namespace Pulse.Patcher
{
    public sealed class ImgbPatchData : Dictionary<string, SafeUnmanagedArray>, IDisposable
    {
        public readonly string XgrArchiveUnpackName;

        public ImgbPatchData(string sourcePath, int count)
            : base(count)
        {
            XgrArchiveUnpackName = sourcePath;
        }

        public void Dispose()
        {
            foreach (SafeUnmanagedArray value in Values)
                value.SafeDispose();
        }

        public static ImgbPatchData ReadFrom(BinaryReader br)
        {
            string xgrArchiveName = br.ReadString();
            int count = br.ReadInt32();

            ImgbPatchData result = new ImgbPatchData(xgrArchiveName, count);

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

        public static void WriteTo(string sourcePath, string sourceDir, BinaryWriter bw)
        {
            bw.Write(sourcePath);

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