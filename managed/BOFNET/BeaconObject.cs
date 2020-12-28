using System.IO;

namespace BOFNET {
    public abstract class BeaconObject {

        public BeaconApi Api { get; }

        public Runtime.LoadAssembyInAppDomainDelegate LoadAssembyInAppDomain { get; }

        public TextWriter BeaconConsole { get; }

        public BeaconObject(BeaconApi api) {
            BeaconConsole = api.Console;
            LoadAssembyInAppDomain = api.LoadAssemblyInAppDomain;
        }
  
        public virtual void Go(string[] _) {}

        public virtual void Go(byte[] _) { }
    }
}
