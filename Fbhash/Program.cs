using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;

namespace Fbhash
{
    class Program
    {
        public static List<Datum> adata;
        static void Main(string[] args)
        {
            adata = new List<Datum>();
            Console.WriteLine("Hello World!");
            GetAdSets("https://graph.facebook.com/v7.0/act_2569284029988076/adcreatives?access_token=" + Keys.token + "&fields=image_hash%2Cname%2Cimage_url&limit=1000");
            File.WriteAllText($"ihash{DateTime.Now:hhmmss}.csv", makeCSV(adata.ToArray()));
        }


        static public void GetAdSets(string url)
        {
            HttpClient HttpClient = new HttpClient();
            using (HttpClient)
            {
                using var request = new HttpRequestMessage(new HttpMethod("GET"), url);
                HttpResponseMessage response = null;
                response = HttpClient.SendAsync(request).Result;
                var resA = response.Content.ReadAsStringAsync().Result;
                AdCData d = JsonSerializer.Deserialize<AdCData>(resA);
                adata.AddRange(d.data.Where(a => a.image_hash != null).Distinct());
                if (d.paging.next != null)
                {
                    GetAdSets(d.paging.next);
                    Console.WriteLine("Getting next 1000");
                }
            }
        }

        static string makeCSV(object[] items)
        {
            var output = "";
            var delimiter = ',';
            using (var sw = new StringWriter())
            {
                var properties = items[0].GetType().GetProperties();
                var header = properties
                .Select(n => n.Name)
                .Aggregate((a, b) => a + delimiter + b);

                sw.WriteLine(header);
                foreach (var item in items)
                {
                    var row = string.Join(",", properties.Select(n => n.GetValue(item, null)).Select(n =>
                    {
                        if (n == null)
                        {
                            return "null";
                        }
                        else
                        {
                            if (n.ToString().Contains(","))
                            {
                                return "\"" + n.ToString() + "\"";
                            }
                            else
                            {
                                return n.ToString();
                            }
                        }
                    }));
                    sw.WriteLine(row);
                }

                output = sw.ToString();
            }
            return output;
        }
    }

    public class Datum
    {
        public string id { get; set; }
        public string image_hash { get; set; }
        public string name { get; set; }
        public string image_url { get; set; }
    }

    public class Cursors
    {
        public string before { get; set; }
        public string after { get; set; }
    }

    public class Paging
    {
        public Cursors cursors { get; set; }
        public string next { get; set; }
    }

    public class AdCData
    {
        public List<Datum> data { get; set; }
        public Paging paging { get; set; }
    }
}
