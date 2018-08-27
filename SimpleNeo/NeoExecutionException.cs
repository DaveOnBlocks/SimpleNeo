using System;

namespace SimpleNeo
{
    public class NeoExecutionException : Exception
    {
    }

    public class WalletException : Exception
    {
        public WalletException(string message) : base(message)
        {
        }
    }

    public class NeoTransactionBuildException: Exception
    {
        public NeoTransactionBuildException(string message) : base(message)
        {
        }
    }
}