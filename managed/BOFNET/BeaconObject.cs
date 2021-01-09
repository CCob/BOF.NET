using System.IO;

namespace BOFNET {
    public abstract class BeaconObject {

        public Runtime.InitialiseChildBOFNETAppDomain InitialiseChildBOFNETAppDomain { get; }

        public TextWriter BeaconConsole { get; }

        public BeaconUseToken BeaconUseToken { get; }

        public BeaconRevertToken BeaconRevertToken { get; }

        public BeaconObject(BeaconApi api) {
            BeaconConsole = api.Console;
            BeaconUseToken = api.BeaconUseToken;
            BeaconRevertToken = api.BeaconRevertToken;
            InitialiseChildBOFNETAppDomain = api.InitialiseChildBOFNETAppDomain;
        }
  
        public virtual void Go(string[] _) {}

        public virtual void Go(byte[] _) { }
    }
}
