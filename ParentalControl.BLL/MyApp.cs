using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ParentalControl.Interfaces;
using System.Management;

namespace ParentalControl.BLL
{
    public class MyApp : IDisposable
    {
        private readonly ILogger<MyApp> _logger;
        private readonly MyAppConfiguration _configuration;
        private int TimerInterval => _configuration.TimerInterval;

        IOperatingSystem _os;

        private CancellationTokenSource? _cancellationTokenSource;

        private Task? _endlessProcess;

        private int _counter = 0;

        public MyApp(
            IOptions<MyAppConfiguration> options,
            IOperatingSystem os,
            ILogger<MyApp> logger)
        {
            _logger = logger;

            _configuration = options?.Value
                ?? throw new ArgumentNullException(nameof(options));

            _os = os
                ?? throw new ArgumentNullException(nameof(os));
        }

        public async Task Start()
        {
            _logger.LogInformation("Запускаем процесс...");

            if (_endlessProcess == null)
            {
                _cancellationTokenSource = new CancellationTokenSource();
                CancellationToken cancellationToken = _cancellationTokenSource.Token;

                _os.OnProcessStarted += ProcessStarted;
                _os.Start();

                await ShowProcesses();

                _endlessProcess =
                    Task.Run(
                        () => EndlessProcess(cancellationToken),
                        cancellationToken);

                _logger.LogInformation("Процесс запущен");
            }
            else
            {
                _logger.LogInformation("Процесс уже работает");
            }
        }

        public async Task Stop()
        {
            _logger.LogInformation("Останавливаем процесс...");

            _os.OnProcessStarted -= ProcessStarted;
            _os.Stop();

            if (_endlessProcess != null)
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = null;

                _endlessProcess = null;

                _logger.LogInformation("Процесс остановлен");
            }
            else
            {
                _logger.LogInformation("Процесс уже остановлен");
            }
        }

        private async Task ShowProcesses()
        {
            string[] processes = _os.GetProcesses();

            int index = 0;
            foreach (string process in processes)
            {
                _logger.LogInformation($"{++index}. {process}");
            }
        }

        private void ProcessStarted(object? sender, EventArgs e)
        {
            var ev = (EventArrivedEventArgs)e;
            // Получаем объект процесса.
            ManagementBaseObject targetInstance = (ManagementBaseObject)ev.NewEvent["TargetInstance"];

            // Выводим информацию о новом процессе.
            var msg = $"Новый процесс: {targetInstance["Name"]}, ID: {targetInstance["ProcessId"]}";

            _logger.LogInformation(msg);
        }

        private async Task EndlessProcess(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Процес стартовал");

            while (!cancellationToken.IsCancellationRequested)
            {
                _counter++;

                _logger.LogInformation($"{_counter}");

                await Task.Delay(TimerInterval, cancellationToken);
            }

            _logger.LogInformation("Процес завершен");
        }

        #region IDisposable

        private bool _disposedValue;

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _cancellationTokenSource?.Dispose();
                    _cancellationTokenSource = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~MyApp()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        #endregion IDisposable
    }
}