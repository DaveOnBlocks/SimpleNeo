using System.IO;
using NUnit.Framework;

namespace SimpleNeo.Tests
{
    [TestFixture]
    public class NodeTests
    {
        [Test]
        public void StartNode()
        {
            var configuration = NetworkConfiguration.PrivateNet();
            configuration.ChainPath = Directory.GetCurrentDirectory() + "\\privateChain";

            using (var node = new SimpleNeo.Client(configuration, new NunitRealTimeLogger()))
            {
                node.Start();
            }
        }
    }
}
