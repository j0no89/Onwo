using System;
using System.IO;

namespace Onwo
{
    public static class Logger
    {
        private static StreamWriter writer;
        private static string _logFilePath;

        public static string LogFilePath
        {
            get { return _logFilePath; }
            set
            {
                if (string.Equals(value, _logFilePath, StringComparison.InvariantCultureIgnoreCase))
                    return;
                _logFilePath = value;
                onLogFilePathChanged();
            }
        }

        private static void onLogFilePathChanged()
        {
            writer?.Dispose();
            if (string.IsNullOrEmpty(LogFilePath))
                writer = null;
            writer = new StreamWriter(LogFilePath);
        }
        static Logger()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            path = Path.Combine(path, "MyLog.txt");
            LogFilePath = path;
            IsEnabled = true;
        }
        public static bool IsEnabled { get; set; }
        public static void Write(string message)
        {
            if (!IsEnabled)
                return;
            writer.Write(message);
        }
        public static void WriteLine(string message)
        {
            if (!IsEnabled)
                return;
            writer.WriteLine(message);
        }
    }
}
