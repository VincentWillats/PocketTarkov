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
        static string apiString = "https://pockettarkovapi-managementservice.azure-api.net/PocketTarkovAPI/";

        public static List<TarkovItemClass> allItemsNames = new List<TarkovItemClass>();
        public static List<TarkovItemClass> matchingItems = new List<TarkovItemClass>();

        static TarkovItemController()
        {
            httpclient.BaseAddress = new Uri(apiString);
            httpclient.DefaultRequestHeaders.Accept.Clear();    

            httpclient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", KeysAPI.OcpApimSubscriptionKey);
        }

        public static List<TarkovItemClass> GetMatchingItems(string searchTerm)
        {
            matchingItems = new List<TarkovItemClass>();
            foreach(TarkovItemClass item in allItemsNames)
            {
                if(item.name.ToLower().Contains(searchTerm.ToLower()) || item.shortName.ToLower().Contains(searchTerm.ToLower()))
                {
                    matchingItems.Add(item);
                }
            }
            return matchingItems;
        }

        public static async Task GetAllItemNamesListAsync()
        {            
            try
            {       
                // Get the response.
                HttpResponseMessage response = await httpclient.GetAsync("GetAllItemData");
                response.EnsureSuccessStatusCode();
                
                string responseContent = await response.Content.ReadAsStringAsync();
                allItemsNames = JsonConvert.DeserializeObject<List<TarkovItemClass>>(responseContent);
                System.Diagnostics.Debug.WriteLine("Items pulled successfully.");
            }
            catch (Exception ea)
            {
                System.Diagnostics.Debug.WriteLine("Error pulling items: " + ea.Message);
            }
        }
    }
}
