using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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

            List<string> linkList = new List<string>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("select link from tbPendingLinks", conn);

                conn.Open();

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string link = reader.GetString(0);
                    linkList.Add(link);
                }

                using (SqlConnection connInsert = new SqlConnection(connectionString))
                {
                    connInsert.Open();
                    InsertLink(connInsert);
                }
            }
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
