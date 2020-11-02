using System;
using System.IO;
using System.Runtime.InteropServices;

namespace BOFNET {
    public abstract class BeaconObject {

        public BeaconApi Api { get; }

        public TextWriter BeaconConsole { get; }

        public BeaconObject(BeaconApi api) {
            BeaconConsole = api.Console;
        }
  
        public virtual void Go(string[] _) {}

        public virtual void Go(byte[] _) { }
    }
}
