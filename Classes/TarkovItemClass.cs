using System;
using System.Collections.Generic;
using System.Text;

namespace PocketTarkov
{
    public class TarkovItemClass
    {           
        public string uid { get; set; }
        public string bsgId { get; set; }
        public string name { get; set; }
        public string shortName { get; set; }
        public int price { get; set; }
        public int avg24hPrice { get; set; }
        public int avg7daysPrice { get; set; }
        public string traderName { get; set; }
        public int traderPrice { get; set; }
        public string traderPriceCur { get; set; }
        public DateTime updated { get; set; }
        public int slots { get; set; }
        public double diff24h { get; set; }
        public double diff7days { get; set; }
        public string icon { get; set; }
        public string link { get; set; }
        public string wikiLink { get; set; }
        public string img { get; set; }
        public string imgBig { get; set; }
        public string reference { get; set; }
    }
}

