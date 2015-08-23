namespace Pulse.OpenGL
{
    /// <summary>	
    /// Resource data formats which includes fully-typed and typeless formats. There is a list of format modifiers at the bottom of the page,  that more fully describes each format type.	
    /// http://msdn.microsoft.com/en-us/library/windows/desktop/bb173059%28v=vs.85%29.aspx
    /// </summary>	
    /// <remarks>	
    /// A few formats have additional restrictions.A resource declared with the DXGI_FORMAT_R32G32B32 family of formats cannot be used simultaneously for vertex and texture data. That is, you may not create a buffer resource with the DXGI_FORMAT_R32G32B32 family of formats that uses any of the following bind flags: D3D10_BIND_VERTEX_BUFFER, D3D10_BIND_INDEX_BUFFER, D3D10_BIND_CONSTANT_BUFFER, or D3D10_BIND_STREAM_OUTPUT (see {{D3D10_BIND_FLAG}}).DXGI_FORMAT_R1_UNORM is designed specifically for text filtering, and must be used with a format-specific, configurable 8x8 filter mode. When calling an HLSL sampling function using this format, the address offset parameter must be set to (0,0).A resource using a sub-sampled format (such as DXGI_FORMAT_R8G8_B8G8) must have a size that is a multiple of 2 in the x dimension.Format is not available in Direct3D 10 and Direct3D 10.1Format ModifiersEach enumeration value contains a format modifier which describes the data type.Format ModifiersDescription_FLOATA floating-point value; 32-bit floating-point formats use IEEE 754 single-precision (s23e8 format): sign bit, 8-bit biased (127) exponent,  and 23-bit mantissa. 16-bit floating-point formats use half-precision (s10e5 format): sign bit, 5-bit biased (15) exponent, and 10-bit mantissa._SINTTwo's complement signed integer. For example, a 3-bit SINT represents the values -4, -3, -2, -1, 0, 1, 2, 3._SNORMSigned normalized integer; which is interpreted in a resource as a signed integer, and is interpreted in a shader as a signed normalized floating-point value in the range [-1, 1]. For an 2's complement number, the maximum value is 1.0f (a 5-bit value 01111 maps to 1.0f, and the minimum value is -1.0f (a 5-bit value 10000 maps to -1.0f). In addition, the second-minimum number maps to -1.0f (a 5-bit value 10001 maps to -1.0f). The resulting integer representations are evenly spaced floating-point values in the range (-1.0f...0.0f, and also a complementary set of representations for numbers in the range (0.0f...1.0f)._SRGBStandard RGB data, which roughly displays colors in a linear ramp of luminosity levels such that an average observer, under average viewing conditions, can view them on an average display.  All 0's maps to 0.0f, and all 1's maps to 1.0f. The sequence of unsigned integer encodings between all 0's and all 1's represent a nonlinear progression in the floating-point interpretation of the numbers between 0.0f to 1.0f. For more detail, see the SRGB color standard, IEC 61996-2-1, at IEC (International Electrotechnical Commission). Conversion to or from sRGB space is automatically done by D3DX10 or D3DX9 texture-load functions. If the format has an alpha channel, the alpha data is also stored in sRGB color space. _TYPELESSTypeless data, with a defined number of bits. Typeless formats are designed for creating typeless resources; that is, a resource whose size is known, but whose data type is not yet fully defined. When a typeless resource is bound to a shader, the application or shader must resolve the format type (which must match the number of bits per component in the typeless format). A typeless format contains one or more subformats; each subformat resolves the data type. For example, in the R32G32B32 group, which defines types for three-component 96-bit data, there is one typeless format and three fully typed subformats.	
    /// <code> DXGI_FORMAT_R32G32B32_TYPELESS, DXGI_FORMAT_R32G32B32_FLOAT, DXGI_FORMAT_R32G32B32_UINT, DXGI_FORMAT_R32G32B32_SINT, </code>	
    /// 	
    /// _UINTUnsigned integer. For instance, a 3-bit UINT represents the values 0, 1, 2, 3, 4, 5, 6, 7._UNORMUnsigned normalized integer; which is interpreted in a resource as an unsigned integer, and is interpreted in a shader as an unsigned normalized floating-point value in the range [0, 1]. All 0's maps to 0.0f, and all 1's maps to 1.0f. A sequence of evenly spaced floating-point values from 0.0f to 1.0f are represented. For instance, a 2-bit UNORM represents 0.0f, 1/3, 2/3, and 1.0f. New Resource FormatsDirect3D 10 offers new data compression formats for compressing high-dynamic range (HDR) lighting data, normal maps and heightfields to a fraction of their original size. These compression types include:Shared-Exponent high-dynamic range (HDR) format (RGBE)New Block-Compressed 1-2 channel UNORM/SNORM formatsThe block compression formats can be used for any of the 2D or 3D texture types ( Texture2D, Texture2DArray, Texture3D, or TextureCube) including mipmap surfaces. The block compression techniques require texture dimensions to be a multiple of 4 (since the implementation compresses on blocks of 4x4 texels). In the texture sampler, compressed formats are always decompressed before texture filtering.	
    /// </remarks>	
    /// <unmanaged>DXGI_FORMAT</unmanaged>
    public enum DXGIFormat
    {
        /// <summary>	
        /// The format is not known.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_UNKNOWN</unmanaged>
        Unknown = 0,

        /// <summary>	
        /// A four-component, 128-bit typeless format. 1	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R32G32B32A32_TYPELESS</unmanaged>
        R32G32B32A32_Typeless = 1,

        /// <summary>	
        /// A four-component, 128-bit floating-point format. 1	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R32G32B32A32_FLOAT</unmanaged>
        R32G32B32A32_Float = 2,

        /// <summary>	
        /// A four-component, 128-bit unsigned-integer format. 1	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R32G32B32A32_UINT</unmanaged>
        R32G32B32A32_UInt = 3,

        /// <summary>	
        /// A four-component, 128-bit signed-integer format. 1	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R32G32B32A32_SINT</unmanaged>
        R32G32B32A32_SInt = 4,

        /// <summary>	
        /// A three-component, 96-bit typeless format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R32G32B32_TYPELESS</unmanaged>
        R32G32B32_Typeless = 5,

        /// <summary>	
        /// A three-component, 96-bit floating-point format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R32G32B32_FLOAT</unmanaged>
        R32G32B32_Float = 6,

        /// <summary>	
        /// A three-component, 96-bit unsigned-integer format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R32G32B32_UINT</unmanaged>
        R32G32B32_UInt = 7,

        /// <summary>	
        /// A three-component, 96-bit signed-integer format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R32G32B32_SINT</unmanaged>
        R32G32B32_SInt = 8,

        /// <summary>	
        /// A four-component, 64-bit typeless format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R16G16B16A16_TYPELESS</unmanaged>
        R16G16B16A16_Typeless = 9,

        /// <summary>	
        /// A four-component, 64-bit floating-point format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R16G16B16A16_FLOAT</unmanaged>
        R16G16B16A16_Float = 10,

        /// <summary>	
        /// A four-component, 64-bit unsigned-integer format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R16G16B16A16_UNORM</unmanaged>
        R16G16B16A16_UNorm = 11,

        /// <summary>	
        /// A four-component, 64-bit unsigned-integer format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R16G16B16A16_UINT</unmanaged>
        R16G16B16A16_UInt = 12,

        /// <summary>	
        /// A four-component, 64-bit signed-integer format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R16G16B16A16_SNORM</unmanaged>
        R16G16B16A16_SNorm = 13,

        /// <summary>	
        /// A four-component, 64-bit signed-integer format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R16G16B16A16_SINT</unmanaged>
        R16G16B16A16_SInt = 14,

        /// <summary>	
        /// A two-component, 64-bit typeless format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R32G32_TYPELESS</unmanaged>
        R32G32_Typeless = 15,

        /// <summary>	
        /// A two-component, 64-bit floating-point format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R32G32_FLOAT</unmanaged>
        R32G32_Float = 16,

        /// <summary>	
        /// A two-component, 64-bit unsigned-integer format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R32G32_UINT</unmanaged>
        R32G32_UInt = 17,

        /// <summary>	
        /// A two-component, 64-bit signed-integer format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R32G32_SINT</unmanaged>
        R32G32_SInt = 18,

        /// <summary>	
        /// A two-component, 64-bit typeless format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R32G8X24_TYPELESS</unmanaged>
        R32G8X24_Typeless = 19,

        /// <summary>	
        /// A 32-bit floating-point component, and two unsigned-integer components (with an additional 32 bits).	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_D32_FLOAT_S8X24_UINT</unmanaged>
        D32_Float_S8X24_UInt = 20,

        /// <summary>	
        /// A 32-bit floating-point component, and two typeless components (with an additional 32 bits).	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R32_FLOAT_X8X24_TYPELESS</unmanaged>
        R32_Float_X8X24_Typeless = 21,

        /// <summary>	
        /// A 32-bit typeless component, and two unsigned-integer components (with an additional 32 bits).	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_X32_TYPELESS_G8X24_UINT</unmanaged>
        X32_Typeless_G8X24_UInt = 22,

        /// <summary>	
        /// A four-component, 32-bit typeless format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R10G10B10A2_TYPELESS</unmanaged>
        R10G10B10A2_Typeless = 23,

        /// <summary>	
        /// A four-component, 32-bit unsigned-integer format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R10G10B10A2_UNORM</unmanaged>
        R10G10B10A2_UNorm = 24,

        /// <summary>	
        /// A four-component, 32-bit unsigned-integer format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R10G10B10A2_UINT</unmanaged>
        R10G10B10A2_UInt = 25,

        /// <summary>	
        /// Three partial-precision floating-point numbers encodeded into a single 32-bit value (a variant of s10e5).  There are no sign bits, and there is a 5-bit biased (15) exponent for each channel, 6-bit mantissa  for R and G, and a 5-bit mantissa for B, as shown in the following illustration.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R11G11B10_FLOAT</unmanaged>
        R11G11B10_Float = 26,

        /// <summary>	
        /// A three-component, 32-bit typeless format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R8G8B8A8_TYPELESS</unmanaged>
        R8G8B8A8_Typeless = 27,

        /// <summary>	
        /// A four-component, 32-bit unsigned-integer format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R8G8B8A8_UNORM</unmanaged>
        R8G8B8A8_UNorm = 28,

        /// <summary>	
        /// A four-component, 32-bit unsigned-normalized integer sRGB format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R8G8B8A8_UNORM_SRGB</unmanaged>
        R8G8B8A8_UNorm_SRgb = 29,

        /// <summary>	
        /// A four-component, 32-bit unsigned-integer format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R8G8B8A8_UINT</unmanaged>
        R8G8B8A8_UInt = 30,

        /// <summary>	
        /// A three-component, 32-bit signed-integer format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R8G8B8A8_SNORM</unmanaged>
        R8G8B8A8_SNorm = 31,

        /// <summary>	
        /// A three-component, 32-bit signed-integer format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R8G8B8A8_SINT</unmanaged>
        R8G8B8A8_SInt = 32,

        /// <summary>	
        /// A two-component, 32-bit typeless format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R16G16_TYPELESS</unmanaged>
        R16G16_Typeless = 33,

        /// <summary>	
        /// A two-component, 32-bit floating-point format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R16G16_FLOAT</unmanaged>
        R16G16_Float = 34,

        /// <summary>	
        /// A two-component, 32-bit unsigned-integer format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R16G16_UNORM</unmanaged>
        R16G16_UNorm = 35,

        /// <summary>	
        /// A two-component, 32-bit unsigned-integer format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R16G16_UINT</unmanaged>
        R16G16_UInt = 36,

        /// <summary>	
        /// A two-component, 32-bit signed-integer format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R16G16_SNORM</unmanaged>
        R16G16_SNorm = 37,

        /// <summary>	
        /// A two-component, 32-bit signed-integer format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R16G16_SINT</unmanaged>
        R16G16_SInt = 38,

        /// <summary>	
        /// A single-component, 32-bit typeless format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R32_TYPELESS</unmanaged>
        R32_Typeless = 39,

        /// <summary>	
        /// A single-component, 32-bit floating-point format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_D32_FLOAT</unmanaged>
        D32_Float = 40,

        /// <summary>	
        /// A single-component, 32-bit floating-point format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R32_FLOAT</unmanaged>
        R32_Float = 41,

        /// <summary>	
        /// A single-component, 32-bit unsigned-integer format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R32_UINT</unmanaged>
        R32_UInt = 42,

        /// <summary>	
        /// A single-component, 32-bit signed-integer format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R32_SINT</unmanaged>
        R32_SInt = 43,

        /// <summary>	
        /// A two-component, 32-bit typeless format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R24G8_TYPELESS</unmanaged>
        R24G8_Typeless = 44,

        /// <summary>	
        /// A 32-bit z-buffer format that uses 24 bits for the depth channel and 8 bits for the stencil channel.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_D24_UNORM_S8_UINT</unmanaged>
        D24_UNorm_S8_UInt = 45,

        /// <summary>	
        /// A 32-bit format, that contains a 24 bit, single-component, unsigned-normalized integer, with an additional typeless 8 bits.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R24_UNORM_X8_TYPELESS</unmanaged>
        R24_UNorm_X8_Typeless = 46,

        /// <summary>	
        /// A 32-bit format, that contains a 24 bit, single-component, typeless format,  with an additional 8 bit unsigned integer component.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_X24_TYPELESS_G8_UINT</unmanaged>
        X24_Typeless_G8_UInt = 47,

        /// <summary>	
        /// A two-component, 16-bit typeless format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R8G8_TYPELESS</unmanaged>
        R8G8_Typeless = 48,

        /// <summary>	
        /// A two-component, 16-bit unsigned-integer format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R8G8_UNORM</unmanaged>
        R8G8_UNorm = 49,

        /// <summary>	
        /// A two-component, 16-bit unsigned-integer format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R8G8_UINT</unmanaged>
        R8G8_UInt = 50,

        /// <summary>	
        /// A two-component, 16-bit signed-integer format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R8G8_SNORM</unmanaged>
        R8G8_SNorm = 51,

        /// <summary>	
        /// A two-component, 16-bit signed-integer format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R8G8_SINT</unmanaged>
        R8G8_SInt = 52,

        /// <summary>	
        /// A single-component, 16-bit typeless format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R16_TYPELESS</unmanaged>
        R16_Typeless = 53,

        /// <summary>	
        /// A single-component, 16-bit floating-point format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R16_FLOAT</unmanaged>
        R16_Float = 54,

        /// <summary>	
        /// A single-component, 16-bit unsigned-normalized integer format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_D16_UNORM</unmanaged>
        D16_UNorm = 55,

        /// <summary>	
        /// A single-component, 16-bit unsigned-integer format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R16_UNORM</unmanaged>
        R16_UNorm = 56,

        /// <summary>	
        /// A single-component, 16-bit unsigned-integer format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R16_UINT</unmanaged>
        R16_UInt = 57,

        /// <summary>	
        /// A single-component, 16-bit signed-integer format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R16_SNORM</unmanaged>
        R16_SNorm = 58,

        /// <summary>	
        /// A single-component, 16-bit signed-integer format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R16_SINT</unmanaged>
        R16_SInt = 59,

        /// <summary>	
        /// A single-component, 8-bit typeless format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R8_TYPELESS</unmanaged>
        R8_Typeless = 60,

        /// <summary>	
        /// A single-component, 8-bit unsigned-integer format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R8_UNORM</unmanaged>
        R8_UNorm = 61,

        /// <summary>	
        /// A single-component, 8-bit unsigned-integer format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R8_UINT</unmanaged>
        R8_UInt = 62,

        /// <summary>	
        /// A single-component, 8-bit signed-integer format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R8_SNORM</unmanaged>
        R8_SNorm = 63,

        /// <summary>	
        /// A single-component, 8-bit signed-integer format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R8_SINT</unmanaged>
        R8_SInt = 64,

        /// <summary>	
        /// A single-component, 8-bit unsigned-integer format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_A8_UNORM</unmanaged>
        A8_UNorm = 65,

        /// <summary>	
        /// A single-component, 1-bit unsigned-normalized integer format. 2.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R1_UNORM</unmanaged>
        R1_UNorm = 66,

        /// <summary>	
        /// Three partial-precision floating-point numbers encoded into a single 32-bit value all sharing the same 5-bit exponent (variant of s10e5).  There is no sign bit, and there is a shared 5-bit biased (15) exponent and a 9-bit mantissa for each channel, as shown in the following illustration. 2.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R9G9B9E5_SHAREDEXP</unmanaged>
        R9G9B9E5_Sharedexp = 67,

        /// <summary>	
        /// A four-component, 32-bit unsigned-normalized integer format. 3	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R8G8_B8G8_UNORM</unmanaged>
        R8G8_B8G8_UNorm = 68,

        /// <summary>	
        /// A four-component, 32-bit unsigned-normalized integer format. 3	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_G8R8_G8B8_UNORM</unmanaged>
        G8R8_G8B8_UNorm = 69,

        /// <summary>	
        /// Four-component typeless block-compression format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_BC1_TYPELESS</unmanaged>
        BC1_Typeless = 70,

        /// <summary>	
        /// Four-component block-compression format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_BC1_UNORM</unmanaged>
        BC1_UNorm = 71,

        /// <summary>	
        /// Four-component block-compression format for sRGB data.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_BC1_UNORM_SRGB</unmanaged>
        BC1_UNorm_SRgb = 72,

        /// <summary>	
        /// Four-component typeless block-compression format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_BC2_TYPELESS</unmanaged>
        BC2_Typeless = 73,

        /// <summary>	
        /// Four-component block-compression format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_BC2_UNORM</unmanaged>
        BC2_UNorm = 74,

        /// <summary>	
        /// Four-component block-compression format for sRGB data.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_BC2_UNORM_SRGB</unmanaged>
        BC2_UNorm_SRgb = 75,

        /// <summary>	
        /// Four-component typeless block-compression format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_BC3_TYPELESS</unmanaged>
        BC3_Typeless = 76,

        /// <summary>	
        /// Four-component block-compression format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_BC3_UNORM</unmanaged>
        BC3_UNorm = 77,

        /// <summary>	
        /// Four-component block-compression format for sRGB data.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_BC3_UNORM_SRGB</unmanaged>
        BC3_UNorm_SRgb = 78,

        /// <summary>	
        /// One-component typeless block-compression format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_BC4_TYPELESS</unmanaged>
        BC4_Typeless = 79,

        /// <summary>	
        /// One-component block-compression format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_BC4_UNORM</unmanaged>
        BC4_UNorm = 80,

        /// <summary>	
        /// One-component block-compression format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_BC4_SNORM</unmanaged>
        BC4_SNorm = 81,

        /// <summary>	
        /// Two-component typeless block-compression format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_BC5_TYPELESS</unmanaged>
        BC5_Typeless = 82,

        /// <summary>	
        /// Two-component block-compression format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_BC5_UNORM</unmanaged>
        BC5_UNorm = 83,

        /// <summary>	
        /// Two-component block-compression format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_BC5_SNORM</unmanaged>
        BC5_SNorm = 84,

        /// <summary>	
        /// A three-component, 16-bit unsigned-normalized integer format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_B5G6R5_UNORM</unmanaged>
        B5G6R5_UNorm = 85,

        /// <summary>	
        /// A four-component, 16-bit unsigned-normalized integer format that supports 1-bit alpha.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_B5G5R5A1_UNORM</unmanaged>
        B5G5R5A1_UNorm = 86,

        /// <summary>	
        /// A four-component, 32-bit unsigned-normalized integer format that supports 8-bit alpha.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_B8G8R8A8_UNORM</unmanaged>
        B8G8R8A8_UNorm = 87,

        /// <summary>	
        /// A four-component, 32-bit unsigned-normalized integer format.	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_B8G8R8X8_UNORM</unmanaged>
        B8G8R8X8_UNorm = 88,

        /// <summary>	
        /// A four-component, 32-bit format that supports 2-bit alpha. 4	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_R10G10B10_XR_BIAS_A2_UNORM</unmanaged>
        R10G10B10_Xr_Bias_A2_UNorm = 89,

        /// <summary>	
        /// A four-component, 32-bit typeless format that supports 8-bit alpha. 4	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_B8G8R8A8_TYPELESS</unmanaged>
        B8G8R8A8_Typeless = 90,

        /// <summary>	
        /// A four-component, 32-bit unsigned-normalized standard RGB format that supports 8-bit alpha. 4	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_B8G8R8A8_UNORM_SRGB</unmanaged>
        B8G8R8A8_UNorm_SRgb = 91,

        /// <summary>	
        /// A four-component, 32-bit typeless format. 4	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_B8G8R8X8_TYPELESS</unmanaged>
        B8G8R8X8_Typeless = 92,

        /// <summary>	
        /// A four-component, 32-bit unsigned-normalized standard RGB format. 4	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_B8G8R8X8_UNORM_SRGB</unmanaged>
        B8G8R8X8_UNorm_SRgb = 93,

        /// <summary>	
        /// A typeless block-compression format. 4	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_BC6H_TYPELESS</unmanaged>
        BC6H_Typeless = 94,

        /// <summary>	
        /// A block-compression format. 4	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_BC6H_UF16</unmanaged>
        BC6H_Uf16 = 95,

        /// <summary>	
        /// A block-compression format. 4	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_BC6H_SF16</unmanaged>
        BC6H_Sf16 = 96,

        /// <summary>	
        /// A typeless block-compression format. 4	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_BC7_TYPELESS</unmanaged>
        BC7_Typeless = 97,

        /// <summary>	
        /// A block-compression format. 4	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_BC7_UNORM</unmanaged>
        BC7_UNorm = 98,

        /// <summary>	
        /// A block-compression format. 4	
        /// </summary>	
        /// <unmanaged>DXGI_FORMAT_BC7_UNORM_SRGB</unmanaged>
        BC7_UNorm_SRgb = 99,
        AYUV = 100,
        Y410 = 101,
        Y416 = 102,
        NV12 = 103,
        P010 = 104,
        P016 = 105,
        Opaque420 = 106,
        YUY2 = 107,
        Y210 = 108,
        Y216 = 109,
        NV11 = 110,
        AI44 = 111,
        IA44 = 112,
        P8 = 113,
        A8P8 = 114,
        B4G4R4A4_UNORM = 115,
        FORCE_UINT = -1
    }
}