using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BOFNET.Bofs {
    public class AssemblyLoader : BeaconObject {
        public AssemblyLoader(BeaconApi api) : base(api) {
        }

        public override void Go(byte[] assemblyData) {
            Assembly assembly = Runtime.LoadAssembly(assemblyData);
            BeaconConsole.WriteLine($"[+] Loaded assembly {assembly.FullName} successfully");

            var assemblyLoaderType = assembly.GetType("Costura.AssemblyLoader", false);
            if (assemblyLoaderType != null) {
                BeaconConsole.WriteLine($"[=] Assembly has been prepared with Costura, running Cosutra embedded assembly loader");
                var attachMethod = assemblyLoaderType?.GetMethod("Attach", BindingFlags.Static | BindingFlags.Public);
                attachMethod?.Invoke(null, new object[] { });
            }
        }
    }
}
