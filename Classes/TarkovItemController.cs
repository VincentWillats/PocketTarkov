using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PocketTarkov
{ 
    public class TarkovItemController
    {
        static HttpClient httpclient = new HttpClient();
        static string tarkovMarket = "https://tarkov-market.com";

        public static List<TarkovItemClass> allItemsNames = new List<TarkovItemClass>();

        static TarkovItemController()
        {
            httpclient.BaseAddress = new Uri(tarkovMarket);
            httpclient.DefaultRequestHeaders.Accept.Clear();
            httpclient.DefaultRequestHeaders.Add("x-api-key", KeysAPI.tarkovMarketAPI);
        }

        public static async Task GetAllItemNamesListAsync()
        {            
            try
            {       
                // Get the response.
                HttpResponseMessage response = await httpclient.GetAsync("/api/v1/items/all");
                response.EnsureSuccessStatusCode();

                string responseContent = await response.Content.ReadAsStringAsync();
                allItemsNames = JsonConvert.DeserializeObject<List<TarkovItemClass>>(responseContent);              

            }
            catch (Exception ea)
            {
                System.Diagnostics.Debug.WriteLine("Error: " + ea.Message);
            }

        }

        public static async Task<TarkovItemClass> GetItemDetails(string itemName)
        {
            TarkovItemClass item = new TarkovItemClass();
            try
            {               
                // Get the response.
                HttpResponseMessage response = await httpclient.GetAsync("/api/v1/item?q=" + itemName);
                response.EnsureSuccessStatusCode();

                string responseContent = await response.Content.ReadAsStringAsync();
                item = JsonConvert.DeserializeObject<List<TarkovItemClass>>(responseContent)[0];
            }
            catch (Exception ea)
            {
                System.Diagnostics.Debug.WriteLine("Error: " + ea.Message);
            }
            return item;
        }
    }
}
