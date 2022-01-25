using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace FtpDownloader.Partial
{
    public class WebApi
    {
        public WebApi()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }


        private void post(JObject json, Callback callback)
        {
            ThreadStart ts = delegate
            {
                String response = send(json);

                var obj = getJson(response);

                if (obj != null)
                {
                    callback.onSuccess(JsonConvert.DeserializeObject<WebResponse>(response));
                }
            };

            new Thread(ts).Start();
        }

        private String send(JObject json)
        {
            json.Add("is_web", 1);

            HttpClient client = new HttpClient();
            var post_params = new StringContent(json.ToString(), Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = client.PostAsync(DotEnv.getBaseUrl(), post_params).Result;

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var content = response.Content.ReadAsStringAsync().Result;                        
                        return content;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "Service Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }
                else
                {   
                    MessageBox.Show(response.ReasonPhrase, "Service Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return "";
        }


        public Object getJson(String response)
        {
            try
            {
                return JToken.Parse(response);
            }
            catch (JsonReaderException jex)
            {
                MessageBox.Show("Response is not valid JSON", "Service Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox.Show(response, "Service Response", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine(response);
                return null;
            }
        }

        public void getAttendanceUpdateDetail(Callback callback)
        {
            JObject json = new JObject();
            json.Add("service_name", "attendance_app_update_detail_get");
            json.Add("version", DotEnv.getVersion());
            post(json, callback);
        }
    }

    public interface Callback
    {
        void onSuccess(WebResponse response);
    }
}
