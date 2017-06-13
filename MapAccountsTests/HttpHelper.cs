using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MapAccountsTests
{
    class HttpHelper
    {
        public String doPostString(String requestUri, String bodyParams = null)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var response = client.PostAsync(requestUri, new StringContent(bodyParams, Encoding.UTF8, "application/json")).Result;
                    return response.Content.ReadAsStringAsync().Result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return null;
                }
            }
        }
        public byte[] doPostByte(String requestUri)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var response = client.GetByteArrayAsync(requestUri).Result;
                    return response;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return null;
                }
            }
        }
    }
}
