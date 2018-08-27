using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Neo.SmartContract;
using Neo.VM;

namespace SimpleNeo
{
    public class NotifyMessages
    {
        private List<KeyValuePair<string, ContractParameter[]>> _receivedNotifications;

        public NotifyMessages()
        {
            _receivedNotifications = new List<KeyValuePair<string, ContractParameter[]>>();
        }

        public void AddMessage(NotifyEventArgs args)
        {
            var contractParameter = args.State.ToParameter();
            if (contractParameter.Type != ContractParameterType.Array)  //will this always be the case?
                return;

            var parameters = contractParameter.Value as ContractParameter[];
            if (parameters == null)
                return;

            var notifyName = System.Text.Encoding.UTF8.GetString((byte[])parameters[0].Value); //will it always be a string?
            Console.WriteLine("Message Received: " + notifyName);
            _receivedNotifications.Add(new KeyValuePair<string, ContractParameter[]>(notifyName, parameters.ToList().Skip(1).ToArray())); //add the name then drop the name parameter out of the list
        }

        public List<ContractParameter[]> FindMessagesThatStartWith(string name)
        {
            return _receivedNotifications.Where(x => x.Key.StartsWith(name)).Select(x => x.Value).ToList();
        }

        public Tuple<bool, string> WasTransferMessageReceived(byte[] fromValue, byte[] toValue, BigInteger amount)
        {
            var transferMessages = FindMessagesThatStartWith("transfer");
            if (transferMessages == null || transferMessages.Count == 0)
                return new Tuple<bool, string>(false, "No messages could be found. Messages are case sensitive.");

            if (transferMessages.Count != 1)
                return new Tuple<bool, string>(false, $"Expected the message once but received it {transferMessages.Count} times");

            var firstMatchingMessage = transferMessages[0];
            var neoFrom = ((byte[])firstMatchingMessage[0].Value);
            if (fromValue == null && neoFrom.Length != 0) //neo will always return an array. Treat an empty array as the equivalent of a null for this case
            {
                return new Tuple<bool, string>(false, $"From value did not match. Expected null but was {firstMatchingMessage[0].Value}");
            }

            //if a from address is set, make sure neo has a value and it is equal
            if (neoFrom.Length != 0 && !fromValue.SequenceEqual((byte[])firstMatchingMessage[0].Value))
            {
                return new Tuple<bool, string>(false, $"From value did not match. Expected {fromValue} but was {firstMatchingMessage[0].Value}");
            }


            var neoTo = (byte[])firstMatchingMessage[1].Value;
            if (toValue == null && neoTo.Length != 0)
            {
                return new Tuple<bool, string>(false, $"To value did not match. Expected null but was {firstMatchingMessage[1].Value}");
            }
            if (neoTo.Length != 0 && !toValue.SequenceEqual(neoTo))
            {
                return new Tuple<bool, string>(false, $"To value did not match. Expected {toValue} but was {firstMatchingMessage[1].Value}");
            }


            //for small numbers it may be a byte array. For larger numbers it may be a bigint. No idea why.
            BigInteger messageAmount = 0;
            if (firstMatchingMessage[2].Type == ContractParameterType.Integer)
            {
                messageAmount = (BigInteger)firstMatchingMessage[2].Value;
            }
            else if (firstMatchingMessage[2].Type == ContractParameterType.ByteArray)
            {
                messageAmount = new BigInteger((byte[])firstMatchingMessage[2].Value);
            }
            else
            {
                throw new ApplicationException("don't know how to transform type to integer " + firstMatchingMessage[2].Type.ToString());
            }

            if (messageAmount != amount)
            {
                return new Tuple<bool, string>(false, $"Amount value did not match. Expected {amount} but was {messageAmount}");
            }


            return new Tuple<bool, string>(true, "");
        }
    }
}