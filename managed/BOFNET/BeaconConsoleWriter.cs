
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

                    bool currentSeekHit = false;
                    int stringIndex = 0;
                    for (int index = 0; index < (data.Length - 2); index++)
                    {
                        if (currentSeekHit != true)
                        {
                            /*  
                            *   Logical to check for sequential null values that go beyond our buffer length.
                            *   Due to representation of null bytes in a UTF-8 callback, this can otherwise lead to "phantom" output which is the flushing of an allocated MemoryStream object.
                            *
                            *   As such, a "beacon operator" may receive multiple instances of "received output" from the callback.
                            *   We implement our own needle here, assuming in both UTF-8 and UTF-16 arrays that sequential null bytes represent the termination of any string.
                            *
                            *   This prevents both pollution of output, and cleanliness of logs.
                            *   All instances of the base class are still properly flushed at the end of our operations with a call the the base class' Dispose method.
                            */
                            if ((data[index] == (byte)0x00) && (data[index+1] == (byte)0x00))
                            {
                                stringIndex = index + 1;
                                currentSeekHit = true;
                            }
                            else
                            {
                                stringIndex += 1;
                            }
                        }
                    }
                    
                    if (currentSeekHit != true)
                    {
                        beaconCallbackWriter(OutputTypes.CALLBACK_OUTPUT_UTF8, data, data.Length);
                    }
                    else
                    {
                        if (stringIndex >= 2)
                        {
                            beaconCallbackWriter(OutputTypes.CALLBACK_OUTPUT_UTF8, data, stringIndex);
                        }
                    }

                    // Regardless of output, seek to the beginning of the MemoryStream
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
