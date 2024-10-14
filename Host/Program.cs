using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Threading.Tasks;
using BevososService;

namespace Host
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (ServiceHost host = new ServiceHost(typeof(ServiceImplementation)))
            {
                host.Open();
                Console.WriteLine("Service is running...");
                Console.ReadLine();
            }
        }
    }
}
