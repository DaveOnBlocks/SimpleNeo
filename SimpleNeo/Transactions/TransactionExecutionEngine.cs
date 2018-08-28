using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using Neo;
using Neo.Core;
using Neo.Network;
using Neo.SmartContract;
using Neo.VM;
using Neo.Wallets;

namespace SimpleNeo.Transactions
{
    public class TransactionExecutionEngine
    {
        private readonly LocalNode _node;
        //private readonly Wallet _wallet;

        internal TransactionExecutionEngine()
        {
        }

        internal TransactionExecutionEngine(LocalNode node)
        {
            _node = node;
        }

        private UInt256 WatchForTx { get; set; }
        private bool TxFound { get; set; }

        public T InvokeLocalMethod<T>(UInt160 scriptHash, string methodName, params ContractParameter[] userSpecifiedParameters)
        {
            var parameters = new List<ContractParameter>();
            var methodParameter = new ContractParameter(ContractParameterType.ByteArray);
            methodParameter.Value = Encoding.UTF8.GetBytes(methodName);
            parameters.Add(methodParameter);


            if (userSpecifiedParameters.Length > 0)
            {
                var args = new ContractParameter();
                args.Type = ContractParameterType.Array;
                args.Value = userSpecifiedParameters.ToList();
                parameters.Add(args);
            }
            else
            {
                parameters.Add(new ContractParameter(ContractParameterType.Array));
            }

            //parameters.Add(new ContractParameter(ContractParameterType.Array));
            var walletSyncAttempts = 0;
            while (Client.CurrentWallet.NeoWallet.WalletHeight < Blockchain.Default.HeaderHeight)
            {
                walletSyncAttempts++;
                Thread.Sleep(1000); //get the wallet in sync or else MakeTransaction will fail
                if (walletSyncAttempts >= 30)
                    throw new WalletException("could not get the wallet in sync after 30 attempts");
            }


            using (var sb = new ScriptBuilder())
            {
                PushParameters(sb, parameters);
                sb.EmitAppCall(scriptHash, false);
                var customScriptText = sb.ToArray().ToHexString();

                var tx = new InvocationTransaction();
                tx.Version = 1;
                tx.Script = sb.ToArray();
                if (tx.Attributes == null) tx.Attributes = new TransactionAttribute[0];
                if (tx.Inputs == null) tx.Inputs = new CoinReference[0];
                if (tx.Outputs == null) tx.Outputs = new TransactionOutput[0];
                if (tx.Scripts == null) tx.Scripts = new Witness[0];


             
//                InvocationTransaction walletTx = null;
//                walletTx = Client.CurrentWallet.NeoWallet.MakeTransaction(new InvocationTransaction
//                {
//                    Version = tx.Version,
//                    Script = tx.Script,
//                    Gas = tx.Gas,
//                    Attributes = tx.Attributes,
//                    Inputs = tx.Inputs,
//                    Outputs = tx.Outputs
//                }, fee: Fixed8.Zero); //all transactions need something so include a small fee


                //if (walletTx == null) throw new NeoTransactionBuildException("Creating a TX resulted in a null transaction");

                //TODO: make the address a selection
                tx.Attributes = new TransactionAttribute[] { new TransactionAttribute() { Usage = TransactionAttributeUsage.Script, Data = Client.CurrentWallet.GetAddresses().First().ToArray() }};

//                ContractParametersContext context;
//                try
//                {
//                    context = new ContractParametersContext(tx);
//                }
//                catch (InvalidOperationException)
//                {
//                    throw new ApplicationException("unsynchronized block");
//                }
//
//                var sign = Client.CurrentWallet.NeoWallet.Sign(context);

                
                var engine = ApplicationEngine.Run(tx.Script, tx);
                var results = new StringBuilder();
                results.AppendLine($"Called: {methodName}");
                results.AppendLine($"VM State: {engine.State}");
                results.AppendLine($"Gas Consumed: {engine.GasConsumed}");

                if (engine.State.HasFlag(VMState.FAULT)) throw new NeoExecutionException();

                var result = engine.EvaluationStack.Pop();

                if (typeof(T) == typeof(string)) return (T) Convert.ChangeType(result.GetString(), typeof(T));

                if (typeof(T) == typeof(bool)) return (T) Convert.ChangeType(result.GetBoolean(), typeof(T));

                if (typeof(T) == typeof(BigInteger)) return (T) Convert.ChangeType(result.GetBigInteger(), typeof(T));

                if (typeof(T) == typeof(byte[]))
                {
                    var stackItems = result.GetByteArray();
                    return (T) Convert.ChangeType(stackItems, typeof(T));
                }

                throw new NotImplementedException(typeof(T).ToString());
            }
        }

