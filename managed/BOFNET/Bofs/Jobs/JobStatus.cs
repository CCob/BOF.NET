

namespace BOFNET.Bofs.Jobs {
    public class JobStatus : BeaconObject {
        public JobStatus(BeaconApi api) : base(api) {
        }

        public override void Go(string[] args) {

            if(args.Length == 0) {
                BeaconConsole.WriteLine("[!] No job id given, can't give you job status");
                return;                
            }

            int jobId = int.Parse(args[0]);

            if(!Runtime.Jobs.TryGetValue(jobId, out BeaconJob job)){
                BeaconConsole.WriteLine($"[!] Job with id {jobId} doesn't exist");
                return;
            }

            BeaconConsole.WriteLine(job);
            
            if(job.BeaconConsole.HasData)
                BeaconConsole.WriteLine(job.BeaconConsole.ConsoleText);

            if (!job.Thread.IsAlive) {
                BeaconConsole.WriteLine("[+] Job completed and console drained, removing from active job list");
                Runtime.Jobs.Remove(jobId);
            }
        }
    }
}
