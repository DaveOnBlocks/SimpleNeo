using System;
using System.IO;
using NUnit.Framework;
using SimpleNeo.Contracts;

namespace SimpleNeo.Tests
{
    [TestFixture]
    public class GeneralTests
    {
        private Client _client;
        private SimpleContract _contract;

        [OneTimeSetUp]
        public void Initialize()
        {
            var configuration = NetworkConfiguration.PrivateNet();
            configuration.ChainPath = Directory.GetCurrentDirectory() + "\\privateChain";

            _client = new Client(configuration, new NunitRealTimeLogger());
            _client.Start();
            _client.OpenWallet("..\\..\\wallets\\owner.db3", "test");
           //_client.CurrentWallet.Rebuild();
            _contract = _client.Contracts.LoadContract(@"C:\Demos\TutorialToken\TutorialToken\bin\Debug\TutorialToken.avm");
        }

        [OneTimeTearDown]
        public void Stop()
        {
            _client.Dispose();
        }

        [Test]
        public void Deploy()
        {
            _contract.InvokeBlockchainMethod("deploy");
        }

        [Test]
        public void DeployNep5()
        {
            if (_client.Contracts.GetContract(_contract.ScriptHash) != null) Assert.Fail("Contract Already Deployed");

            _contract.Author = "Author";
            _contract.Name = "Sample Coin";
            _contract.Description = "Sample to show deploying a simple contract";
            _contract.Email = "e@mail.com";
            _contract.Version = DateTime.Now.ToString();
            _client.Contracts.DeployNEP5Contract(_contract); //deploy a NEP-5 contract with a 05 return type, a 0710 parameter list, storage enabled, dynamic call not enabled. Will wait for a block before returning

            _client.Contracts.WaitForContract(_contract); //wait until the contract appears on the blockchain (by default, tries 10 times with a one second pause in between)
        }
    }
}