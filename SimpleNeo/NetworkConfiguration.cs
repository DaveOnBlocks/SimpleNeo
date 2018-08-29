using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using Neo.IO.Json;
using Newtonsoft.Json;

namespace SimpleNeo
{
    public class NetworkConfiguration
    {
        public NetworkConfiguration()
        {
            StandbyValidators = new List<string>();
            SeedList = new List<IPEndPoint>();
            AddressVersion = 0x23;
        }

        public int WsPort { get; set; }
        public int NodePort { get; set; }

        public uint Magic { get; set; }
        public byte AddressVersion { get; set; }
        public List<string> StandbyValidators { get; set; }
        public List<IPEndPoint> SeedList { get; set; }
        public string ChainPath { get; set; }

        internal void CreateJsonFiles()
        {
            dynamic data = new ExpandoObject();
            data.ProtocolConfiguration = new ExpandoObject();
            data.ProtocolConfiguration.Magic = this.Magic;
            data.ProtocolConfiguration.AddressVersion = this.AddressVersion;
            data.ProtocolConfiguration.StandbyValidators = this.StandbyValidators;
            data.ProtocolConfiguration.SeedList = new List<string>();

            foreach (var seed in this.SeedList)
            {
                data.ProtocolConfiguration.SeedList.Add(seed.Address.ToString() + ":" + seed.Port);
            }

            File.WriteAllText(@"protocol.json", Newtonsoft.Json.JsonConvert.SerializeObject(data, Formatting.Indented));
        }


        public static NetworkConfiguration PrivateNet()
        {
            var configuration = new NetworkConfiguration();
            configuration.Magic = 56753;
            configuration.NodePort = 20333;
            configuration.WsPort = 20334;

            configuration.StandbyValidators.Add("02b3622bf4017bdfe317c58aed5f4c753f206b7db896046fa7d774bbc4bf7f8dc2");
            configuration.StandbyValidators.Add("02103a7f7dd016558597f7960d27c516a4394fd968b9e65155eb4b013e4040406e");
            configuration.StandbyValidators.Add("03d90c07df63e690ce77912e10ab51acc944b66860237b608c4f8f8309e71ee699");
            configuration.StandbyValidators.Add("02a7bc55fe8684e0119768d104ba30795bdcc86619e864add26156723ed185cd62");

            configuration.SeedList.Add(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 20333));
            configuration.SeedList.Add(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 20334));
            configuration.SeedList.Add(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 20335));
            configuration.SeedList.Add(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 20336));
            
            return configuration;
        }

        public static NetworkConfiguration MainNet()
        {
            var configuration = new NetworkConfiguration();
            configuration.Magic = 7630401;
            configuration.NodePort = 10333;
            configuration.WsPort = 10334;

            configuration.StandbyValidators.Add("03b209fd4f53a7170ea4444e0cb0a6bb6a53c2bd016926989cf85f9b0fba17a70c");
            configuration.StandbyValidators.Add("02df48f60e8f3e01c48ff40b9b7f1310d7a8b2a193188befe1c2e3df740e895093");
            configuration.StandbyValidators.Add("03b8d9d5771d8f513aa0869b9cc8d50986403b78c6da36890638c3d46a5adce04a");
            configuration.StandbyValidators.Add("02ca0e27697b9c248f6f16e085fd0061e26f44da85b58ee835c110caa5ec3ba554");
            configuration.StandbyValidators.Add("024c7b7fb6c310fccf1ba33b082519d82964ea93868d676662d4a59ad548df0e7d");
            configuration.StandbyValidators.Add("02aaec38470f6aad0042c6e877cfd8087d2676b0f516fddd362801b9bd3936399e");
            configuration.StandbyValidators.Add("02486fd15702c4490a26703112a5cc1d0923fd697a33406bd5a1c00e0013b09a70");

            configuration.SeedList.Add(new IPEndPoint(IPAddress.Parse("seed1.neo.org"), 10333));
            configuration.SeedList.Add(new IPEndPoint(IPAddress.Parse("seed2.neo.org"), 10333));
            configuration.SeedList.Add(new IPEndPoint(IPAddress.Parse("seed3.neo.org"), 10333));
            configuration.SeedList.Add(new IPEndPoint(IPAddress.Parse("seed4.neo.org"), 10333));
            configuration.SeedList.Add(new IPEndPoint(IPAddress.Parse("seed5.neo.org"), 10333));
            

            return configuration;
        }

        public static NetworkConfiguration TestNet()
        {
            var configuration = new NetworkConfiguration();
            configuration.Magic = 1953787457;
            configuration.NodePort = 20333;
            configuration.WsPort = 20334;

            configuration.StandbyValidators.Add("0327da12b5c40200e9f65569476bbff2218da4f32548ff43b6387ec1416a231ee8");
            configuration.StandbyValidators.Add("026ce35b29147ad09e4afe4ec4a7319095f08198fa8babbe3c56e970b143528d22");
            configuration.StandbyValidators.Add("0209e7fd41dfb5c2f8dc72eb30358ac100ea8c72da18847befe06eade68cebfcb9");
            configuration.StandbyValidators.Add("039dafd8571a641058ccc832c5e2111ea39b09c0bde36050914384f7a48bce9bf9");
            configuration.StandbyValidators.Add("038dddc06ce687677a53d54f096d2591ba2302068cf123c1f2d75c2dddc5425579");
            configuration.StandbyValidators.Add("02d02b1873a0863cd042cc717da31cea0d7cf9db32b74d4c72c01b0011503e2e22");
            configuration.StandbyValidators.Add("034ff5ceeac41acf22cd5ed2da17a6df4dd8358fcb2bfb1a43208ad0feaab2746b");

            configuration.SeedList.Add(new IPEndPoint(IPAddress.Parse("seed1.neo.org"), 20333));
            configuration.SeedList.Add(new IPEndPoint(IPAddress.Parse("seed2.neo.org"), 20333));
            configuration.SeedList.Add(new IPEndPoint(IPAddress.Parse("seed3.neo.org"), 20333));
            configuration.SeedList.Add(new IPEndPoint(IPAddress.Parse("seed4.neo.org"), 20333));
            configuration.SeedList.Add(new IPEndPoint(IPAddress.Parse("seed5.neo.org"), 20333));


            return configuration;
        }

        public static NetworkConfiguration CozTestNet()
        {
            var configuration = new NetworkConfiguration();
            configuration.Magic = 1010102;
            configuration.NodePort = 20333;
            configuration.WsPort = 20334;

            configuration.StandbyValidators.Add("032d9e51c7d48b0f5cc63d63deb89767685832cf69eb7113900290f217ae0504ee");
            configuration.StandbyValidators.Add("022a5b7ccf03166a95e1750f0c350c734c34fe7aac66622eecdb5a529d2e69b1df");
            configuration.StandbyValidators.Add("03c478d43271c297696ee3ab5a7946ee60287015c7dca6cba867819c7f271bc4ea");
            configuration.StandbyValidators.Add("0393ef777d01fb60eef1da3474b975c6a393b464bcfe588e2ad7dbc4dbdfa2c244");

            configuration.SeedList.Add(new IPEndPoint(IPAddress.Parse("188.68.34.29"), 10330));
            configuration.SeedList.Add(new IPEndPoint(IPAddress.Parse("188.68.34.29"), 10332));
            configuration.SeedList.Add(new IPEndPoint(IPAddress.Parse("188.68.34.29"), 10334));
            configuration.SeedList.Add(new IPEndPoint(IPAddress.Parse("188.68.34.29"), 10336));


            return configuration;
        }
    }
}