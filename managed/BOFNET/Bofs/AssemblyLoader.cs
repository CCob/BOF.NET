using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BOFNET.Bofs {
    public class AssemblyLoader : BeaconObject {
        public AssemblyLoader(BeaconApi api) : base(api) {
        }

        public override void Go(byte[] assemblyData) {
            Runtime.LoadAssembly(assemblyData);            
        }
    }
}
