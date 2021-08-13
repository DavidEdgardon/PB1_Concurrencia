using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Prueba1
{
    class Stock
    {
        public DateTime Date { get; set; }

        public decimal High { get; set; }

        public decimal Low { get; set; }

        public decimal Close { get; set; }
    }
    class Program
    {

        private static async Task<IEnumerable<Stock>> GetTickersAsync(string ticker)
        {
            var result = await _httpclient.GetStreamAsync($"https://stooq.com/q/d/l/?s={ticker}&i=d");
            using (var reader = new StreamReader(result))
            {
                using (var csvReader = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    MissingFieldFound = null
                }))
                {
                    return csvReader.GetRecords<Stock>().ToList();
                }
            }
        }

        static void calAverage(IEnumerable<Stock> tickers)
        {
            var closeList = new ConcurrentBag<decimal>();
            var dateYear = tickers.Select(x => x.Date.Year).Distinct();
            Parallel.ForEach(dateYear, tmp =>
            {
                lock (lockObject)
                { 
                    decimal count = 0;
                    var cant = tickers.Select(x => x.Date.Year).Where(x => x == tmp).Count();
                    foreach(var row in tickers)
                    {  
                        if(row.Date.Year == tmp)
                        {
                            count += row.Close;
                        }
                    }
                    closeList.Add(count / cant);
                }
            });

            var path = @"C:\Users\David\Desktop\Concurrencia\prueba.Json";
            StreamWriter sw = File.AppendText(path);

           Parallel.ForEach(closeList, i =>
            {
                if (File.Exists(path))
                {
                    sw.WriteLine("\n");
                    sw.WriteLine(i);
                }
                else
                {
                    var newJson = JsonConvert.SerializeObject(i);
                    File.WriteAllText(path, newJson);
                }
            });

            sw.Close();
        
            PrintAverage(closeList);

        }

          static void PrintAverage(ConcurrentBag<decimal> x)
          {
            int year = 1986;
              foreach (var tmp in x)
              {
                Console.WriteLine($"Año : {year}, Promedio: {tmp}");
                year++;
              }
          }


        private static readonly HttpClient _httpclient = new HttpClient();
        private static object lockObject = new object();
            
        static async Task Main(string[] args)
        {
            var ticker =  GetTickersAsync("msft.us");
            calAverage(ticker.Result);
            await Task.WhenAll(ticker);
            Console.ReadKey();
        }
    }
}
