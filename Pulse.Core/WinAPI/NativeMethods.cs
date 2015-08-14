using System;
using System.Runtime.InteropServices;
using Pulse.Core.WinAPI;

namespace Pulse.Core
{
    public static class NativeMethods
    {
        [DllImport("User32.dll", SetLastError = true)]
        public static extern int SetWindowRgn(IntPtr handle, IntPtr region, bool redraw);

        [DllImport("gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject(IntPtr handle);

        [DllImport("shell32.dll", SetLastError = true)]
        public static extern IntPtr SHGetFileInfo(string path, uint attributes, ref ShellFileSystemInfo info, int fileInfoSize, SHGetFileInfoFlags flags);

        #region Restart Manager

        /// <summary>
        /// Starts a new Restart Manager session.
        /// <para>A maximum of 64 Restart Manager sessions per user session can be open on the system at the same time.</para>
        /// <para>When this function starts a session, it returns a session handle and session key that can be used in subsequent calls to the Restart Manager API.</para>
        /// <see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/aa373668(v=vs.85).aspx"/>
        /// <remarks>The RmStartSession function returns an error if a session with the same session key already exists.
        /// The RmStartSession function should be called by the primary installer that controls the user interface or that controls the installation sequence of multiple patches in an update.
        /// A secondary installer can join an existing Restart Manager session by calling the RmJoinSession function with the session handle and session key returned from the RmStartSession function call of the primary installer.</remarks>
        /// </summary>
        /// <param name="sessionHandle">A pointer to the handle of a Restart Manager session. The session handle can be passed in subsequent calls to the Restart Manager API.</param>
        /// <param name="sessionFlags">Reserved. This parameter should be 0.</param>
        /// <param name="sessionKey">A null-terminated string that contains the session key to the new session. The string must be allocated before calling the RmStartSession function.</param>
        /// <returns>This is the most recent error received. The function can return one of the system error codes that are defined in Winerror.h.
        /// <para>SUCCESS - The resources specified have been registered.</para>
        /// <para>SEM_TIMEOUT - A Restart Manager function could not obtain a Registry write mutex in the allotted time. A system restart is recommended because further use of the Restart Manager is likely to fail.</para>
        /// <para>BAD_ARGUMENTS - One or more arguments are not correct. This error value is returned by the Restart Manager function if a NULL pointer or 0 is passed in a parameter that requires a non-null and non-zero value.</para>
        /// <para>MAX_SESSIONS_REACHED - The maximum number of sessions has been reached.</para>
        /// <para>WRITE_FAULT - The system cannot write to the specified device.</para>
        /// <para>OUTOFMEMORY - A Restart Manager operation could not complete because not enough memory was available.</para>
        /// </returns>
        [DllImport("rstrtmgr.dll", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern Win32Error RmStartSession(out uint sessionHandle, int sessionFlags, string sessionKey);

        /// <summary>
        /// Ends the Restart Manager session.
        /// <para>This function should be called by the primary installer that has previously started the session by calling the RmStartSession function.</para>
        /// <para>The RmEndSession function can be called by a secondary installer that is joined to the session once no more resources need to be registered by the secondary installer.</para>
        /// <see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/aa373659(v=vs.85).aspx"/>
        /// </summary>
        /// <param name="sessionHandle">A handle to an existing Restart Manager session.</param>
        /// <returns>This is the most recent error received. The function can return one of the system error codes that are defined in Winerror.h.
        /// <para>SUCCESS - The resources specified have been registered.</para>
        /// <para>SEM_TIMEOUT - A Restart Manager function could not obtain a Registry write mutex in the allotted time. A system restart is recommended because further use of the Restart Manager is likely to fail.</para>
        /// <para>WRITE_FAULT - The system cannot write to the specified device.</para>
        /// <para>OUTOFMEMORY - A Restart Manager operation could not complete because not enough memory was available.</para>
        /// <para>INVALID_HANDLE - An invalid handle was passed to the function. No Restart Manager session exists for the handle supplied.</para>
        /// </returns>
        [DllImport("rstrtmgr.dll", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern Win32Error RmEndSession(uint sessionHandle);

        /// <summary>
        /// Registers resources to a Restart Manager session.
        /// <para>The Restart Manager uses the list of resources registered with the session to determine which applications and services must be shut down and restarted.</para>
        /// <para>Resources can be identified by filenames, service short names, or RM_UNIQUE_PROCESS structures that describe running applications.</para>
        /// <para>The RmRegisterResources function can be used by a primary or secondary installer.</para>
        /// <see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/aa373663(v=vs.85).aspx"/>
        /// <remarks>Each call to the RmRegisterResources function performs relatively expensive write operations.
        /// Do not call this function once per file, instead group related files together into components and register these together.</remarks>
        /// </summary>
        /// <param name="sessionHandle">A handle to an existing Restart Manager session.</param>
        /// <param name="filesCount">The number of files being registered.</param>
        /// <param name="fileNames">An array of null-terminated strings of full filename paths. This parameter can be NULL if filesCount is 0.</param>
        /// <param name="applicationsCount">The number of processes being registered.</param>
        /// <param name="applications">An array of RM_UNIQUE_PROCESS structures. This parameter can be NULL if applicationCount is 0.</param>
        /// <param name="servicesCount">The number of services to be registered.</param>
        /// <param name="serviceNames">An array of null-terminated strings of service short names. This parameter can be NULL if nServices is 0.</param>
        /// <returns>This is the most recent error received. The function can return one of the system error codes that are defined in Winerror.h.
        /// <para>SUCCESS - The resources specified have been registered.</para>
        /// <para>SEM_TIMEOUT - A Restart Manager function could not obtain a Registry write mutex in the allotted time. A system restart is recommended because further use of the Restart Manager is likely to fail.</para>
        /// <para>BAD_ARGUMENTS - One or more arguments are not correct. This error value is returned by Restart Manager function if a NULL pointer or 0 is passed in a parameter that requires a non-null and non-zero value.</para>
        /// <para>WRITE_FAULT - An operation was unable to read or write to the registry.</para>
        /// <para>OUTOFMEMORY - A Restart Manager operation could not complete because not enough memory was available.</para>
        /// <para>INVALID_HANDLE - No Restart Manager session exists for the handle supplied.</para>
        /// </returns>
        [DllImport("rstrtmgr.dll", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern Win32Error RmRegisterResources(uint sessionHandle, UInt32 filesCount, string[] fileNames, UInt32 applicationsCount, [In] RestartManagerUniqueProcess[] applications, UInt32 servicesCount, string[] serviceNames);

        /// <summary>
        /// Gets a list of all applications and services that are currently using resources that have been registered with the Restart Manager session.
        /// <see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/aa373661(v=vs.85).aspx"/>
        /// </summary>
        /// <param name="sessionHandle">A handle to an existing Restart Manager session.</param>
        /// <param name="procInfoNeeded">A pointer to an array size necessary to receive RM_PROCESS_INFO structures required to return information for all affected applications and services.</param>
        /// <param name="procInfo">A pointer to the total number of RM_PROCESS_INFO structures in an array and number of structures filled.</param>
        /// <param name="affectedApps">An array of RM_PROCESS_INFO structures that list the applications and services using resources that have been registered with the session.</param>
        /// <param name="rebootReasons">Pointer to location that receives a value of the RM_REBOOT_REASON enumeration that describes the reason a system restart is needed.</param>
        /// <returns>This is the most recent error received. The function can return one of the system error codes that are defined in Winerror.h.
        /// <para>SUCCESS - The resources specified have been registered.</para>
        /// <para>MORE_DATA - This error value is returned by the RmGetList function if the rgAffectedApps buffer is too small to hold all application information in the list.</para>
        /// <para>CANCELLED - The current operation is canceled by user.</para>
        /// <para>SEM_TIMEOUT - A Restart Manager function could not obtain a Registry write mutex in the allotted time. A system restart is recommended because further use of the Restart Manager is likely to fail.</para>
        /// <para>BAD_ARGUMENTS - One or more arguments are not correct. This error value is returned by Restart Manager function if a NULL pointer or 0 is passed in a parameter that requires a non-null and non-zero value.</para>
        /// <para>WRITE_FAULT - An operation was unable to read or write to the registry.</para>
        /// <para>OUTOFMEMORY - A Restart Manager operation could not complete because not enough memory was available.</para>
        /// <para>INVALID_HANDLE - No Restart Manager session exists for the handle supplied.</para>
        /// </returns>
        [DllImport("rstrtmgr.dll", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern Win32Error RmGetList(uint sessionHandle, out uint procInfoNeeded, ref uint procInfo, [In, Out] RestartManagerProcessInfo[] affectedApps, ref RestartManagerRebootReason rebootReasons);

        #endregion
    }
}