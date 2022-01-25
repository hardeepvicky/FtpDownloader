using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace FtpDownloader.Partial
{
    public class MyWebClient : WebClient
    {        
        private int timeout { get; set; }

        public MyWebClient(int seconds = 10)
        {
            this.timeout = seconds * 60000;
        }        

        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest w = base.GetWebRequest(uri);
            w.Timeout = this.timeout;
            return w;
        }
    }

    class Common
    {   
        static string macAddr;

        public static bool CheckForERPConnection()
        {
            try
            {
                using (var client = new MyWebClient(3))
                {   
                    using (client.OpenRead(DotEnv.getPingUrl()))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                
            }

            return false;
        }

        public static string getMachineAddress()
        {
            if (macAddr == null)
            {
                macAddr =
                    (
                        from nic in NetworkInterface.GetAllNetworkInterfaces()
                        where nic.OperationalStatus == OperationalStatus.Up
                        select nic.GetPhysicalAddress().ToString()
                    ).FirstOrDefault();
            }

            return macAddr;
        }

        public static void deleteDirectoryFiles(String path, DateTime last_date)
        {
            var directory = new DirectoryInfo(path);            
            var files = directory.GetFiles().Where(file => file.LastWriteTime <= last_date);

            foreach (var file in files)
            {
                file.Delete();
            }
        }

        public static bool deleteFile(String file)
        {

            try
            {   
                if (File.Exists(file) )
                {   
                    File.Delete(file);
                    return true;
                }                
            }
            catch (IOException ioExp)
            {
                
            }

            return false;
        }
    }
}
