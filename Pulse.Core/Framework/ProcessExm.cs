using System;
using System.Diagnostics;
using System.Management;

namespace Pulse.Core.WinAPI
{
    public static class ProcessExm
    {
        public static String GetExecutablePath(this Process process)
        {
            try
            {
                return process.MainModule.FileName;
            }
            catch
            {
                return TryRequestExecutablePathSafe(process.Id);
            }
        }

        private static string TryRequestExecutablePathSafe(int processId)
        {
            try
            {
                string processIdStr = processId.ToString();
                const string query = "SELECT ExecutablePath, ProcessID FROM Win32_Process";
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);

                foreach (ManagementBaseObject o in searcher.Get())
                {
                    ManagementObject item = o as ManagementObject;
                    if (item == null)
                        continue;

                    object id = item["ProcessID"];
                    object path = item["ExecutablePath"];

                    if (path != null && id.ToString() == processIdStr)
                        return path.ToString();
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, Lang.Error.Process.CannotGetExecutablePath);
            }

            return string.Empty;
        }
    }
}