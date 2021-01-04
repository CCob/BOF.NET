using System.IO;

namespace BOFNET {
    public abstract class BeaconObject {

        public Runtime.LoadAssembyInAppDomainDelegate LoadAssembyInAppDomain { get; }

        public TextWriter BeaconConsole { get; }

        public BeaconUseToken BeaconUseToken { get; }

        public BeaconRevertToken BeaconRevertToken { get; }

        public BeaconObject(BeaconApi api) {
            BeaconConsole = api.Console;
            BeaconUseToken = api.BeaconUseToken;
            BeaconRevertToken = api.BeaconRevertToken;
            LoadAssembyInAppDomain = api.LoadAssemblyInAppDomain;
        }
  
        public virtual void Go(string[] _) {}

        public virtual void Go(byte[] _) { }
    }
}
