using ParentalControl.Interfaces;
using System.Diagnostics;
using System.Management;

namespace ParentalControl.BLL
{
    public class WindowsOS : IOperatingSystem
    {
        private ManagementEventWatcher _watcher;

        public event EventHandler OnProcessStarted;

        public void Start()
        {
            if (_watcher == null)
            {
                // Создаем запрос WMI для отслеживания создания новых процессов.
                string query = @"SELECT * FROM __InstanceCreationEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_Process'";

                // Создаем экземпляр Watcher.
                _watcher = new ManagementEventWatcher(new WqlEventQuery(query));

                // Подписываемся на событие.
                _watcher.EventArrived += ProcessStartedEvent;

                // Начинаем наблюдение.
                _watcher.Start();
            }
        }

        public void Stop()
        {
            if (_watcher != null)
            {
                _watcher.Stop();
                _watcher.EventArrived -= ProcessStartedEvent;
                _watcher = null;
            }
        }

        private void ProcessStartedEvent(object sender, EventArrivedEventArgs e)
        {
            OnProcessStarted?.Invoke(this, e);
        }

        public string GetHostName()
        {
            string hostName = Environment.MachineName;
            return hostName;
        }

        public string[] GetProcesses()
        {
            Process[] processes = Process.GetProcesses();

            return processes
                .Select(p => p.ProcessName)
                .ToArray();
        }
    }
}
