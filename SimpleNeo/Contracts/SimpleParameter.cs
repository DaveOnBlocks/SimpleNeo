using System.Numerics;
using Neo;
using Neo.SmartContract;

namespace SimpleNeo.Contracts
{
    public class SimpleParameter
    {
        public SimpleParameter(BigInteger value) : this(value, ContractParameterType.Integer)
        {
        }

        public SimpleParameter(UInt160 value) : this(value, ContractParameterType.Hash160)
        {
        }

        public SimpleParameter(string value) : this(value, ContractParameterType.String)
        {
        }

        public SimpleParameter(byte[] value) : this(value, ContractParameterType.ByteArray)
        {
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


        public SimpleParameter(object value, ContractParameterType type)
        {
            ParameterType = type;
            Value = value;
        }

        public ContractParameterType ParameterType { get; }
        public object Value { get; set; }

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
    }
}