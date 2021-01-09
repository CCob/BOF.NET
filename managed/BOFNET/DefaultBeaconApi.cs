using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BOFNET {
    public class DefaultBeaconApi : BeaconApi {
        public TextWriter Console { get; }

        public Runtime.InitialiseChildBOFNETAppDomain InitialiseChildBOFNETAppDomain { get; }

        public BeaconUseToken BeaconUseToken { get; }

        public BeaconRevertToken BeaconRevertToken { get; }

        public DefaultBeaconApi(BeaconOutputWriter consoleWriter, Runtime.InitialiseChildBOFNETAppDomain initialiseChildBOFNETAppDomain, 
            BeaconUseToken beaconUseToken, BeaconRevertToken beaconRevertToken) {

            this.Console = consoleWriter;
            this.InitialiseChildBOFNETAppDomain = initialiseChildBOFNETAppDomain;
            this.BeaconUseToken = beaconUseToken;
            this.BeaconRevertToken = beaconRevertToken;
        }
    }
}
