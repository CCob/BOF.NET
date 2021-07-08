
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace BOFNET {
    public class BeaconConsoleWriter : BeaconOutputWriter {

        private class BeaconStream : MemoryStream {

            object syncLock = new object();

            public uint FlushTrigger { get; set; } = 4096;

            Thread ownerThread;
            BeaconCallbackWriter beaconCallbackWriter;

            public BeaconStream(BeaconCallbackWriter beaconCallbackWriter, Thread ownerThread) {
                this.beaconCallbackWriter = beaconCallbackWriter;
                this.ownerThread = ownerThread;
            }

            public override void Write(byte[] buffer, int offset, int count) {
                lock (syncLock) {
                    base.Write(buffer, offset, count);
                    if (Position > FlushTrigger) {
                        Flush();
                    }
                }
            }

            public override void Flush() {
                base.Flush();

                if (Position > 0 && beaconCallbackWriter != null && ownerThread == Thread.CurrentThread) {
                    byte[] data = new byte[Position];
                    Seek(0, SeekOrigin.Begin);
                    Read(data, 0, data.Length);
                    beaconCallbackWriter(OutputTypes.CALLBACK_OUTPUT_UTF8, data, data.Length);
                    Seek(0, SeekOrigin.Begin);
                }                            
            }

            public override void Close() {
                beaconCallbackWriter = null;
            }
        }

        public BeaconConsoleWriter(BeaconCallbackWriter beaconConsoleWriter) : base(new BeaconStream(beaconConsoleWriter, Thread.CurrentThread)){
        }
 
        protected override void Dispose(bool disposing) {
            Flush();
            base.Dispose(disposing);
        }
    }
}
