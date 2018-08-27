using Neo;

namespace SimpleNeo.Transactions
{
    public class InvokeOptions
    {
        public Fixed8 AttachedNeo { get; set; }
        public Fixed8 AttachedGas { get; set; }
        public Fixed8 Fee { get; set; }
        public int NumberOfTimesToRunInTransaction { get; set; }

        public InvokeOptions()
        {
            NumberOfTimesToRunInTransaction = 1;
            Fee = Fixed8.FromDecimal(0.001m);
        }
    }
}