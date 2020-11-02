using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BOFNET.Bofs {
    public class Initializer : BeaconObject {

        public Initializer(BeaconApi api) : base(api) { }   

        public override void Go(byte[] assemblyData) {
            BeaconConsole.WriteLine($"[+] BOFNET Runtime Initalized, assembly size {assemblyData.Length}");
        }
    }
}
