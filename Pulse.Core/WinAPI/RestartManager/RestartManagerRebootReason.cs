namespace Pulse.Core.WinAPI
{
    /// <summary>
    /// Describes the reasons a restart of the system is needed.
    /// <para>Unmanaged name: RM_REBOOT_REASON</para> 
    /// <see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/aa373675(v=vs.85).aspx"/>
    /// </summary>
    public enum RestartManagerRebootReason : uint
    {
        None = 0x0,
        PermissionDenied = 0x1,
        SessionMismatch = 0x2,
        CriticalProcess = 0x4,
        CriticalService = 0x8,
        DetectedSelf = 0x10
    }
}