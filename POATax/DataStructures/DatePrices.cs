using System;
using System.Collections.Generic;
using System.Text;

namespace POATax.DataStructures
{
    public class DatePrices
    {
        public int id { get; set; }
        public string symbol_from { get; set; }
        public string symbol_to { get; set; }
        public decimal price { get; set; }
        public DateTime date { get; set; }
        public int epoch_date { get; set; }
    }
}
