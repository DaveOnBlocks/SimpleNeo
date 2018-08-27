using NUnit.Framework;

namespace SimpleNeo.Tests
{
    public class NunitRealTimeLogger : ILogger
    {
        public void LogMessage(string message)
        {
            TestContext.Progress.WriteLine(message);
            TestContext.Progress.Flush();
        }
    }
}