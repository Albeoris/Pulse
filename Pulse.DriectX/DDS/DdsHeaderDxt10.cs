using System.Runtime.InteropServices;

namespace Pulse.DirectX
{
    /// <summary>
    /// DDS header extension to handle resource arrays, DXGI pixel formats that don't map to the legacy Microsoft DirectDraw pixel format structures, and additional metadata.
    /// http://msdn.microsoft.com/en-us/library/windows/desktop/bb943983%28v=vs.85%29.aspx
    /// <remarks>
    /// Use this structure together with a DDS_HEADER to store a resource array in a DDS file. For more info, see texture arrays.
    /// This header is present if the dwFourCC member of the DDS_PIXELFORMAT structure is set to 'DX10'.</remarks>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public sealed class DdsHeaderDxt10
    {
        /// <summary>
        /// The surface pixel format
        /// </summary>
        private DXGIFormat DxgiFormat;

        /// <summary>
        /// Identifies the type of resource. The following values for this member are a subset of the values in the D3D10_RESOURCE_DIMENSION or D3D11_RESOURCE_DIMENSION enumeration:
        /// </summary>
        private D3D10ResourceDimension ResourceDimension;

        /// <summary>
        /// Identifies other, less common options for resources. The following value for this member is a subset of the values in the D3D10_RESOURCE_MISC_FLAG or D3D11_RESOURCE_MISC_FLAG enumeration:
        /// </summary>
        private D3D10ResourceMisc MiscFlag;

        /// <summary>
        /// The number of elements in the array.
        /// <para>For a 2D texture that is also a cube-map texture, this number represents the number of cubes. This number is the same as the number in the NumCubes member of D3D10_TEXCUBE_ARRAY_SRV1 or D3D11_TEXCUBE_ARRAY_SRV).
        /// In this case, the DDS file contains arraySize*6 2D textures. For more information about this case, see the miscFlag description.</para>
        /// <para>For a 3D texture, you must set this number to 1.</para>
        /// </summary>
        private int ArraySize;

        /// <summary>
        /// Contains additional metadata (formerly was reserved). The lower 3 bits indicate the alpha mode of the associated resource. The upper 29 bits are reserved and are typically 0. 
        /// </summary>
        private DdsAlphaMode AlphaMode;
    }
}