# SimpleNeo
Provides a simplified interface to do common tasks against Neo blockchain. The existing Neo code base does not provide a good clean API for performing common tasks. The goal of this library is to abstract the underlying complexity of the Neo code base into a set of simple functions that make using Neo practial and simple from any .NET application.

SimpleNeo runs as a full node. This means the entire blockchain is downloaded to be used. It also does not require any other services to be running (e.g. NeoScan API) to allow it to work. This allows you to easily setup an environment to develop in, not have a reliance on any RPC node being online/available/fast, as well as enable features that RPC does not expose.

# Status
This is currently an alpha project and everything is subject to change.

Current Wallet Functionality:
* Open a db3/json wallet file
* Transfer NEO/GAS from a wallet
* Get balance of NEO/GAS (of an open wallet)

Current Contract Functionality:
* Deploy contracts (shortcut for NEP-5)
* Load a contract from disk
* Load a contract from the blockchain
* Invoke a contract method locally against the NEO Virtual Machine
* Invoke a contract against the blockchain
* Programatically define the network (will generate a protocol.json file at runtime)

Planned Functionality:
* Async operations
* Possibly add an address object that can easily convert to script hash (Neo recently added a WalletAccount object)
* Ability to monitor the blockchain for notifications for a specific contract
* More control over which address is used in a wallet (via InvokeOptions)
 

# Example Usage
```
using (var client = new Client(Directory.GetCurrentDirectory() + "\\privateChain"))
{
    client.Start();
    client.OpenWallet("myWallet.db3", "*******");

    //transfer funds to an account
    client.CurrentWallet.PerformFundTransfer(Fixed8.One, "AaEQXNpntbPXtyWcbdHZTtFuzQXKWMde6u", Blockchain.UtilityToken); //send one gas
    client.CurrentWallet.PerformFundTransfer(Fixed8.One, "AaEQXNpntbPXtyWcbdHZTtFuzQXKWMde6u", Blockchain.GoverningToken); //send one neo


    //load a nep-5 contract from disk and deploy it
    var contract = client.Contracts.LoadContract(@"C:\Demos\TutorialToken\TutorialToken\bin\Debug\TutorialToken.avm"); 

    contract.Author = "Author";
    contract.Name = "Sample Coin";
    contract.Description = "Sample to show deploying a simple contract";
    contract.Email = "e@mail.com";
    contract.Version = DateTime.Now.ToString();
    //deploy a NEP-5 contract with a 05 return type, a 0710 parameter list, storage enabled, dynamic call not enabled.
    client.Contracts.DeployNEP5Contract(contract); 

    //wait until the contract appears on the blockchain (by default, tries 10 times with a one second pause in between)
    client.Contracts.WaitForContract(contract); 

    //invoke a method locally using blockchain data but not altering data (this works the same as when you "test" invoke from neo-gui
    Console.WriteLine(contract.InvokeLocalMethod<string>("name")); 

    var amount = new ContractParameter(ContractParameterType.Integer) {Value = (BigInteger) 101};
    var address1 = client.CurrentWallet.Addresses.First();
    var address2 = client.CurrentWallet.Addresses.ToList()[1]; 

    var from = new ContractParameter(ContractParameterType.Hash160) { Value = address1.ToArray() };
    var to = new ContractParameter(ContractParameterType.Hash160) { Value = address2.ToArray() };

    //invoke a method on the blockchain and retrieve any notification messages generated
    var messages = contract.InvokeBlockchainMethod("transfer", from, to, amount); 
    var matchingMessages = messages.FindMessagesThatStartWith("transfer");
    if (matchingMessages.Count != 0)
    {
        //firstMatch is an array of contractParameters for the different parts of the notification message
        var firstMatch = matchingMessages[0]; 
        Console.WriteLine($"Transfer was invoked. From: {firstMatch[1]} To: {firstMatch[2]} Amount: {firstMatch[3]}");
    }

    //a simplified way to test the transfer was done (good for automated tests):
    var wasTransferMessageReceived = messages.WasTransferMessageReceived(address1.ToArray(), address2.ToArray(), (BigInteger)amount.Value);

}
```

#Credits and License
The implementations of a lot of functionality has been discovered via the [neo-gui](https://github.com/neo-project/neo-gui) project. 

This project is released under the MIT license, see [LICENSE](/LICENSE) for more details.
