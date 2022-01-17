using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Continuum.GUI.Services;
using Tewr.Blazor.FileReader;

namespace Continuum.GUI
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddRazorPages();
			services.AddServerSideBlazor();
			services.AddSingleton<ModService>();
			services.AddSingleton<ToastService>();
			services.AddSingleton<DialogService>();
			services.AddFileReaderService();
			services.AddBlazorContextMenu();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			Continuum.Common.Logging.Logger.ConfigureLogger(new AppLogger());
			Global.CONTENT_ROOT = env.WebRootPath;

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapBlazorHub();
				endpoints.MapFallbackToPage("/_Host");
			});

			// Open the Electron-Window here
			var windowOptions = new BrowserWindowOptions()
			{
				MinWidth = 1000,
				MinHeight = 800,
				Width = 1460,
				Height = 800,
				UseContentSize = true,
				BackgroundColor = "#303030"
			};

			windowOptions.Frame = false;

			Task.Run(async () => await CreateBrowserWindow(windowOptions));
		}

		async Task CreateBrowserWindow(BrowserWindowOptions windowOptions)
		{
			var browserWindow = await Electron.WindowManager.CreateWindowAsync(windowOptions);
			//browserWindow.SetMenuBarVisibility(false);
		}
	}
}
