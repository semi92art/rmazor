using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace ClickersAPI
{
    public class Program
    {
        public static void Main(string[] _Args)
        {
            CreateHostBuilder(_Args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] _Args) =>
            Host.CreateDefaultBuilder(_Args)
                .ConfigureWebHostDefaults(_WebBuilder =>
                {
                    _WebBuilder.UseStartup<Startup>();
                });
    }
}
