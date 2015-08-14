using System;

namespace Pulse.Core.WinAPI
{
    public sealed class RestartManager : IDisposable
    {
        private readonly uint _sessionHandle;

        public RestartManager()
        {
            string sessionKey = new Guid().ToString();
            Win32Error result = NativeMethods.RmStartSession(out _sessionHandle, 0, sessionKey);
            result.Check();
        }

        ~RestartManager()
        {
            NativeMethods.RmEndSession(_sessionHandle);
        }

        public void Dispose()
        {
            Win32Error error = NativeMethods.RmEndSession(_sessionHandle);
            if (error != Win32Error.ERROR_SUCCESS)
                Log.Error(error.GetException(), "[RestartManager] Failed to dispose.");
            GC.SuppressFinalize(this);
        }

        public void RegisterResources(string[] fileNames, RestartManagerUniqueProcess[] applications, string[] serviceNames)
        {
            uint fileCount = 0;
            if (fileNames != null)
            {
                fileCount = (uint)fileNames.Length;
                if (fileCount == 0)
                    fileNames = null;
            }

            uint applicationCount = 0;
            if (applications != null)
            {
                applicationCount = (uint)applications.Length;
                if (applicationCount == 0)
                    applications = null;
            }

            uint serviceCount = 0;
            if (serviceNames != null)
            {
                serviceCount = (uint)serviceNames.Length;
                if (serviceCount == 0)
                    serviceNames = null;
            }

            Win32Error result = NativeMethods.RmRegisterResources(_sessionHandle, fileCount, fileNames, applicationCount, applications, serviceCount, serviceNames);
            result.Check();
        }

        public void RegisterFiles(params string[] fileNames)
        {
            RegisterResources(fileNames, null, null);
        }

        public void RegisterApplications(params RestartManagerUniqueProcess[] applications)
        {
            RegisterResources(null, applications, null);
        }

        public void RegisterServices(params string[] serviceNames)
        {
            RegisterResources(null, null, serviceNames);
        }

        public RestartManagerProcessInfo[] GetList()
        {
            uint desiredCount;
            uint requestingCount = 0;
            RestartManagerProcessInfo[] infos = null;
            RestartManagerRebootReason rebootReason = RestartManagerRebootReason.None;

            Win32Error result = NativeMethods.RmGetList(_sessionHandle, out desiredCount, ref requestingCount, null, ref rebootReason);
            while (result == Win32Error.ERROR_MORE_DATA)
            {
                infos = new RestartManagerProcessInfo[desiredCount];
                requestingCount = desiredCount;

                result = NativeMethods.RmGetList(_sessionHandle, out desiredCount, ref requestingCount, infos, ref rebootReason);
            }

            if (result == Win32Error.ERROR_CANCELLED)
            {
                Log.Message("[RestartManager]Operation [GetList] cancelled by user.");
                return new RestartManagerProcessInfo[0];
            }

            result.Check();
            return infos;
        }
    }
}