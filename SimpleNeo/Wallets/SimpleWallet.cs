using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Threading;
using Neo;
using Neo.Core;
using Neo.Cryptography;
using Neo.Implementations.Wallets.EntityFramework;
using Neo.Network;
using Neo.SmartContract;
using Neo.Wallets;

namespace SimpleNeo.Wallets
{
    public class SimpleWallet : IDisposable
    {
        private readonly LocalNode _node;

        internal SimpleWallet(LocalNode node)
        {
            _node = node;
        }

        internal UserWallet NeoWallet { get; set; } //the neo wallet we perform operations against

        public IEnumerable<UInt160> Addresses => NeoWallet.GetAddresses();

        public void Dispose()
        {
            NeoWallet?.Dispose();
        }

        public uint WalletHeight => NeoWallet.WalletHeight;

        public void Rebuild()
        {
            NeoWallet.Rebuild();
            while (NeoWallet.WalletHeight < Blockchain.Default.HeaderHeight)
            {
                Thread.Sleep(1000);
            }
        }

        public void Open(string path, string password)
        {
            var tempWallet = UserWallet.Open(path, password);
            while (tempWallet.WalletHeight < Blockchain.Default.HeaderHeight) Thread.Sleep(500); //sync the wallet
            tempWallet.LoadTransactions();
            NeoWallet = tempWallet; //UserWallet type has a method to LoadTransactions. The general wallet does not
        }


        public void PerformFundTransfer(Fixed8 amountToTransfer, UInt160 destinationScriptHash, IInventory assetId, UInt160 fromAddress)
        {
            //  public UInt160 ChangeAddress => Wallet.ToScriptHash((string)comboBox1.SelectedItem);

            var tx = new ContractTransaction();
            var neoOutput = new TransactionOutput();
            neoOutput.AssetId = assetId.Hash;
            neoOutput.Value = amountToTransfer;
            neoOutput.ScriptHash = destinationScriptHash;
            tx.Outputs = new[] {neoOutput};

            var walletTx = NeoWallet.MakeTransaction(tx, fromAddress);

            if (walletTx == null)
                throw new ApplicationException("Wallet TX was null. Possibly insufficient funds");

            ContractParametersContext context;
            try
            {
                context = new ContractParametersContext(walletTx);
            }
            catch (InvalidOperationException)
            {
                throw new ApplicationException("unsynchronized block");
            }

            var sign = NeoWallet.Sign(context);
            if (context.Completed)
            {
                context.Verifiable.Scripts = context.GetScripts();
                var saveTransaction = NeoWallet.SaveTransaction(walletTx); //changes with different versions of NEO
                //Wallet.ApplyTransaction(walletTx);
                var relay = _node.Relay(walletTx);

                var originalHeight = Blockchain.Default.Height; //store the height we sent at then wait for the next block
                //possibly check if sign/relay/save has actually worked? 

                while (Blockchain.Default.Height <= originalHeight + 1) Thread.Sleep(1000); //wait for next block
            }
            else
            {
                throw new ApplicationException("Incompleted Signature");
            }
        }

        //not sure if this belongs here yet as a public method
        //_client.CurrentWallet.AddressToScriptHash does not fit quite right to me
        private UInt160 AddressToScriptHash(string address)
        {
            if (address.Length != 25)
                throw new ValidationException("Address must be 25 characters long");

            return Wallet.ToScriptHash(address);
        }

        public void PerformFundTransfer(Fixed8 amountToTransfer, string address, IInventory assetId)
        {
          PerformFundTransfer(amountToTransfer, AddressToScriptHash(address), assetId);
        }

        public void PerformFundTransfer(Fixed8 amountToTransfer, UInt160 destinationScriptHash, IInventory assetId)
        {
            PerformFundTransfer(amountToTransfer, destinationScriptHash, assetId, null);
        }

        public Fixed8 GetBalance(UInt256 assetId)
        {
            return this.NeoWallet.GetBalance(assetId);
        }
    }
}