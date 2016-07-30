using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;

namespace CrawlerWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("CRAWLER Worker");

            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();

            var Configuration = builder.Build();

            string connectionString = Configuration["Database"];
            string crawlerEndpoint = Configuration["Crawler"];
            string delayTimeString = Configuration["Delay"];

            int delay = Int32.Parse(delayTimeString);

            while (true)
            {
                Console.Write(".");

                try
                {
                    DoWork(connectionString, crawlerEndpoint);
                }
                catch(Exception ex) {
                    Console.WriteLine();
                    Console.WriteLine(ex.Message);
                }

                Task.Delay(delay).Wait();
            }
        }

        static void DoWork(string connectionString, string crawlerEndpoint)
        {
            List<string> linkList = new List<string>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("prGetNexLink", conn);

                conn.Open();
                
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string link = reader.GetString(0);

                    linkList.Add(link);

                    string body = LoadWebPage(link);

                    using (SqlConnection connInsert = new SqlConnection(connectionString))
                    {
                        connInsert.Open();
                        InsertLink(connInsert, link, body);
                    }                       
                }
            }

            FeedSearcher(crawlerEndpoint, "");
            FeedSearcher(crawlerEndpoint, "");
            FeedSearcher(crawlerEndpoint, "");
            FeedSearcher(crawlerEndpoint, "");
            FeedSearcher(crawlerEndpoint, "");
        }

        static string LoadWebPage(string link)
        {
            string result = null;

            using (var client = new HttpClient())
            {
                var url = new Uri(link);
                var message = new HttpRequestMessage(HttpMethod.Get, url);
                message.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.3; WOW64; Trident/7.0; Touch; rv:11.0) like Gecko");
                
                HttpResponseMessage response = client.SendAsync(message).Result;

                if (response.IsSuccessStatusCode)
                {
                    result = response.Content.ReadAsStringAsync().Result;
                }
            }

            return result;
        }

        static void FeedSearcher(string link, string body)
        {
            using (var client = new HttpClient())
            {                
                var url = new Uri(link);

                var req = new HttpRequestMessage(HttpMethod.Get, url);

                req.Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage response = client.SendAsync(req).Result;

                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                }
            }
        }

        static void InsertLink(SqlConnection conn, string link, string body)
        {
            SqlCommand cmd = new SqlCommand("INSERT tbLinks(link,body) VALUES (@1,@2)", conn);
            var p1 = cmd.Parameters.Add("@1", SqlDbType.VarChar, 60000);
            var p2 = cmd.Parameters.Add("@2", SqlDbType.VarChar, -1);

            p1.Value = link;
            p2.Value = body;

            cmd.ExecuteNonQuery();
        }

    }
}
