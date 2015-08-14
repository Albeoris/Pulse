namespace Pulse.Core.WinAPI
{
    /// <summary>
    /// Specifies the type of application that is described by the RM_PROCESS_INFO structure.
    /// <para>Unmanaged name: RM_APP_TYPE</para> 
    /// <see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/aa373670(v=vs.85).aspx"/>
    /// </summary>
    public enum RestartManagerAppType
    {
        /// <summary>
        /// The application cannot be classified as any other type. An application of this type can only be shut down by a forced shutdown.
        /// </summary>
        UnknownApp = 0,

        /// <summary>
        /// A Windows application run as a stand-alone process that displays a top-level window.
        /// </summary>
        MainWindow = 1,

        /// <summary>
        /// A Windows application that does not run as a stand-alone process and does not display a top-level window.
        /// </summary>
        OtherWindow = 2,

        /// <summary>
        /// The application is a Windows service.
        /// </summary>
        Service = 3,

        /// <summary>
        /// The application is Windows Explorer.
        /// </summary>
        Explorer = 4,

        /// <summary>
        /// The application is a stand-alone console application.
        /// </summary>
        Console = 5,

        /// <summary>
        /// A system restart is required to complete the installation because a process cannot be shut down.
        /// <para>The process cannot be shut down because of the following reasons.</para>
        /// <para>The process may be a critical process.</para>
        /// <para>The current user may not have permission to shut down the process.</para>
        /// <para>The process may belong to the primary installer that started the Restart Manager.</para>
        /// </summary>
        Critical = 1000
    }
}