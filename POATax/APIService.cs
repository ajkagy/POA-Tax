using System;
using System.Collections.Generic;
using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Threading;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace POATax
{
    public class APIService
    {

        public string GetCryptoCompareAverageDailyPrice(string symbol, long epochPrice)
        {
            try
            {
                APIService api = new APIService();
                string jsonResponse = api.CallRest("https://min-api.cryptocompare.com/data/dayAvg?fsym=" + symbol + "&tsym=USD&toTs=" + epochPrice + "&avgType=MidHighLow");
                dynamic cc_info = JObject.Parse(jsonResponse);
                return cc_info.USD;
                //   JToken data = cc_info.Data;
                // List<JToken> jtokList = data.Children().ToList<JToken>();

                //List<cc_ids> cc_idList = new List<cc_ids>();
                //for (int i = 0; i < jtokList.Count; i++)
                //{
                //    cc_ids obj = new cc_ids();
                //    JToken itm = jtokList[i];
                //    obj.new_id = itm.SelectToken("..Id").Value<string>();
                //    obj.symbol = ((Newtonsoft.Json.Linq.JProperty)itm).Name;

                //    cc_idList.Add(obj);
                //}


                //using (var dbContext = new TokenApexEntities())
                //{
                //    dbContext.Database.ExecuteSqlCommand("TRUNCATE TABLE cc_ids;");
                //    dbContext.cc_ids.AddRange(cc_idList);
                //    dbContext.SaveChanges();
                //}
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        private string CallRest(string url)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36";
                WebResponse response = req.GetResponse();
                Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.  
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.  
                string responseFromServer = reader.ReadToEnd();
                // Clean up the streams and the response.
                var data = responseFromServer;

                reader.Close();
                response.Close();
                //m = JsonConvert.DeserializeObject<cmc_ticker>(data);
                return data;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }

}
