using ParentalControl.BLL;
using ParentalControl.Interfaces;
using Serilog;

namespace ParentalControl.SVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom
                .Configuration(configuration)
                .CreateLogger();
            
            var builder = Host.CreateApplicationBuilder(args);
            
            builder.Logging.AddSerilog(Log.Logger);

            //builder.Logging.SetMinimumLevel(LogLevel.Debug);

            builder.Services.AddWindowsService(options =>
            {
                options.ServiceName = "Parental Control Service";
            });

            builder.Services.AddSingleton<IOperatingSystem, WindowsOS>();

            builder.Services.Configure<MyAppConfiguration>(builder.Configuration.GetSection(MyAppConfiguration.Name));

            builder.Services.AddSingleton<MyApp>();

            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();

            host.Run();
        }
    }
}