using System.Runtime.InteropServices;

namespace Pulse.Core.WinAPI
{
    /// <summary>
    /// Describes an application that is to be registered with the Restart Manager.
    /// <para>Unmanaged name: RM_PROCESS_INFO</para> 
    /// <see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/aa373674(v=vs.85).aspx"/>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct RestartManagerProcessInfo
    {
        private const int MaxAppNameLength = 255;
        private const int MaxServiceNameLength = 63;

        /// <summary>
        /// Contains an RM_UNIQUE_PROCESS structure that uniquely identifies the application by its PID and the time the process began.
        /// </summary>
        public RestartManagerUniqueProcess Process;

        /// <summary>
        /// If the process is a service, this parameter returns the long name for the service. If the process is not a service, this parameter returns the user-friendly name for the application.
        /// If the process is a critical process, and the installer is run with elevated privileges, this parameter returns the name of the executable file of the critical process.
        /// If the process is a critical process, and the installer is run as a service, this parameter returns the long name of the critical process.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxAppNameLength + 1)] public string ApplicationName;

        /// <summary>
        /// If the process is a service, this is the short name for the service. This member is not used if the process is not a service.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxServiceNameLength + 1)] public string ServiceShortName;

        /// <summary>
        /// Contains an RM_APP_TYPE enumeration value that specifies the type of application as RmUnknownApp, RmMainWindow, RmOtherWindow, RmService, RmExplorer or RmCritical.
        /// </summary>
        public RestartManagerAppType ApplicationType;

        /// <summary>
        /// Contains a bit mask that describes the current status of the application. See the RM_APP_STATUS enumeration.
        /// </summary>
        public RestartManagerAppStatus AppStatus;

        /// <summary>
        /// Contains the Terminal Services session ID of the process. If the terminal session of the process cannot be determined, the value of this member is set to RM_INVALID_SESSION (-1).
        /// This member is not used if the process is a service or a system critical process.
        /// </summary>
        public uint TerminalServiceSessionId;

        /// <summary>
        /// TRUE if the application can be restarted by the Restart Manager; otherwise, FALSE.
        /// <para>This member is always TRUE if the process is a service.</para>
        /// <para>This member is always FALSE if the process is a critical system process.</para>
        /// </summary>
        [MarshalAs(UnmanagedType.Bool)] public bool IsRestartable;
    }
}