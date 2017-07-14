using System;
using Microsoft.Owin.Hosting;

namespace Stubomatic.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            const string uri = "http://localhost:9000";
            using (WebApp.Start<Startup>(uri))
            {
                Console.WriteLine("Service listening at {0}", uri);
                Console.WriteLine("Press [enter] to quit...");
                Console.ReadLine();
            }
        }
    }
}
