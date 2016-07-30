using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace CrawlerWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("CRAWLER Worker");

            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json");

            var Configuration = builder.Build();

            string connectionString = Configuration["Database"];

            DoWork(connectionString);
        }

        static void DoWork(string connectionString)
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

                    FeedSearcher(body);
                }

            }
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

        static void FeedSearcher(string body)
        {
            Debug.WriteLine(body);
            //using (var client = new HttpClient())
            //{
            //    var url = new Uri("");

            //    client.PostAsync()
            //}
        }

        static void InsertLink(SqlConnection conn)
        {
            SqlCommand cmd = new SqlCommand("INSERT tbLinks(keywords,link) VALUES (@1,@2)", conn);
            var p1 = cmd.Parameters.Add("@1", SqlDbType.VarChar, 2000);
            var p2 = cmd.Parameters.Add("@2", SqlDbType.VarChar, 2000);

            p1.Value = "test";
            p2.Value = "http://localhost";

            cmd.ExecuteNonQuery();
        }

    }
}
