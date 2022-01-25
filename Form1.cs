using FtpDownloader.Partial;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FtpDownloader
{
    public partial class Form1 : Form, Callback
    {
        NetworkCredential credentials;

        public Form1()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            btnExit.Visible = false;
        }

        private void form_shown(object sender, EventArgs e)
        {
            if (!DotEnv.Load("env.txt"))
            {
                return;
            }

            var ftp_user = DotEnv.getFtpUsername();
            var ftp_pass = DotEnv.getFtpPassword();
            var ftp_url = DotEnv.getFtpUrl();

            if (ftp_user == null || ftp_pass == null || ftp_url == null)
            {
                return;
            }

            if (!Common.CheckForERPConnection())
            {
                MessageBox.Show("Server is not accessible", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            credentials = new NetworkCredential(ftp_user, ftp_pass);

            WebApi webApi = new WebApi();
            webApi.getAttendanceUpdateDetail(this);
        }

        private void updateListBox(string msg)
        {
            if (listInfo.InvokeRequired)
            {
                listInfo.BeginInvoke((MethodInvoker)delegate ()
                {
                    listInfo.Items.Add(msg);
                    listInfo.Refresh();
                    int itemsPerPage = (int)(listInfo.Height / listInfo.ItemHeight);
                    listInfo.TopIndex = listInfo.Items.Count - itemsPerPage;
                });
            }
            else
            {
                listInfo.Items.Add(msg);
                listInfo.Refresh();
                int itemsPerPage = (int)(listInfo.Height / listInfo.ItemHeight);
                listInfo.TopIndex = listInfo.Items.Count - itemsPerPage;
            }
            
        }

        public void onSuccess(Partial.WebResponse response)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate ()
                {
                    if (response.status != "1")
                    {
                        MessageBox.Show(response.msg, "Api Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var current_version = DotEnv.getVersion();
                    if (response.version == current_version)
                    {
                        updateListBox("Current Application Version is " + current_version + " is same as server");
                        btnExit.Visible = true;
                        return;
                    }

                    if (response.ftp_path_list.Count == 0)
                    {
                        updateListBox("Server did not return any FTP Path");
                        btnExit.Visible = true;
                        return;
                    }

                    updateListBox("Starting Download Files From FTP");

                    ThreadStart ts = delegate
                    {
                        try
                        {
                            foreach (var ftp_path in response.ftp_path_list)
                            {
                                _downloadFtpDirectory(DotEnv.getFtpUrl() + ftp_path + "/");
                            }
                        }
                        catch (Exception ex)
                        {
                            updateListBox(ex.Message);
                        }

                        updateListBox("Update Finish");

                        if (btnExit.InvokeRequired)
                        {
                            btnExit.BeginInvoke((MethodInvoker)delegate ()
                            {
                                btnExit.Visible = true;
                            });
                        }
                        else
                        {
                            btnExit.Visible = true;
                        }
                    };

                    new Thread(ts).Start();
                });
            }
        }

        private void _downloadFtpDirectory(string url)
        {
            updateListBox("Fetching " + url);
            
            FtpWebRequest listRequest = (FtpWebRequest)WebRequest.Create(url);
            listRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            listRequest.Credentials = credentials;

            List<string> lines = new List<string>();

            try
            {
                using (var listResponse = (FtpWebResponse)listRequest.GetResponse())
                {
                    using (Stream listStream = listResponse.GetResponseStream())
                    {
                        using (var listReader = new StreamReader(listStream))
                        {
                            while (!listReader.EndOfStream)
                            {
                                var s = listReader.ReadLine();
                                lines.Add(s);

                                string[] tokens = s.Split(new[] { ' ' }, 9, StringSplitOptions.RemoveEmptyEntries);
                                updateListBox(tokens[8]);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                updateListBox(ex.Message);
            }


            var localPath = DotEnv.getLocalPath();

            if (!Directory.Exists(localPath))
            {
                Directory.CreateDirectory(localPath);
            }

            foreach (string line in lines)
            {
                string[] tokens =
                    line.Split(new[] { ' ' }, 9, StringSplitOptions.RemoveEmptyEntries);
                string name = tokens[8];
                string permissions = tokens[0];

                string localFilePath = Path.Combine(localPath, name);
                string fileUrl = url + name;

                if (permissions[0] == 'd')
                {
                    if (!Directory.Exists(localFilePath))
                    {
                        Directory.CreateDirectory(localFilePath);
                    }

                    _downloadFtpDirectory(fileUrl + "/");
                }
                else
                {
                    Common.deleteFile(localFilePath);

                    updateListBox("Downloadling File : " + fileUrl);

                    FtpWebRequest downloadRequest = (FtpWebRequest)WebRequest.Create(fileUrl);
                    downloadRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                    downloadRequest.Credentials = credentials;

                    try
                    {
                        using (FtpWebResponse downloadResponse = (FtpWebResponse)downloadRequest.GetResponse())
                        {
                            using (Stream sourceStream = downloadResponse.GetResponseStream())
                            {
                                using (Stream targetStream = File.Create(localFilePath))
                                {
                                    byte[] buffer = new byte[10240];
                                    int read;
                                    while ((read = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                                    {
                                        targetStream.Write(buffer, 0, read);
                                    }

                                    var size_in_mb = Math.Round((double)targetStream.Length / 1024);

                                    updateListBox("Download File : " + localFilePath + " ( " + size_in_mb + " kb )");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        updateListBox(ex.Message);
                    }
                }
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void form_activated(object sender, EventArgs e)
        {
        }
    }

    
}
