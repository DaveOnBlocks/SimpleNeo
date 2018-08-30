using System.IO;
using System.Linq;
using System.Numerics;
using Neo;
using Neo.SmartContract;
using NUnit.Framework;
using SimpleNeo.Contracts;

namespace SimpleNeo.Tests.Functional
{
    [TestFixture]
    [SingleThreaded]
    public class Nep5Tests
    {
        private Client _client;
        private SimpleContract _contract;
        private SimpleParameter _nonOwnerParameter;
        private SimpleParameter _ownerParameter;
        private UInt160 _owner;
        private UInt160 _nonOwner;

        [OneTimeSetUp]
        public void Initialize()
        {
            var configuration = NetworkConfiguration.PrivateNet();
            configuration.ChainPath = Directory.GetCurrentDirectory() + "\\privateChain";

            _client = new Client(configuration ,new NunitRealTimeLogger());
            _client.Start();

            _client.OpenWallet("wallets\\nonOwner.db3", "test");
            _nonOwner = Client.CurrentWallet.GetAddresses().First();
            _client.OpenWallet("wallets\\owner.db3", "test");
            _owner = Client.CurrentWallet.GetAddresses().First();

            //If you have not opened a wallet, contracts will not work. may be hard to use due to that
            //the contract from disk, will not know the parameters
            _contract = _client.Contracts.LoadContract(@"C:\Demos\TutorialToken\TutorialToken\bin\Debug\TutorialToken.avm"); //get the hash from the disk version
            _contract = _client.Contracts.GetContract(_contract.ScriptHash); //get the data from the network
            _nonOwnerParameter = new SimpleParameter(_owner);
            _ownerParameter = new SimpleParameter(_nonOwner);
        }

        [OneTimeTearDown]
        public void Stop()
        {
            _client.Dispose();
        }

        [Test]
        public void BalanceOf_GetInvalidAddressBalance()
        {
            var param = new SimpleParameter(new UInt160());
            Assert.Throws<NeoExecutionException>(() => _contract.InvokeLocalMethod<BigInteger>("balanceOf", param));
        }

        [Test]
        public void BalanceOf_GetNonExistantAccountBalance()
        {
            var param = new SimpleParameter(new UInt160(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 })); //20 digit address but all zeroos
            var balance = _contract.InvokeLocalMethod<BigInteger>("balanceOf", param);
            Assert.AreEqual(new BigInteger(0), balance);
        }

        //        [Test]
        //        public void ClearOutBalanceShouldDeleteKey()
        //        {
        //            var amount = new ContractParameter();
        //            amount.Type = ContractParameterType.Integer;
        //            
        //
        //            var toStartingBalance = _contract.InvokeLocalMethod<BigInteger>("balanceOf", _nonOwnerParameter);
        //            if (toStartingBalance == 0) //make sure the balance we are going to clear has something to start with
        //            {
        //                BigInteger amountToTransfer = 101;
        //                amount.Value = amountToTransfer;
        //                _contract.InvokeBlockchainMethod("transfer", _ownerParameter, _nonOwnerParameter, amount);
        //            }
        //            else
        //            {
        //                amount.Value = toStartingBalance;
        //            }
        //
        //            Assert.IsNotNull(GetBalanceStorageItemFor(PegContractSetup.NonOwner.ToArray())); //make sure the storage is set
        //
        //            //empty out the wallet
        //            _client.OpenWallet("wallets\\nonOwner.db3", "test");
        //            _contract.InvokeBlockchainMethod("transfer", _nonOwnerParameter, _ownerParameter, amount); //transfer it all to the owner
        //
        //            Assert.IsNull(GetBalanceStorageItemFor(PegContractSetup.NonOwner.ToArray())); //make sure the storage is cleared out
        //
        //            _client.OpenWallet("wallets\\owner.db3", "test"); //make sure we are back to the owner 
        //        }
        //
        [Test]
        public void ContractHasStorageSet()
        {
            Assert.IsTrue(_contract.HasStorage);
        }

        [Test]
        public void ContractHasTwoParameters()
        {
            Assert.AreEqual(2, _contract.ParameterList.Length);
        }

        [Test]
        public void ContractReturnsByteArray()
        {
            Assert.AreEqual(ContractParameterType.ByteArray, _contract.ReturnType);
        }

        [Test]
        public void FirstParameterIsString()
        {
            Assert.AreEqual(ContractParameterType.String, _contract.ParameterList[0]);
        }

        [Test]
        public void GetDecimals()
        {
            var deimals = _contract.InvokeLocalMethod<BigInteger>("decimals");
            Assert.AreEqual(new BigInteger(2), deimals);
        }

