using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BOFNET.Bofs {
    public class VFS : BeaconObject {
        public struct VirtualFile {
            public string ContentType;
            public byte[] Data;
        }
        public static Dictionary<string, VirtualFile> VirtualFiles { get; private set; } = new Dictionary<string, VirtualFile>();

        public VFS(BeaconApi api) : base(api) {
        }

        public override void Go(string[] args) {

            if (args.Length == 0) {
                BeaconConsole.WriteLine("[!] Usage: vfs command [args]");
                return;
            }

            string command = args[0];

            if (command == "list") {
                foreach (var vf in VirtualFiles) {
                    BeaconConsole.WriteLine($"{vf.Key} {vf.Value.ContentType} {vf.Value.Data.Length / 1024.0:0.00}KB");
                }

            } else {

                if (args.Length < 2) {
                    BeaconConsole.WriteLine($"[!] Usage: vfs {command} file_name");
                    return;
                }

                string fileName = args[1];
                if (!VirtualFiles.ContainsKey(fileName)) {
                    BeaconConsole.WriteLine($"[!] File {fileName} does not exist within the VFS store");
                    return;
                }

                if (command == "download") {
                    var virtualFile = VirtualFiles[fileName];
                    DownloadFile(fileName, new MemoryStream(virtualFile.Data));
                } else if (command == "delete") {
                    VirtualFiles.Remove(fileName);
                    BeaconConsole.WriteLine($"[+] File {fileName} removed from VFS store");
                }
            }
        }

        public override void Go(byte[] raw_args) {

            List<object> args = new Unpack("bZZ", raw_args).Values;

            var data = (byte[])args[0];
            var fileName = (string)args[1];
            var contentType = (string)args[2];

            var vf = new VirtualFile {
                Data = (byte[])args[0],
                ContentType = (string)args[2]
            };

            if (VirtualFiles.ContainsKey(fileName)) {
                BeaconConsole.WriteLine($"[+] File {fileName} updated, Content-Type: {contentType}, Content-Length:{data.Length}");
                VirtualFiles[fileName] = vf;
            } else {
                VirtualFiles.Add(fileName, vf);
                BeaconConsole.WriteLine($"[+] File {fileName} added, Content-Type: {contentType}, Content-Length:{data.Length}");
            }
        }
    }
}