        private void PushParameters(ScriptBuilder sb, List<ContractParameter> parameterList)
        {
            for (var i = parameterList.Count - 1; i >= 0; i--)
            {
                var param = parameterList[i];
                if (param == null || param.Value == null) continue;

                switch (param.Type)
                {
                    case ContractParameterType.String:
                        sb.EmitPush(Encoding.UTF8.GetBytes((string) param.Value));
                        break;
                    case ContractParameterType.Hash160:
                        var paramValue = (UInt160)param.Value;
                        sb.EmitPush(paramValue.ToArray());
                        break;
                    case ContractParameterType.Hash256:
                        var paramValue256 = (UInt256)param.Value;
                        sb.EmitPush(paramValue256.ToArray());
                        break;

                    case ContractParameterType.Signature:
                    case ContractParameterType.ByteArray:
                    case ContractParameterType.PublicKey:
                        sb.EmitPush((byte[]) param.Value);
                        break;
                    case ContractParameterType.Boolean:
                        sb.EmitPush((bool) param.Value);
                        break;
                    case ContractParameterType.Integer:
                        sb.EmitPush((BigInteger) param.Value);
                        break;
                    case ContractParameterType.Array:
                        var arrayList = (List<ContractParameter>) param.Value;
                        PushParameters(sb, arrayList);
                        sb.EmitPush(arrayList.Count);
                        sb.Emit(OpCode.PACK);
                        break;
                    default:
                        throw new NotImplementedException(param.Type.ToString());
                }
            }
        }

        public bool InvokeTransactionOnBlockchain(InvocationTransaction tx, UInt160 contractHash, InvokeOptions options)
        {
            Blockchain.PersistCompleted += Blockchain_PersistCompleted;
            if (options.AttachedNeo > Fixed8.Zero)
            {
                var neoOutput = new TransactionOutput();
                neoOutput.AssetId = Blockchain.GoverningToken.Hash;
                neoOutput.Value = options.AttachedNeo;
                neoOutput.ScriptHash = contractHash;
                var transactionOutputs = tx.Outputs.ToList();
                transactionOutputs.Add(neoOutput);
                tx.Outputs = transactionOutputs.ToArray();
            }

            if (options.AttachedGas > Fixed8.Zero)
            {
                var neoOutput = new TransactionOutput();
                neoOutput.AssetId = Blockchain.UtilityToken.Hash;
                neoOutput.Value = options.AttachedGas;
                neoOutput.ScriptHash = contractHash;
                var transactionOutputs = tx.Outputs.ToList();
                transactionOutputs.Add(neoOutput);
                tx.Outputs = transactionOutputs.ToArray();
            }

//            if (Client.CurrentWallet.WalletHeight > Blockchain.Default.HeaderHeight)
//            {
//                throw new ApplicationException("Wallet height is ahead of the blockchain height! It may need a rebuild");
//            }

            var walletSyncAttempts = 0;
            while (Client.CurrentWallet.NeoWallet.WalletHeight < Blockchain.Default.HeaderHeight)
            {
                walletSyncAttempts++;
                Thread.Sleep(1000); //get the wallet in sync or else MakeTransaction will fail
                if (walletSyncAttempts >= 30)
                    throw new WalletException("could not get the wallet in sync after 30 attempts");
            }


            var walletTx = Client.CurrentWallet.NeoWallet.MakeTransaction(new InvocationTransaction
            {
                Version = tx.Version,
                Script = tx.Script,
                Gas = tx.Gas,
                Attributes = tx.Attributes,
                Inputs = tx.Inputs,
                Outputs = tx.Outputs
            }, fee: options.Fee); //include a small fee

            if (walletTx == null)
                throw new NeoTransactionBuildException("Wallet TX was null. Possibly insufficient funds. If not wallet may need a rebuild");

            var context = new ContractParametersContext(walletTx);
            var sign = Client.CurrentWallet.NeoWallet.Sign(context); //fail here with index out of bounds
            if (context.Completed)
            {
                context.Verifiable.Scripts = context.GetScripts();
                Client.CurrentWallet.NeoWallet.ApplyTransaction(walletTx); //changes with different versions of NEO
                //Wallet.ApplyTransaction(walletTx);

                var relay = _node.Relay(walletTx);

                //var originalHeight = Blockchain.Default.Height; //store the height we sent at then wait for the next block
                //possibly check if sign/relay/save has actually worked? 

                //while (Blockchain.Default.Height <= originalHeight + 2) Thread.Sleep(1000); //wait for next block
                //while (this._wallet.WalletHeight <= originalHeight + 2) Thread.Sleep(1000); //wait for wallet to sync too!

                TxFound = false;
                WatchForTx = walletTx.Hash;

                Console.WriteLine(walletTx.Hash);
                //Console.WriteLine(tx.Hash);

                var count = 0;
                while (TxFound == false) //wait until the transaction is confirmed
                {
                    Thread.Sleep(1000);
                    count++;
                    if (count > 30)
                    {
                        Blockchain.PersistCompleted -= Blockchain_PersistCompleted;
                        return false;
                    }
                }

//                while(Client.CurrentWallet.WalletHeight < TxFoundInBlock && WalletIndexer.IndexHeight < TxFoundInBlock) //make sure the wallet gets this block
//                {
//                    Thread.Sleep(1000);
//                }

                //ensure we have an unspent coin back to use?
                //seems like the WalletIndexer is running on a background thread so the block may not be fully processed
                //e.g. the unconfirmed array in the wallet may not be updated in real time. The only event we have is that BlockChain.PersistCompleted was done which means we have the block stored to disk locally, this does not mean
                //that the wallet has completed updating based on the new block!
                while(Client.CurrentWallet.NeoWallet.FindUnspentCoins(Blockchain.UtilityToken.Hash, walletTx.NetworkFee, new UInt160[] { Client.CurrentWallet.GetAddresses().First() }) == null)
                {
                    Thread.Sleep(500); 
                }
                
            }
            else
            {
                Blockchain.PersistCompleted -= Blockchain_PersistCompleted;
                throw new ApplicationException("Incompleted Signature");
            }

            Blockchain.PersistCompleted -= Blockchain_PersistCompleted;
            return true;
        }

