using System;
using System.IO;
using System.Windows.Forms;

namespace FtpDownloader.Partial
{
    class DotEnv
    {
        public static bool Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show(filePath + " is not found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            foreach (var line in File.ReadAllLines(filePath))
            {
                var parts = line.Split(
                    '=',
                    (char)StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length != 2)
                    continue;

                Environment.SetEnvironmentVariable(parts[0], parts[1]);
            }

            return true;
        }

        private static void _showError(string msg)
        {
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static string getBaseUrl()
        {
            var s = Environment.GetEnvironmentVariable("API_URL");

            if (s == null)
            {
                _showError("API_URL is not found in env file");
            }

            return s;
        }

        public static string getPingUrl()
        {
            var s = Environment.GetEnvironmentVariable("PING_URL");

            if (s == null)
            {
                _showError("PING_URL is not found in env file");
            }

            return s;
        }

        public static string getVersion()
        {
            var s = Environment.GetEnvironmentVariable("VERSION");

            if (s == null)
            {
                _showError("VERSION is not found in env file");
            }

            return s;
        }

        public static string getFtpUrl()
        {
            var s = Environment.GetEnvironmentVariable("FTP_URL");

            if (s == null)
            {
                _showError("API_URL is not found in env file");
            }

            return s;
        }

        public static string getFtpUsername()
        {
            var s = Environment.GetEnvironmentVariable("FTP_USERNAME");

            if (s == null)
            {
                _showError("FTP_USERNAME is not found in env file");
            }

            return s;
        }

        public static string getFtpPassword()
        {
            var s = Environment.GetEnvironmentVariable("FTP_PASSWORD");

            if (s == null)
            {
                _showError("FTP_PASSWORD is not found in env file");                
            }

            return s;
        }

        public static string getLocalPath()
        {
            var s = Environment.GetEnvironmentVariable("LOCAL_PATH");

            if (s == null)
            {
                _showError("LOCAL_PATH is not found in env file");
            }

            return s;
        }
    }
}
