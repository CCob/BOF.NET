using BOFNET;
using BOFNET.Bofs.Boo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BOFNET_Tests {
    class Program {

        class ConsoleApi : BeaconApi {
            public TextWriter Console => System.Console.Out;
            public Runtime.InitialiseChildBOFNETAppDomain InitialiseChildBOFNETAppDomain => null;

            public BeaconUseToken BeaconUseToken => throw new NotImplementedException();

            public BeaconRevertToken BeaconRevertToken => throw new NotImplementedException();

            public BeaconCallbackWriter BeaconCallbackWriter => throw new NotImplementedException();
        }


        static void Main(string[] args) {

            BooRunner runner = new BooRunner(new ConsoleApi());
            runner.Go(new string[] { "beaconPrint 'Hello, World!'" });
        }
    }
}
