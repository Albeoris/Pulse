using System.Diagnostics;

namespace Pulse.Core.WinAPI
{
    public static class RestartManagerHelper
    {
        public static Process[] GetFileLockers(params string[] filePathes)
        {
            RestartManagerProcessInfo[] infos;
            using (RestartManager rm = new RestartManager())
            {
                rm.RegisterFiles(filePathes);
                infos = rm.GetList();
            }

            if (infos.IsNullOrEmpty())
                return new Process[0];

            Process[] result = new Process[infos.Length];
            for (int index = 0; index < infos.Length; index++)
            {
                RestartManagerProcessInfo info = infos[index];
                result[index] = Process.GetProcessById(info.Process.ProcessId);
            }
            return result;
        }
    }
}