using System;
using System.Collections.Generic;
using System.Text;

namespace POATax.DataStructures
{
    public class Transactions
    {
        public int id { get; set; }
        public string address { get; set; }
        public decimal transactionAmtPOA { get; set; }
        public DateTime date { get; set; }
    }
}
