using System.Runtime.InteropServices;

namespace Pulse.Core.WinAPI
{
    /// <summary>
    /// Uniquely identifies a process by its PID and the time the process began. An array of RM_UNIQUE_PROCESS structures can be passed to the RmRegisterResources function.
    /// <para>Unmanaged name: RM_UNIQUE_PROCESS</para> 
    /// <see cref="http://msdn.microsoft.com/ru-RU/library/windows/desktop/aa373677(v=vs.85).aspx"/>
    /// <remarks>The RM_UNIQUE_PROCESS structure can be used to uniquely identify an application in an RM_PROCESS_INFO structure or registered with the Restart Manager session by the RmRegisterResources function.</remarks>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RestartManagerUniqueProcess
    {
        /// <summary>
        /// The product identifier (PID).
        /// </summary>
        public int ProcessId;

        /// <summary>
        /// The creation time of the process. The time is provided as a FILETIME structure that is returned by the lpCreationTime parameter of the GetProcessTimes function.
        /// </summary>
        public System.Runtime.InteropServices.ComTypes.FILETIME ProcessStartTime;
    }
}