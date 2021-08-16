using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DAM2.Shared.Insights
{
    public class InsightsLogger
    {
        public class InsightsEvent
        {
            public string eid { get; set; }
            public string name { get; set; }
            public string tenant { get; set; }
            public string msg { get; set; }
            public string pid { get; set; }
            public string cid { get; set; }
            public JObject props { get; set; }

        }

        static string url = "https://daminsights1.azurewebsites.net/api/post";
        static readonly HttpClient _client = new HttpClient();

        public static void Post(InsightsEvent e)
        {
            try
            {

                JObject jdata = new JObject();
                jdata["eid"] = e.eid;
                jdata["name"] = e.name;
                jdata["tenant"] = e.tenant;
                jdata["msg"] = e.msg;
                jdata["pid"] = e.pid;
                jdata["cid"] = e.cid;
                jdata["token"] = "VgHxbcrrLAyPoksuJ3Nykifp";
                if (e.props != null)
                {
                    jdata["props"] = e.props;
                }

                string jsonInString = jdata.ToString(Newtonsoft.Json.Formatting.None);
                Task t = _client.PostAsync(url, new StringContent(jsonInString, Encoding.UTF8, "application/json"));
            }

            catch (HttpRequestException ex)
            {
                Console.WriteLine("Message :{0} ", ex.Message);
            }
        }
    }
}