        private void Blockchain_PersistCompleted(object sender, Block e)
        {
            foreach (var transaction in e.Transactions)
            {
                Console.WriteLine(transaction.Type + " " + transaction.Hash);
                if (transaction.Hash == WatchForTx)
                {
                    TxFound = true;
                    TxFoundInBlock = e.Index;
                }
            }
        }

        private uint TxFoundInBlock { get; set; }

        public NotifyMessages InvokeBlockchainMethod(UInt160 contractHash, string methodToInvoke, InvokeOptions options, params ContractParameter[] userSpecifiedParameters)
        {
            //TODO: refactor this as it is copy/paste from Invoking locally now.
            var parameters = new List<ContractParameter>();
            var methodParameter = new ContractParameter(ContractParameterType.ByteArray);
            methodParameter.Value = Encoding.UTF8.GetBytes(methodToInvoke);
            parameters.Add(methodParameter);


            if (userSpecifiedParameters.Length > 0)
            {
                var args = new ContractParameter();
                args.Type = ContractParameterType.Array;
                args.Value = userSpecifiedParameters.ToList();
                parameters.Add(args);
            }
            else
            {
                parameters.Add(new ContractParameter(ContractParameterType.Array));
            }

            using (var sb = new ScriptBuilder())
            {
                for (var i = 0; i < options.NumberOfTimesToRunInTransaction; i++)
                {
                    PushParameters(sb, parameters);
                    sb.EmitAppCall(contractHash, false);
                }

                //useful for debugging to compare to what neo GUI does
                var customScriptText = sb.ToArray().ToHexString();


                //UI: 00c1013151c1086765744f776e6572677917c149bf660121556a4bc88b6adcb0b12b04f9
                var tx = new InvocationTransaction();
                tx.Version = 1;
                tx.Script = sb.ToArray();
                if (tx.Attributes == null) tx.Attributes = new TransactionAttribute[0];
                if (tx.Inputs == null) tx.Inputs = new CoinReference[0];
                if (tx.Outputs == null) tx.Outputs = new TransactionOutput[0];
                if (tx.Scripts == null) tx.Scripts = new Witness[0];
                var l = tx.Scripts.ToList();

                var messages = new NotifyMessages();
                EventHandler<NotifyEventArgs> handleNotify = (sender, args) => { messages.AddMessage(args); };
                StateReader.Notify += handleNotify; //listen to the blockchain and associate any messages you get to this transaction
                InvokeTransactionOnBlockchain(tx, contractHash, options);
                StateReader.Notify -= handleNotify; //stop listening.
                return messages;
            }
        }
    }
}