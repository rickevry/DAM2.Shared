using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DAM2.Shared.Insights
{
    public class InsightsLogger
    {
        public class InsightsEvent
        {
#pragma warning disable IDE1006
            public string eid { get; set; }
            public string name { get; set; }
            public string tenant { get; set; }
            public string msg { get; set; }
            public DateTime? created { get; set; }
            public string uid { get; set; }
            public string pid { get; set; }
            public string cid { get; set; }
            public string level { get; set; }
            public string operation { get; set; }
            public string source { get; set; }
            public JObject props { get; set; }
#pragma warning restore IDE1006
        }

        static readonly string url = "https://daminsights1.azurewebsites.net/api/post";
        static readonly HttpClient _client = new();
        static readonly CultureInfo culture = CultureInfo.InvariantCulture;

        public static void Post(InsightsEvent e)
        {
            try
            {

                var jdata = new JObject
                {
                    ["eid"] = e.eid,
                    ["name"] = e.name,
                    ["tenant"] = e.tenant,
                    ["msg"] = e.msg,
                    ["pid"] = e.pid,
                    ["cid"] = e.cid,
                    ["level"] = e.level,
                    ["uid"] = e.uid,
                    ["op"] = e.operation,
                    ["source"] = e.source ?? "SMH"
                };
                if (!e.created.HasValue)
                {
                    e.created = DateTime.UtcNow;
                }
                jdata["created"] = e.created.Value.ToString("o", culture);

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
