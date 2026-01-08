using System.Diagnostics.Contracts;

namespace RescueRobotsCar.Services
{
    public class Logger
    {
        public bool IsClockEnabled { get; private set; } = true;
        public enum Severity
        {
            Info,
            Warning,
            Error
        }
        public void Log(string message, Severity sever)
        {
            string LogMsg = string.Empty;
            if (IsClockEnabled)
                LogMsg += $"[{DateTime.Now:HH:mm:ss}] ";
            LogMsg += $"[{sever}] {message}";
            Console.WriteLine(LogMsg);
        }
        public void ActivateClock(bool state)
        {
            IsClockEnabled = state;
        }
    }
}
