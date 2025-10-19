using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ParentalControl.BLL;
using ParentalControl.Interfaces;

namespace ParentalControl.CLI
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // настраиваем логгирование
            ILoggerFactory loggerFactory =
                LoggerFactory.Create(
                    builder => builder.AddConsole());

            var mainLogger = loggerFactory.CreateLogger<Program>();

            mainLogger.LogInformation("Main started");

            var myAppLogger = loggerFactory.CreateLogger<MyApp>();

            // подключаем дочерние модули
            IOperatingSystem winOs = new WindowsOS();

            // собираем конфигурационные настройки
            IConfiguration configuration =
                new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    //.AddEnvironmentVariables()
                    .Build();

            var serviceCollection = new ServiceCollection();

            serviceCollection.Configure<MyAppConfiguration>(
                configuration.GetSection(MyAppConfiguration.Name));

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            IOptions<MyAppConfiguration> myAppOptions = serviceProvider.GetService<IOptions<MyAppConfiguration>>();

            // получаем окончательный экземпляр нашего приложения со всеми зависимостями
            var myApp = new MyApp(myAppOptions, winOs, myAppLogger);
            
            await myApp.Start();

            // обработчик нажатия клавиш
            using (var longTaskCancellationTokenSource = new CancellationTokenSource())
            {
                var longTaskCancellationToken = longTaskCancellationTokenSource.Token;

                // запускаем задачу для чтения клавиш в фоновом режиме
                var inputTask =
                    Task.Run(
                        () => ReadKeys(
                                myApp,
                                longTaskCancellationTokenSource,
                                mainLogger));

                try
                {
                    await Task.Delay(-1, longTaskCancellationToken);
                }
                catch (OperationCanceledException ex)
                {
                    mainLogger.LogInformation(ex.Message);
                }
                catch (Exception ex)
                {
                    mainLogger.LogError(ex, "Произошла ошибка");
                }
            }

            mainLogger.LogInformation("Main stopped");
        }

        private static async Task ReadKeys(
            MyApp myApp,
            CancellationTokenSource dependentTaskCancellationTokenSource,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Запускаем бесконечный цикл для чтения нажатий клавиш
                while (!cancellationToken.IsCancellationRequested)
                {
                    // Ожидаем нажатия клавиши
                    if (Console.KeyAvailable)
                    {
                        var pressedKey = Console.ReadKey(true);

                        if (pressedKey.Key == ConsoleKey.Escape)
                        {
                            dependentTaskCancellationTokenSource.Cancel();
                            
                            await myApp?.Stop();

                            // Выходим из цикла при нажатии Escape
                            //break;
                        }
                        else if (pressedKey.Key == ConsoleKey.UpArrow)
                        {
                            await myApp?.Start();
                        }
                        else if (pressedKey.Key == ConsoleKey.DownArrow)
                        {
                            await myApp?.Stop();
                        }
                        else
                        {
                            logger.LogInformation($"Вы нажали: {pressedKey.Key}");
                        }

                        await Task.Delay(100, cancellationToken);
                    }
                }

                logger.LogInformation($"Завершение цикла опроса нажатия клавиш");
            }
            catch (Exception ex)
            {
                logger.LogInformation($"Опрос нажатия клавиш завершен с ошибкой: {ex.Message}");
            }
        }
    }
}
