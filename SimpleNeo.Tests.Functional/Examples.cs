using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using Neo;
using Neo.Core;
using Neo.SmartContract;
using SimpleNeo.Contracts;

namespace SimpleNeo.Tests
{
    public class Examples
    {
        public void SendFunds()
        {
            var configuration = NetworkConfiguration.PrivateNet();
            configuration.ChainPath = Directory.GetCurrentDirectory() + "\\privateChain";
            using (var client = new Client(configuration))
            {
                client.Start();
                client.OpenWallet("myWallet.db3", "*******");
                
                //transfer funds to an account
                Client.CurrentWallet.PerformFundTransfer(Fixed8.One, "AaEQXNpntbPXtyWcbdHZTtFuzQXKWMde6u", Blockchain.UtilityToken); //send one gas
                Client.CurrentWallet.PerformFundTransfer(Fixed8.One, "AaEQXNpntbPXtyWcbdHZTtFuzQXKWMde6u", Blockchain.GoverningToken); //send one neo


                //load a nep-5 contract from disk and deploy it
                var contract = client.Contracts.LoadContract(@"C:\Demos\TutorialToken\TutorialToken\bin\Debug\TutorialToken.avm"); //get the hash from the disk version

                contract.Author = "Author";
                contract.Name = "Sample Coin";
                contract.Description = "Sample to show deploying a simple contract";
                contract.Email = "e@mail.com";
                contract.Version = DateTime.Now.ToString(CultureInfo.InvariantCulture);
                client.Contracts.DeployNEP5Contract(contract); //deploy a NEP-5 contract with a 05 return type, a 0710 parameter list, storage enabled, dynamic call not enabled.

                client.Contracts.WaitForContract(contract); //wait until the contract appears on the blockchain (by default, tries 10 times with a one second pause in between)

                Console.WriteLine(contract.InvokeLocalMethod<string>("name")); //invoke a method locally using blockchain data but not altering data (this works the same as when you "test" invoke from neo-gui

                var amount = new SimpleParameter(101);
                var address1 = Client.CurrentWallet.GetAddresses().First();
                var address2 = Client.CurrentWallet.GetAddresses().ToList()[1]; 

                var from = new SimpleParameter(address1);
                var to = new SimpleParameter(address2);

                var messages = contract.InvokeBlockchainMethod("transfer", from, to, amount); //invoke a method on the blockchain and retrieve any notification messages generated
                var matchingMessages = messages.FindMessagesThatStartWith("transfer");
                if (matchingMessages.Count != 0)
                {
                    var firstMatch = matchingMessages[0]; //firstMatch is an array of contractParameters for the different parts of the notification message
                    Console.WriteLine($"Transfer was invoked. From: {firstMatch[1]} To: {firstMatch[2]} Amount: {firstMatch[3]}");
                }

                //a simplified way to test the transfer was done (good for automated tests):
                var wasTransferMessageReceived = messages.WasTransferMessageReceived(address1.ToArray(), address2.ToArray(), (BigInteger)amount.Value);

            }
        }
    }
}