
using System;
using System.Threading;

namespace BOFNET {
    public class BeaconJob {

        BeaconObject BeaconObject { get; set; }

        public Thread Thread { get; private set; }
       
        public BeaconJobWriter BeaconConsole { get; private set; }

        public BeaconJob(BeaconObject bo, string[] args, BeaconJobWriter beaconTaskWriter) {

            BeaconConsole = beaconTaskWriter;
            BeaconObject = bo;
            Thread = new Thread(new ParameterizedThreadStart(this.DoTask));
            Thread.Start(args);
        }

        private void DoTask(object args) {
            try {
                if (args is string[] stringArgs) {
                    BeaconObject.Go(stringArgs);
                }
            }catch(Exception e) {
                BeaconConsole.WriteLine($"Job execution failed with exception:\n{e}");
            }
        }

        public override string ToString() {
            return $"Type: {BeaconObject.GetType().Name}, Id: {Thread.ManagedThreadId}, Active: {Thread.IsAlive}, Console Data: {BeaconConsole.HasData}";
        }
    }
}
