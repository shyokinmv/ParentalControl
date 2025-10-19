using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParentalControl.Interfaces
{
    public interface IOperatingSystem
    {
        event EventHandler OnProcessStarted;

        string GetHostName();
        string[] GetProcesses();

        void Start();
        void Stop();
    }
}
