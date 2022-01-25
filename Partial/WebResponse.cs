using System;
using System.Collections.Generic;

namespace FtpDownloader.Partial
{
    public class WebResponse
    {
        public String service_name { get; set; }
        public String status { get; set; }
        public String msg { get; set; }
        public String version { get; set; }
        public List<String> ftp_path_list { get; set; }
    }
}