        [Test]
        public void GetName()
        {
            var name = _contract.InvokeLocalMethod<string>("name");
            Assert.AreEqual("Tutorial Token", name);
        }

        [Test]
        public void GetNameUppercase() //code is case sensitive
        {
            var name = _contract.InvokeLocalMethod<string>("NAME");
            Assert.AreEqual("", name);
        }

        [Test]
        public void GetSymbol()
        {
            var name = _contract.InvokeLocalMethod<string>("symbol");
            Assert.AreEqual("TT", name);
        }

        [Test]
        public void GetTotalSupply()
        {
            var supply = _contract.InvokeLocalMethod<BigInteger>("totalSupply");
            Assert.AreEqual(new BigInteger(1_000__00), supply); //1,000.00 is all we have
        }

        [Test]
        public void SecondParameterIsArray()
        {
            Assert.AreEqual(ContractParameterType.Array, _contract.ParameterList[1]);
        }

        [Test]
        public void TransferDoubleOwnerBalanceToRecipient()
        {
            var amount = new SimpleParameter(0);


            var fromStartingBalance = _contract.InvokeLocalMethod<BigInteger>("balanceOf", _ownerParameter);
            var toStartingBalance = _contract.InvokeLocalMethod<BigInteger>("balanceOf", _nonOwnerParameter);


            amount.Value = fromStartingBalance * 2; //way over what the sender has in their posession


            var messages = _contract.InvokeBlockchainMethod("transfer", _ownerParameter, _nonOwnerParameter, amount);

            //ensure the transfer event was not fired
            if (messages.FindMessagesThatStartWith("transfer").Count != 0)
                Assert.Fail("Transfer event should not have fired but it did");


            var fromPostTransferBalance = _contract.InvokeLocalMethod<BigInteger>("balanceOf", _ownerParameter);
            var toPostTransferBalance = _contract.InvokeLocalMethod<BigInteger>("balanceOf", _nonOwnerParameter);

            Assert.AreEqual(fromStartingBalance, fromPostTransferBalance, "From balance");
            Assert.AreEqual(toStartingBalance, toPostTransferBalance, "To balance");
        }

        [Test]
        public void TransferFromOwnerToRecipient()
        {
            BigInteger amountToTransfer = 101;
            var amount = new SimpleParameter(amountToTransfer);

            var fromStartingBalance = _contract.InvokeLocalMethod<BigInteger>("balanceOf", _ownerParameter);
            var toStartingBalance = _contract.InvokeLocalMethod<BigInteger>("balanceOf", _nonOwnerParameter);

            var messages = _contract.InvokeBlockchainMethod("transfer", _ownerParameter, _nonOwnerParameter, amount);

            //ensure the transfer event was fired
            var wasTransferMessageReceived = messages.WasTransferMessageReceived(_owner.ToArray(), _nonOwner.ToArray(), amountToTransfer);
            if (!wasTransferMessageReceived.Item1)
                Assert.Fail(wasTransferMessageReceived.Item2);

            var fromPostTransferBalance = _contract.InvokeLocalMethod<BigInteger>("balanceOf", _ownerParameter);
            var toPostTransferBalance = _contract.InvokeLocalMethod<BigInteger>("balanceOf", _nonOwnerParameter);

            Assert.AreEqual(fromStartingBalance - amountToTransfer, fromPostTransferBalance, "From balance");
            Assert.AreEqual(toStartingBalance + amountToTransfer, toPostTransferBalance, "To balance");
        }


        [Test]
        public void TransferFromSelfToSelf_ShouldFireEventButNotChangeBalance()
        {
            BigInteger amountToTransfer = 101;
            var amount = new SimpleParameter(amountToTransfer);

            var fromStartingBalance = _contract.InvokeLocalMethod<BigInteger>("balanceOf", _ownerParameter);

            var messages = _contract.InvokeBlockchainMethod("transfer", _ownerParameter, _ownerParameter, amount);

            //ensure the transfer event was fired
            var wasTransferMessageReceived = messages.WasTransferMessageReceived(_owner.ToArray(), _owner.ToArray(), amountToTransfer);
            Assert.IsTrue(wasTransferMessageReceived.Item1);

            var fromPostTransferBalance = _contract.InvokeLocalMethod<BigInteger>("balanceOf", _ownerParameter);
            Assert.AreEqual(fromStartingBalance, fromPostTransferBalance, "From balance");
        }

