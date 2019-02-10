using Microsoft.Extensions.DependencyInjection;
using System;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var runs = 100000;

            if (args.Length == 1)
            {
                int.TryParse(args[0], out runs);
            }

            var services = new ServiceCollection();
            services.AddMemoryCache();
            services.AddTransient<Demo>();

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<Demo>().Run(runs).GetAwaiter().GetResult();

            Console.WriteLine("Press any key to end this program");
            Console.ReadKey();
        }
    }
}
