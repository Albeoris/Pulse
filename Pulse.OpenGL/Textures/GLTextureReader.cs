using System;
using System.IO;
using System.Linq;
using Examples.TextureLoaders;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.OpenGL
{
    public static class GLTextureReader
    {
        public static GLTexture ReadFromWpd(WpdArchiveListing listing, WpdEntry entry)
        {
            using (Stream headers = listing.Accessor.ExtractHeaders())
            using (Stream content = listing.Accessor.ExtractContent())
            {
                headers.SetPosition(entry.Offset);

                GtexData gtex;
                SectionHeader sectionHeader = headers.ReadContent<SectionHeader>();
                switch (sectionHeader.Type)
                {
                    case SectionType.Txb:
                        gtex = ReadGtexFromTxb(headers);
                        break;
                    case SectionType.Vtex:
                        gtex = ReadGtexFromVtex(headers);
                        break;
                    default:
                        throw new NotImplementedException();
                }

                if (gtex.Header.LayerCount == 0)
                    return null;

                int offset = 0;
                byte[] rawData = new byte[gtex.MipMapData.Sum(d => d.Length)];
                foreach (GtexMipMapLocation mimMap in gtex.MipMapData)
                {
                    using (StreamSegment textureInput = new StreamSegment(content, mimMap.Offset, mimMap.Length, FileAccess.Read))
                    {
                        textureInput.EnsureRead(rawData, offset, mimMap.Length);
                        offset += mimMap.Length;
                    }
                }

                using (GLService.AcquireContext())
                    return ImageDDS.LoadFromStream(rawData, gtex);
            }
        }

        private static GtexData ReadGtexFromTxb(Stream headers)
        {
            TextureHeader textureHeader = headers.ReadContent<TextureHeader>();
            return headers.ReadContent<GtexData>();

        }

        private static GtexData ReadGtexFromVtex(Stream headers)
        {
            VtexHeader textureHeader = headers.ReadContent<VtexHeader>();
            headers.Seek(textureHeader.GtexOffset - VtexHeader.Size, SeekOrigin.Current);
            return headers.ReadContent<GtexData>();
        }
    }
}