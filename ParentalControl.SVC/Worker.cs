using ParentalControl.BLL;

namespace ParentalControl.SVC
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly MyApp _myApp;

        public Worker(
            MyApp myApp,
            ILogger<Worker> logger)
        {
            _logger = logger;
            _myApp = myApp;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker starting at: {time}", DateTimeOffset.Now);

                await _myApp.Start();

                _logger.LogInformation("Worker starting at: {time}", DateTimeOffset.Now);

                await Task.Delay(-1, stoppingToken);

                _logger.LogInformation("Worker stopping at: {time}", DateTimeOffset.Now);

                await _myApp.Stop();

                _logger.LogInformation("Worker stopped at: {time}", DateTimeOffset.Now);
            }
        }
    }
}
