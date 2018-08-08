using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SimpleWallet
{
    public static class Comms
    {
        private static HttpClient client = new HttpClient();
        private static String SeedUrl = $"http://{Config.SeedNodes[0]}:14242/";

        public static async Task<byte[]> RequestBytes(String method, Dictionary<String, String> parameters)
        {
            string encodedParameters = WebUtility.UrlEncode(String.Join("&", parameters.Select(x => $"{x.Key}={x.Value}")));

            HttpResponseMessage response = await client.GetAsync($"{SeedUrl}{method}?{encodedParameters}");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            byte[] responseData = await response.Content.ReadAsByteArrayAsync();
            if (responseData.Length == 0)
            {
                return null;
            }

            return responseData;
        }

        public static async Task<String> RequestJson(String method, Dictionary<String, String> parameters)
        {
            string encodedParameters = String.Join("&", parameters.Select(x => $"{x.Key}={x.Value}"));

            HttpResponseMessage response = await client.GetAsync($"{SeedUrl}{method}?{encodedParameters}");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadAsStringAsync();
        }
    }
}
