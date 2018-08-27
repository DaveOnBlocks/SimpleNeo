using NUnit.Framework;

namespace SimpleNeo.Tests
{
    [TestFixture]
    public class NodeTests
    {
        [Test]
        public void StartNode()
        {
            //TODO: I want to be able to set network options here instead of protocol.json (or to specify a json file to read in)
            using (var node = new SimpleNeo.Client("privateChain", new NunitRealTimeLogger()))
            {
                node.Start();
            }
        }
    }
}
