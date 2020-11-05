
using System;
using System.IO;
using System.Text;

namespace BOFNET {
    public class BeaconJobWriter
        : BeaconOutputWriter {

        public string ConsoleText {
            get {
                MemoryStream ms = (MemoryStream)BaseStream;
                if(ms.Position > 0) {
                    byte[] data = new byte[ms.Position];
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.Read(data, 0, data.Length);
                    ms.Seek(0, SeekOrigin.Begin);
                    return Encoding.GetString(data);
                } else {
                    return "";
                }
            }               
        }        
        public bool HasData { get => ((MemoryStream)BaseStream).Position > 0; }
            
        public BeaconJobWriter() : base(new MemoryStream()) {
            AutoFlush = true;
        }
    }
}
