using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BOFNET.Bofs.Jobs {
    public class JobList
        : BeaconObject {
        public JobList(BeaconApi api) : base(api) {
        }

        public override void Go(string[] _) {            
            foreach(BeaconJob job in Runtime.Jobs.Values) {
                BeaconConsole.WriteLine(job);
            }
        }
    }
}
