using System;
using System.IO;

namespace CnCGenerals2EMU
{
    public static class BackendLog
    {
        public static string logFile = "BackendLog.txt";
        public static void Clear()
        {
            if (File.Exists(logFile))
                File.Delete(logFile);
        }

        public static void Write(string s)
        {
            File.AppendAllText(logFile, s);
        }
    }
}
