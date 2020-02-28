using System;
using System.Collections.Generic;
using System.Text;

namespace MLWLive.Stock
{
    public class Stock
    {
        public string Name { get; set; }

        public string Ticker { get; set; }

        public int Amount { get; set; }

        public decimal Price { get; set; }

        public DateTime LastPriceCheck { get; set; }
    }
}
