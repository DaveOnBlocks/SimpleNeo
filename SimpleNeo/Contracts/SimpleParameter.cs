using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Neo;
using Neo.SmartContract;

namespace SimpleNeo.Contracts
{
    public class SimpleParameter //where T: UInt160, UInt256, BigInteger, Byte[], Boolean, String
    {
        public ContractParameterType ParameterType { get; }
        public object Value { get; set; }

        public static SimpleParameter CreateParameter(BigInteger value)
        {
            return new SimpleParameter(value, ContractParameterType.Integer);
        }

        public static SimpleParameter CreateParameter(UInt160 value)
        {
            return new SimpleParameter(value, ContractParameterType.Hash160);
        }

//        public static SimpleParameter<UInt256> CreateParameter(UInt256 value)
//        {
//            return new SimpleParameter<UInt256>(value, ContractParameterType.Hash256);
//        }
//
//        public static SimpleParameter<byte[]> CreateParameter(byte[] value)
//        {
//            return new SimpleParameter<byte[]>(value, ContractParameterType.ByteArray);
//        }
//
//        public static SimpleParameter<bool> CreateParameter(bool value)
//        {
//            return new SimpleParameter<bool>(value, ContractParameterType.ByteArray);
//        }
//
//        public static SimpleParameter<string> CreateParameter(string value)
//        {
//            return new SimpleParameter<string>(value, ContractParameterType.String);
//        }
//
//        public static SimpleParameter<Array> CreateParameter(Array value)
//        {
//            return new SimpleParameter<Array>(value, ContractParameterType.Array);
//        }


        private SimpleParameter(object value, ContractParameterType type)
        {
            ParameterType = type;
            this.Value = value;
        }

        //TODO: the remaining types
//        internal ContractParameterType ToContractParameterType()
//        {
//           if (typeof(T) == typeof(byte[])) //?
//                return ContractParameterType.InteropInterface;
//            if (typeof(T) == typeof(byte[])) //?
//                return ContractParameterType.PublicKey;
//            if (typeof(T) == typeof(byte[])) //?
//                return ContractParameterType.Signature;
//        }
        public static SimpleParameter CreateParameter(string value)
        {
            return new SimpleParameter(value, ContractParameterType.String);
        }

        public static SimpleParameter CreateParameter(byte[] value)
        {
            return new SimpleParameter(value, ContractParameterType.ByteArray);
        }
    }
}