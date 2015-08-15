using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.WinAPI
{
    /// <summary>
    /// Describes the current status of an application that is acted upon by the Restart Manager.
    /// <para>Unmanaged name: RM_APP_STATUS</para> 
    /// <see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/aa373669(v=vs.85).aspx"/>
    /// <remarks>The constants of RM_APP_STATUS can be combined with OR operators. The combination describes the history of actions taken by Restart Manager on the application.</remarks>
    /// </summary>
    [Flags]
    public enum RestartManagerAppStatus
    {
        /// <summary>
        /// The application is in a state that is not described by any other enumerated state.
        /// </summary>
        Unknown = 0x0,

        /// <summary>
        /// The application is currently running.
        /// </summary>
        Running = 0x1,

        /// <summary>
        /// The Restart Manager has stopped the application.
        /// </summary>
        Stopped = 0x2,

        /// <summary>
        /// An action outside the Restart Manager has stopped the application.
        /// </summary>
        StoppedOther = 0x4,

        /// <summary>
        /// The Restart Manager has restarted the application.
        /// </summary>
        Restarted = 0x8,

        /// <summary>
        /// The Restart Manager encountered an error when stopping the application.
        /// </summary>
        ErrorOnStop = 0x10,

        /// <summary>
        /// The Restart Manager encountered an error when restarting the application.
        /// </summary>
        ErrorOnRestart = 0x20,

        /// <summary>
        /// Shutdown is masked by a filter.
        /// </summary>
        ShutdownMasked = 0x40,

        /// <summary>
        /// Restart is masked by a filter.
        /// </summary>
        RestartMasked = 0x80
    }
}