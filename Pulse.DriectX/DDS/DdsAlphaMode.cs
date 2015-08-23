namespace Pulse.DirectX
{
    /// <summary>
    /// Contains additional metadata (formerly was reserved). The lower 3 bits indicate the alpha mode of the associated resource. The upper 29 bits are reserved and are typically 0.
    /// http://msdn.microsoft.com/en-us/library/windows/desktop/bb943983%28v=vs.85%29.aspx
    /// </summary>
    public enum DdsAlphaMode
    {
        /// <summary>
        /// Alpha channel content is unknown. This is the value for legacy files, which typically is assumed to be 'straight' alpha.
        /// </summary>
        Unknown = 0x0,

        /// <summary>
        /// Any alpha channel content is presumed to use straight alpha.
        /// </summary>
        Straight = 0x1,

        /// <summary>
        /// Any alpha channel content is using premultiplied alpha. The only legacy file formats that indicate this information are 'DX2' and 'DX4'.
        /// </summary>
        Premultiplied = 0x2,

        /// <summary>
        /// Any alpha channel content is all set to fully opaque.
        /// </summary>
        Opaque = 0x3,

        /// <summary>
        /// Any alpha channel content is being used as a 4th channel and is not intended to represent transparency (straight or premultiplied).
        /// </summary>
        Custom = 0x4
    }
}