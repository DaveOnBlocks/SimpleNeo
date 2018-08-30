using System.IO;
using Neo;
using Neo.Core;
using NUnit.Framework;

namespace SimpleNeo.Tests
{
    [TestFixture]
    public class WalletTests
    {
        private Client _client;

        [OneTimeSetUp]
        public void Initialize()
        {
            var configuration = NetworkConfiguration.PrivateNet();
            configuration.ChainPath = Directory.GetCurrentDirectory() + "\\privateChain";
            _client = new Client(configuration, new NunitRealTimeLogger());
            _client.Start();
        }

        [OneTimeTearDown]
        public void Stop()
        {
            _client.Dispose();
        }

        [Test]
        public void TransferFunds()
        {

            _client.OpenWallet("wallets\\owner.db3", "test");
            Client.CurrentWallet.PerformFundTransfer(Fixed8.One, "AaEQXNpntbPXtyWcbdHZTtFuzQXKWMde6u", Blockchain.UtilityToken); //send one gas
            Client.CurrentWallet.PerformFundTransfer(Fixed8.One, "AaEQXNpntbPXtyWcbdHZTtFuzQXKWMde6u", Blockchain.GoverningToken); //send one neo
        }
    }
}

    