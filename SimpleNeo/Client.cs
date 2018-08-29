using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Neo.Core;
using Neo.Implementations.Blockchains.LevelDB;
using Neo.Network;
using Neo.Wallets;
using SimpleNeo.Contracts;
using SimpleNeo.Transactions;
using SimpleNeo.Wallets;

namespace SimpleNeo
{
    public class Client : IDisposable
    {
        private readonly NetworkConfiguration _configuration;
        private readonly ILogger _logger;
        private Blockchain _blockchain;

        public Client(NetworkConfiguration configuration) : this(configuration, new ConsoleLogger())
        {
        }

        public Client(NetworkConfiguration configuration, ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public LocalNode LocalNode { get; private set; }
        public static SimpleWallet CurrentWallet { get; set; }
        public ContractEngine Contracts { get; private set; }
        public TransactionExecutionEngine Transaction { get; private set; }

        public void Dispose()
        {
            Stop();
        }

        /// <summary>
        ///     Starts a node and connects to the network. Once connected, starts synchornizing the blocks
        /// </summary>
        public void Start()
        {
            _configuration.CreateJsonFiles();
            
            _logger.LogMessage("Chain location: " + _configuration.ChainPath);
            //TODO: take in the folder, take in the ports as parameters
            var levelDbBlockchain = new LevelDBBlockchain(_configuration.ChainPath);
            _blockchain = Blockchain.RegisterBlockchain(levelDbBlockchain);
            LocalNode = new LocalNode {UpnpEnabled = true};
            _logger.LogMessage("Starting Node");

            //start a node in the background;
            Task.Run(() => { LocalNode.Start(_configuration.NodePort, _configuration.WsPort); });

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var nodeCount = 0;
            while (LocalNode.RemoteNodeCount <= 1)
            {
                if (stopWatch.Elapsed.TotalSeconds > 30)
                    throw new ApplicationException("could not connect to any peers within 30 seconds. Check network and settings and try again.");
                _logger.LogMessage("Looking for peers");
                Thread.Sleep(1000); //wait to connect to peers.
            }

            _logger.LogMessage("connected to peers");
            //sync the chain. If headerHeight == 0 then we have not received any data about the height yet
            var count = 0;
            while (Blockchain.Default.HeaderHeight == 0 || Blockchain.Default.Height < Blockchain.Default.HeaderHeight)
            {
                if (count % 10 == 0) //don't spam out too many messages
                    _logger.LogMessage($"Synchronizing blockchain. Processed {Blockchain.Default.Height.ToString("N0", CultureInfo.InvariantCulture)} of {Blockchain.Default.HeaderHeight.ToString("N0", CultureInfo.InvariantCulture)} blocks");
                Thread.Sleep(1000);
                count++;
            }

            _logger.LogMessage("Blockchain Synchronized");
        }


        /// <summary>
        ///     Stops and disposes of resources. Dispose() calls this method
        /// </summary>
        public void Stop()
        {
            _logger.LogMessage("Shutting Down");
            LocalNode?.Dispose();
            _blockchain?.Dispose();
        }

        public void OpenWallet(string path, string password)
        {
            CurrentWallet?.Dispose(); //clean up the current one, if any 

            var simpleWallet = new SimpleWallet(LocalNode);
            simpleWallet.Open(path, password);
            _logger.LogMessage("Index Height is :" + WalletIndexer.IndexHeight);
            Transaction = new TransactionExecutionEngine(LocalNode);
            Contracts = new ContractEngine(Transaction);
            CurrentWallet = simpleWallet;
        }
    }
}