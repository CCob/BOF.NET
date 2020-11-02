using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BOFNET.Bofs {
    public class ListAssemblies : BeaconObject {
        public ListAssemblies(BeaconApi api) : base(api) {}

        public override void Go(string[] _) {
            foreach(KeyValuePair<string, Assembly> assembly in Runtime.LoadedAssemblies) {
                BeaconConsole.WriteLine($"{assembly.Key}: {assembly.Value.FullName}");
            }           
        }
    }
}
