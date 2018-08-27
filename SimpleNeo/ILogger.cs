using System;

namespace SimpleNeo
{
    public interface ILogger
    {
        void LogMessage(string message);
    }

    public class ConsoleLogger : ILogger
    {
        public void LogMessage(string message)
        {
           Console.WriteLine(message);
        }
    }
}