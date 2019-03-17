using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using LiteDB;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using POATax.DataStructures;
namespace POATax
{
    public class Web3Core
    {

        public async Task<Int32> getAllTransactionsByAccount(IConfiguration Configuration)
        {
            int startBlock = 0;
            int lastBlock = 0;
            DateTime inception = new DateTime(2017, 12, 15);
            CultureInfo provider = CultureInfo.InvariantCulture;
            //Get Block Number from starting Date
            try
            {
                //Calculate approx last block
                DateTime lastDayOfTheYear = new DateTime(Convert.ToInt32(Configuration["Tax Year"]), 12, 31);
                double lastBlockDiff = (lastDayOfTheYear - inception).TotalDays;
                lastBlock = Convert.ToInt32(lastBlockDiff) * 17280;

                if (Configuration["Starting Date"] != "")
                {

                    DateTime dtStartDate = DateTime.ParseExact(Configuration["Starting Date"], "MM-dd-yyyy", provider);

                    double dayDifference = (dtStartDate - inception).TotalDays;

                    startBlock = Convert.ToInt32(dayDifference) * 17280;
                }
            }
            catch (Exception ex)
            {

            }

            WriteStatus("Starting Transaction Search...");
            List<Transaction> transactionList = new List<Transaction>();

            var web3 = new Nethereum.Web3.Web3(Configuration["RPC"]);
            var b = 1;
            HexBigInteger maxBlockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            using (var db = new LiteDatabase(@"POATax.db"))
            {
                // Get Transactions
                LiteCollection<POATax.DataStructures.Transactions> transactions = db.GetCollection<POATax.DataStructures.Transactions>("Transactions");

                //Override startblock based on last transaction date in DB
                var results = transactions.Find(Query.All("date", Query.Descending), limit: 1);
                DateTime dtStartDate = DateTime.ParseExact(Configuration["Starting Date"], "MM-dd-yyyy", provider);
                foreach (Transactions t in results)
                {
                    if (dtStartDate < t.date)
                    {
                        double dayDifference = (t.date - inception).TotalDays;
                        dayDifference = dayDifference - 1;
                        startBlock = Convert.ToInt32(dayDifference) * 17280;
                    }
                }



                if (startBlock == 0)
                {
                    startBlock = Convert.ToInt32(Configuration["Starting Block"]);
                }

                int accumulatedRewards = 0;
                DateTime CurrentDate = DateTime.MinValue;

                for (var i = startBlock; i <= maxBlockNumber.Value; i++)
                {
                    if (i % 10000 == 0)
                    {
                        WriteStatus("Processing Block  " + i + " of " + lastBlock + " approx.");
                    }

                    var block = await web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(new HexBigInteger(i));

                    if (block != null && block.Transactions != null)
                    {
                        DateTime dt = UnixTimeStampToDateTime((int)block.Timestamp.Value);

                        if (CurrentDate == DateTime.MinValue)
                        {
                            //Set Current Processing Block Date
                            CurrentDate = DateTime.Parse(dt.ToString("MM/dd/yyyy", provider));
                        }

                        if (block.Miner == Configuration["Mining Address"])
                        {
                            try
                            {
                                if (CurrentDate == DateTime.Parse(dt.ToString("MM/dd/yyyy", provider)))
                                {
                                    //Accumulate rewards for this day. We don't want to accumulate and record rewards outside of the tax year selected.
                                    if (Convert.ToInt32(Configuration["Tax Year"]) == dt.Year)
                                    {
                                        accumulatedRewards = accumulatedRewards + 1;
                                    }
                                }
                                else
                                {
                                    //New Day. Put accumulated rewards in the DB and reset
                                    if (Convert.ToInt32(Configuration["Tax Year"]) == dt.Year)
                                    {
                                        var find = transactions.Find(x => x.date == dt);
                                        if (find.Count() == 0)
                                        {
                                            var t = new POATax.DataStructures.Transactions
                                            {
                                                address = Configuration["Mining Address"],
                                                transactionAmtPOA = accumulatedRewards,
                                                date = CurrentDate
                                            };
                                            transactions.Insert(t);
                                        }
                                        WriteStatus("Processed Date: " + CurrentDate.ToString("MM/dd/yyyy", provider));
                                        CurrentDate = DateTime.Parse(dt.ToString("MM/dd/yyyy", provider));
                                        accumulatedRewards = 1;

                                    } else if  (Convert.ToInt32(Configuration["Tax Year"]) < dt.Year)
                                    {
                                        var t = new POATax.DataStructures.Transactions
                                        {
                                            address = Configuration["Mining Address"],
                                            transactionAmtPOA = accumulatedRewards,
                                            date = CurrentDate
                                        };
                                        transactions.Insert(t);
                                        //Last Day in the year, save and break out of current loop
                                        break;
                                    }
                                    
                                }

                            }
                            catch (Exception ex)
                            {

                            }

                        }

                    }
                }

            }
            return b;
        }

        private static DateTime UnixTimeStampToDateTime(int unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds((int)unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        private static void WriteStatus(string s)
        {
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, 8);
            Console.WriteLine("Status: " + s);
            Console.SetCursorPosition(0, 8);
        }

    }
}
