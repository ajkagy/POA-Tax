using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;
using POATax.DataStructures;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Globalization;

namespace POATax
{
    class Program
    {
        public static IConfiguration Configuration { get; set; }
        public static void Main(string[] args)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            try
            {

                var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("settings.json");

                Configuration = builder.Build();

                //Validation on Config Items
                if (Configuration["RPC"] == "")
                {
                    throw new Exception("Error: No RPC Server Set");
                }
                if (Configuration["Starting Block"] == "")
                {
                    throw new Exception("Error: No Starting Block");
                }
                if (Configuration["Tax Year"] == "")
                {
                    throw new Exception("Error: No Tax Year Set");
                }
                if (Configuration["Mining Address"] == "")
                {
                    throw new Exception("Error: No Mining Address Set");
                }


                //Check Dates for Database and insert
                Web3Core w3 = new Web3Core();

                try
                {
                    DateTime StartDate = new DateTime(Convert.ToInt32(Configuration["Tax Year"]), 1, 1, 12, 0, 0);
                    DateTime EndDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0);
                    int DayInterval = 1;

                    List<DateTime> dateList = new List<DateTime>();
                    while (StartDate.AddDays(DayInterval) <= EndDate)
                    {
                        if (Convert.ToInt32(Configuration["Tax Year"]) == StartDate.Year)
                        {
                            StartDate = StartDate.AddDays(DayInterval);
                            dateList.Add(StartDate);
                        }
                        else
                        {
                            break;
                        }
                    }

                    List<DateTime> dbDatePrices = new List<DateTime>();
                    Console.WriteLine("Performing preliminary checks...");
                    Console.WriteLine("Updating POA Date/Prices from CryptoCompare...Please Wait");
                    using (var db = new LiteDatabase(@"POATax.db"))
                    {
                        // Get DatePrices
                        LiteCollection<DatePrices> datePrices = db.GetCollection<DatePrices>("DatePrices");

                        APIService api = new APIService();
                        for (int i = 0; i < dateList.Count; i++)
                        {

                            var find = datePrices.Find(x => x.date == dateList[i]);

                            if (find.Count() == 0)
                            {
                                long unixTime = ((DateTimeOffset)dateList[i]).ToUnixTimeSeconds();
                                //Get from Cryptocompare and Add to DB

                                string usd_value = api.GetCryptoCompareAverageDailyPrice("POA", unixTime);

                                var dp = new DatePrices
                                {
                                    symbol_from = "POA",
                                    symbol_to = "USD",
                                    price = Convert.ToDecimal(usd_value),
                                    date = dateList[i],
                                    epoch_date = (int)unixTime
                                };
                                datePrices.Insert(dp);
                                Console.SetCursorPosition(0, 3);
                                Console.WriteLine("Adding Date..." + dateList[i].ToString("MM/dd/yyyy", provider));

                                System.Threading.Thread.Sleep(500);
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occured reading the settings.json configuration.");
                    Console.ReadLine();
                }

                string xx = "";
                while (true)
                {
                    if (xx == "1")
                    {
                        Console.Clear();
                        WriteTitle();
                        Console.SetCursorPosition(0, 8);
                        w3.getAllTransactionsByAccount(Configuration).Wait();
                        Console.Write(new string(' ', Console.WindowWidth));
                        Console.SetCursorPosition(0, 8);
                        WorkSheetClass ws = new WorkSheetClass();
                        ws.GenerateExcelWorkSheet(Configuration["Mining Address"]);
                        Console.ReadLine();
                    }
                    else if (xx == "2")
                    {
                        WorkSheetClass ws = new WorkSheetClass();
                        ws.GenerateExcelWorkSheet(Configuration["Mining Address"]);
                        Console.ReadLine();
                    } else if (xx == "3")
                    {
                        Console.Clear();
                        using (var db = new LiteDatabase(@"POATax.db"))
                        {
                            db.DropCollection("Transactions");
                        }
                        WriteTitle();
                        Console.SetCursorPosition(0, 8);
                        Console.WriteLine("Transactions in database have been cleared. Press ENTER to continue.");
                        Console.ReadLine();
                    } else if (xx == "")
                    {

                    }
                    else
                    {
                        Console.WriteLine("Error: Invalid selection. Press ENTER to continue");
                        Console.SetCursorPosition(0, 11);
                        Console.Write(new string(' ', Console.WindowWidth));
                        Console.SetCursorPosition(0, 11);
                        Console.ReadLine();
                    }
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);
                    WriteTitle();
                    Console.WriteLine("Input a number from the Menu:");
                    Console.WriteLine("1. Run taxes based on settings.json");
                    Console.WriteLine("2. Output excel Doc");
                    Console.WriteLine("3. Delete data in database");
                    xx = Console.ReadLine();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }

        }

        private static void WriteTitle()
        {
            Console.Title = "POA Tax Estimator";
            string title = @"
  _____   ____            _______       __   __
 |  __ \ / __ \   /\     |__   __|/\    \ \ / /
 | |__) | |  | | /  \       | |  /  \    \ V / 
 |  ___/| |  | |/ /\ \      | | / /\ \    > <  
 | |    | |__| / ____ \     | |/ ____ \  / . \ 
 |_|     \____/_/    \_\    |_/_/    \_\/_/ \_\
                                               ";

            Console.WriteLine(title);
        }
    }
}
