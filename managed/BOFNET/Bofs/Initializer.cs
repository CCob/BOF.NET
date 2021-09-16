using System;
using System.Reflection;

namespace BOFNET.Bofs {
    public class Initializer : BeaconObject {

        public Initializer(BeaconApi api) : base(api) { }   

        public override void Go(byte[] assemblyData) {
            Runtime.RegisterRuntimeAssembly(assemblyData);
            BeaconConsole.WriteLine($"[+] BOFNET Runtime Initalized, assembly size {assemblyData.Length}, .NET Runtime Version: {Environment.Version} in AppDomain {AppDomain.CurrentDomain.FriendlyName}");
            if (Runtime.PatchEnvironmentExit()) {
                BeaconConsole.WriteLine($"[+] Environment.Exit() patched successfully");
            }
            else {
                BeaconConsole.WriteLine($"[!] Environment.Exit() patched failed");
            }
        }
    }
}