        [Test]
        public void TransferNegativeAmountFromOwnerToRecipeint()
        {
            BigInteger amountToTransfer = -101;
            var amount = new SimpleParameter(amountToTransfer);

            var fromStartingBalance = _contract.InvokeLocalMethod<BigInteger>("balanceOf", _ownerParameter);
            var toStartingBalance = _contract.InvokeLocalMethod<BigInteger>("balanceOf", _nonOwnerParameter);

            var messages = _contract.InvokeBlockchainMethod("transfer", _ownerParameter, _nonOwnerParameter, amount);

            //ensure the transfer event was not fired
            if (messages.FindMessagesThatStartWith("transfer").Count != 0)
                Assert.Fail("Transfer event should not have fired but it did");

            var fromPostTransferBalance = _contract.InvokeLocalMethod<BigInteger>("balanceOf", _ownerParameter);
            var toPostTransferBalance = _contract.InvokeLocalMethod<BigInteger>("balanceOf", _nonOwnerParameter);

            Assert.AreEqual(fromStartingBalance, fromPostTransferBalance, "From balance");
            Assert.AreEqual(toStartingBalance, toPostTransferBalance, "To balance");
        }

        [Test]
        public void TransferNonOwnedBalance()
        {
            BigInteger amountToTransfer = -101;
            var amount = new SimpleParameter(amountToTransfer);

            var fromStartingBalance = _contract.InvokeLocalMethod<BigInteger>("balanceOf", _ownerParameter);
            var toStartingBalance = _contract.InvokeLocalMethod<BigInteger>("balanceOf", _nonOwnerParameter);


            var messages = _contract.InvokeBlockchainMethod("transfer", _nonOwnerParameter, _ownerParameter, amount); //NOTE: that the sender is not the user initiating the transaction!
            //ensure the transfer event was not fired
            if (messages.FindMessagesThatStartWith("transfer").Count != 0)
                Assert.Fail("Transfer event should not have fired but it did");

            var fromPostTransferBalance = _contract.InvokeLocalMethod<BigInteger>("balanceOf", _ownerParameter);
            var toPostTransferBalance = _contract.InvokeLocalMethod<BigInteger>("balanceOf", _nonOwnerParameter);

            Assert.AreEqual(fromStartingBalance, fromPostTransferBalance, "From balance");
            Assert.AreEqual(toStartingBalance, toPostTransferBalance, "To balance");
        }

        [Test]
        public void TransferVerySmallAmountFromOwnerToRecipient()
        {
            BigInteger amountToTransfer = 000000001;
            var amount = new SimpleParameter(amountToTransfer);

            var fromStartingBalance = _contract.InvokeLocalMethod<BigInteger>("balanceOf", _ownerParameter);
            var toStartingBalance = _contract.InvokeLocalMethod<BigInteger>("balanceOf", _nonOwnerParameter);

            var messages = _contract.InvokeBlockchainMethod("transfer", _ownerParameter, _nonOwnerParameter, amount);

            //ensure the transfer event was fired
            var wasTransferMessageReceived = messages.WasTransferMessageReceived(_owner.ToArray(), _nonOwner.ToArray(), amountToTransfer);
            if (!wasTransferMessageReceived.Item1)
                Assert.Fail(wasTransferMessageReceived.Item2);

            var fromPostTransferBalance = _contract.InvokeLocalMethod<BigInteger>("balanceOf", _ownerParameter);
            var toPostTransferBalance = _contract.InvokeLocalMethod<BigInteger>("balanceOf", _nonOwnerParameter);

            Assert.AreEqual(fromStartingBalance - amountToTransfer, fromPostTransferBalance, "From balance");
            Assert.AreEqual(toStartingBalance + amountToTransfer, toPostTransferBalance, "To balance");
        }

        [Test]
        public void TransferZeroAmountFromOwnerToRecipeint()
        {
            BigInteger amountToTransfer = 0;
            var amount = new SimpleParameter(amountToTransfer);

            var fromStartingBalance = _contract.InvokeLocalMethod<BigInteger>("balanceOf", _ownerParameter);
            var toStartingBalance = _contract.InvokeLocalMethod<BigInteger>("balanceOf", _nonOwnerParameter);

            var messages = _contract.InvokeBlockchainMethod("transfer", _ownerParameter, _nonOwnerParameter, amount);

            //ensure the transfer event was not fired
            var wasTransferMessageReceived = messages.WasTransferMessageReceived(_owner.ToArray(), _nonOwner.ToArray(), amountToTransfer);
            Assert.IsTrue(wasTransferMessageReceived.Item1);

            var fromPostTransferBalance = _contract.InvokeLocalMethod<BigInteger>("balanceOf", _ownerParameter);
            var toPostTransferBalance = _contract.InvokeLocalMethod<BigInteger>("balanceOf", _nonOwnerParameter);

            Assert.AreEqual(fromStartingBalance, fromPostTransferBalance, "From balance");
            Assert.AreEqual(toStartingBalance, toPostTransferBalance, "To balance");
        }
    }
}