using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BOFNET.Bofs
{
    public class PatchEnvironmentExit : BeaconObject
    {
        public PatchEnvironmentExit(BeaconApi api) : base(api) { }
        public override void Go(string[] args)
        {
            if (Runtime.PatchEnvironmentExit()) {
                BeaconConsole.WriteLine($"[+] Environment.Exit() patched successfully");
            }
            else {
                BeaconConsole.WriteLine($"[!] Environment.Exit() patched failed");
            }
        }
    }
}
