using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BOFNET.Bofs.Jobs {
    public class JobRunnerAssembly : BeaconObject { 
        public JobRunnerAssembly(BeaconApi api) : base(api) {
        }

        public AppDomain LoadAssemblyInAppDomain(string appDomain, byte[] data, int len) {
            return null;
        }

        public override void Go(string[] args) {
            if (args.Length == 0) {
                BeaconConsole.WriteLine("[!] Cannot continue execution, no .NET assembly specified to run");
                return;
            }

            if (Runtime.Jobs.Values.Where(p => p.StandardAssembly != null && p.Thread.IsAlive).Count() > 0) {
                BeaconConsole.WriteLine("[!] Cannot continue execution, multiple instances of bofnet_job_assembly cannot run in parallel");
                return;
            }

            if (Runtime.LoadedAssemblies.ContainsKey(args[0])) {
                BeaconJobWriter btw = new BeaconJobWriter();
                BeaconJob beaconJob = new BeaconJob(null, args.Skip(1).ToArray(), btw, args[0]);

                Runtime.Jobs[beaconJob.Thread.ManagedThreadId] = beaconJob;
                BeaconConsole.WriteLine($"[+] Started Task {args[0]} with job id {beaconJob.Thread.ManagedThreadId}");
            }
            else {
                BeaconConsole.WriteLine("[!] Cannot continue execution, specified .NET assembly not loaded");
            }
        }
    }
}
