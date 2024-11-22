using System;
using System.ServiceModel;
using BevososService;
using BevososService.Implementations;


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
