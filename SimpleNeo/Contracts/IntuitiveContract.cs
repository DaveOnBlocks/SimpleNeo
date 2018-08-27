using System.Collections.Generic;
using Neo;
using Neo.Core;
using Neo.SmartContract;
using SimpleNeo.Transactions;

namespace SimpleNeo.Contracts
{
    public class IntuitiveContract
    {
        private readonly TransactionExecutionEngine _transactionExecutionEngine;

        internal IntuitiveContract(ContractState state, TransactionExecutionEngine transactionExecutionEngine)
        {
            _transactionExecutionEngine = transactionExecutionEngine;
            Name = state.Name;
            Script = state.Script;
            Author = state.Author;
            Description = state.Description;
            Email = state.Email;
            Version = state.CodeVersion;
            HasStorage = state.HasStorage;
            ParameterList = state.ParameterList;
            ReturnType = state.ReturnType;
        }

        public ContractParameterType ReturnType { get; set; }

        public ContractParameterType[] ParameterList { get; set; }

        public bool HasStorage { get; set; }

        internal IntuitiveContract(byte[] script, TransactionExecutionEngine transactionExecutionEngine)
        {
            _transactionExecutionEngine = transactionExecutionEngine;
            Script = script;
            ParameterList = new ContractParameterType[0];
        }


        //  public ContractParameterType[] ParameterList;
        //  public ContractParameterType ReturnType;
        //  public bool HasStorage;
        public string Name { get; set; }
        public string Author { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }

        public byte[] Script { get; set; }

        public UInt160 ScriptHash => Script.ToScriptHash();
        public string Version { get; set; }

        public T InvokeLocalMethod<T>(string methodName, params SimpleParameter[] parameters) //forward to the transactionEngine
        {
            return _transactionExecutionEngine.InvokeLocalMethod<T>(ScriptHash, methodName, ConvertToContractParameters(parameters));
        }

        public NotifyMessages InvokeBlockchainMethod(string methodToInvoke, params SimpleParameter[] parameters)
        {
            return InvokeBlockchainMethod(methodToInvoke, new InvokeOptions(), parameters);
        }

        public NotifyMessages InvokeBlockchainMethod(string methodToInvoke, InvokeOptions options, params SimpleParameter[] parameters)
        {
            return _transactionExecutionEngine.InvokeBlockchainMethod(ScriptHash, methodToInvoke, options, ConvertToContractParameters(parameters));
        }

        private ContractParameter[] ConvertToContractParameters(SimpleParameter[] parameters)
        {
            var neoParameter = new List<ContractParameter>();
            foreach (var simpleParameter in parameters)
            {
                var p = new ContractParameter(simpleParameter.ParameterType) {Value = simpleParameter.Value};
                neoParameter.Add(p);
            }

            return neoParameter.ToArray();
        }

        public StorageItem Storage(byte[] key)
        {
            var storageKey = new StorageKey
            {
                ScriptHash = this.ScriptHash,
                Key = key
            };
            return Blockchain.Default.GetStorageItem(storageKey);
        }
    }
}