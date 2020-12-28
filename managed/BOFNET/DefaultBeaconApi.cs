using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BOFNET {
    public class DefaultBeaconApi : BeaconApi {
        public TextWriter Console { get; }

        public Runtime.LoadAssembyInAppDomainDelegate LoadAssemblyInAppDomain { get; }

        public DefaultBeaconApi(BeaconOutputWriter consoleWriter, Runtime.LoadAssembyInAppDomainDelegate loadAssembyInAppDomain) {
            this.Console = consoleWriter;
            this.LoadAssemblyInAppDomain = loadAssembyInAppDomain;
        }
    }
}
