using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Neo;
using Neo.Core;
using Neo.IO.Json;
using Neo.SmartContract;
using Neo.VM;
using SimpleNeo.Transactions;

namespace SimpleNeo.Contracts
{
    public class ContractEngine
    {
        private readonly TransactionExecutionEngine _transactionExecutionEngine;

        internal ContractEngine()
        {

        }
        internal ContractEngine(TransactionExecutionEngine transactionExecutionEngine)
        {
            _transactionExecutionEngine = transactionExecutionEngine;
        }

        public SimpleContract GetContract(UInt160 scriptHash)
        {
            var contractState = Blockchain.Default.GetContract(scriptHash);
            if (contractState == null)
                return null;
            return new SimpleContract(contractState, _transactionExecutionEngine);
        }

        public bool DeployNEP5Contract(SimpleContract contract)
        {
            //check to see if the hash is already deployed
            var contractState = Blockchain.Default.GetContract(contract.ScriptHash);
            if (contractState != null)
                throw new ApplicationException($"Can not deploy contract as contract with hash of {contract.ScriptHash} already exists on the blockchain");

            var return_type = "05".HexToBytes().Select(p => (ContractParameterType?) p).FirstOrDefault() ?? ContractParameterType.Void;

            InvocationTransaction tx;
            using (var sb = new ScriptBuilder())
            {
                sb.EmitSysCall("Neo.Contract.Create", contract.Script, "0710".HexToBytes(), return_type, true, contract.Name, contract.Version, contract.Author, contract.Email, contract.Description);
                tx = new InvocationTransaction { Script = sb.ToArray() };
            }

            //var scriptAsText = tx.Script.ToHexString();
            //var scriptCopy = scriptAsText;

            tx.Version = 1;
            //tx.Script = contract.Script; //scriptCopy.HexToBytes(); //probably redundant
            if (tx.Attributes == null) tx.Attributes = new TransactionAttribute[0];
            if (tx.Inputs == null) tx.Inputs = new CoinReference[0];
            if (tx.Outputs == null) tx.Outputs = new TransactionOutput[0];
            if (tx.Scripts == null) tx.Scripts = new Witness[0];
            var engine = ApplicationEngine.Run(tx.Script, tx);
            var resultStringBuilder = new StringBuilder();
            resultStringBuilder.AppendLine("Test Results: ");
            resultStringBuilder.AppendLine($"VM State: {engine.State}");
            resultStringBuilder.AppendLine($"Gas Consumed: {engine.GasConsumed}");
            resultStringBuilder.AppendLine($"Evaluation Stack: {new JArray(engine.EvaluationStack.Select(p => p.ToParameter().ToJson()))}");

            if (!engine.State.HasFlag(VMState.FAULT))
            {
                tx.Gas = engine.GasConsumed - Fixed8.FromDecimal(10);
                if (tx.Gas < Fixed8.Zero) tx.Gas = Fixed8.Zero;
                tx.Gas = tx.Gas.Ceiling();
            }
            else
            {
                return false;
            }

            if (_transactionExecutionEngine.InvokeTransactionOnBlockchain(tx, null, new InvokeOptions()))
                return true;
            return false;
        }

        public SimpleContract LoadContract(string avmPath)
        {
            return new SimpleContract(File.ReadAllBytes(avmPath), _transactionExecutionEngine);
        }

        public ContractState WaitForContract(SimpleContract contractData, int maxAttempts = 30)
        {
            var seekCount = 0;
            var contractFound = false;
            var contract = Blockchain.Default.GetContract(contractData.ScriptHash);
            do
            {
                if (contract == null)
                {
                    seekCount++;
                    Thread.Sleep(1000);
                    if (seekCount > maxAttempts)
                        throw new ApplicationException($"Could not find contract {contractData.ScriptHash} after {maxAttempts} attempts");
                }
                else
                {
                    contractFound = true;
                }
            } while (!contractFound);

            return contract;
        }
    }
}