using System;
using System.ComponentModel;

namespace Pulse.Core.WinAPI
{
    public static class Win32ErrorExm
    {
        public static void Check(this Win32Error self, string formatMessage = null, params object[] args)
        {
            if (self == Win32Error.ERROR_SUCCESS)
                return;

            Win32Exception ex = self.GetException();
            if (formatMessage == null)
                throw ex;

            throw new Exception(String.Format(formatMessage, args), ex);
        }

        public static void Throw(this Win32Error self)
        {
            throw GetException(self);
        }

        public static Win32Exception GetException(this Win32Error self)
        {
            return new Win32Exception((int)self);
        }

        public static int MakeHResult(this Win32Error self)
        {
            return unchecked((int)0x80070000 | (int)self);
        }
    }
}