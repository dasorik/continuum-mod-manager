using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using ElectronNET.API;

namespace Continuum.GUI
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = CreateHostBuilder(args);
			builder.Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseElectron(args);
					webBuilder.UseStartup<Startup>();
				});
	}
}
