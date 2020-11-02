using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BOFNET {
    public class DefaultBeaconApi : BeaconApi {
        public TextWriter Console { get; }

        public DefaultBeaconApi(BeaconConsoleWriter consoleWriter) {
            this.Console = consoleWriter;
        }
    }
}
