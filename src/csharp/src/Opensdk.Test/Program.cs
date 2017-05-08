using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Taikor.Opensdk;

namespace Opensdk.Test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //单线程访问测试
            Console.WriteLine("Waitting for authentication...");
            string userId = "";
            string appSecert = "";

            try
            {
                TaikorOauthClient client = new TaikorOauthClient(userId, appSecert);
                Console.WriteLine("authentication success.");

                Console.WriteLine("request the reader articles api.");
                Dictionary<string, object> parameters = new Dictionary<string, object>();

                string test = client.HttpGet("/", parameters).Content.ReadAsStringAsync().Result;

                Console.WriteLine(test);
                Console.ReadKey();
            }
            catch(Exception ex) { }
        }
    }
}
