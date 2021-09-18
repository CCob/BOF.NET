
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace BOFNET {
    public class BeaconJob {

        BeaconObject BeaconObject { get; set; }

        public Thread Thread { get; private set; }
       
        public BeaconJobWriter BeaconConsole { get; private set; }

        public string StandardAssembly { get; private set; }

        string StandardAssemblyEntryPointType { get; set; }

        volatile static ProducerConsumerStream MemoryStreamPC = new ProducerConsumerStream();

        volatile static bool RunThread;

        public BeaconJob(BeaconObject bo, string[] args, BeaconJobWriter beaconTaskWriter, string standardAssembly = null) {
            StandardAssembly = standardAssembly;
            BeaconConsole = beaconTaskWriter;
            BeaconObject = bo;
            Thread = new Thread(new ParameterizedThreadStart(this.DoTask));
            Thread.Start(args);
        }

        private void DoTask(object args) {
            try {
                if (args is string[] stringArgs) {
                    if (StandardAssembly != null) {
                        // Redirect stdout to MemoryStream
                        StreamWriter memoryStreamWriter = new StreamWriter(MemoryStreamPC);
                        memoryStreamWriter.AutoFlush = true;
                        Console.SetOut(memoryStreamWriter);
                        Console.SetError(memoryStreamWriter);

                        // Start thread to check MemoryStream to send data to Beacon
                        RunThread = true;
                        Thread runtimeWriteLine = new Thread(() => RuntimeWriteLine(BeaconConsole));
                        runtimeWriteLine.Start();

                        // Run main program passing original arguments
                        object[] mainArguments = new[] { stringArgs };
                        StandardAssemblyEntryPointType = Runtime.LoadedAssemblies[StandardAssembly].Assembly.EntryPoint.DeclaringType.ToString();
                        Runtime.LoadedAssemblies[StandardAssembly].Assembly.EntryPoint.Invoke(null, mainArguments);

                        // Trigger safe exit of thread, ensuring MemoryStream is emptied too
                        RunThread = false;
                        runtimeWriteLine.Join();
                    }
                    else {
                        BeaconObject.Go(stringArgs);
                    }
                }
            } catch(Exception e) {
                BeaconConsole.WriteLine($"Job execution failed with exception:\n{e}");
            }
        }

        public override string ToString() {
            return $"Type: {(StandardAssembly != null ? StandardAssemblyEntryPointType : BeaconObject.GetType().FullName).ToString()}, Id: {Thread.ManagedThreadId}, Standard Assembly: {(StandardAssembly != null ? true : false).ToString()}, Active: {Thread.IsAlive}, Console Data: {BeaconConsole.HasData}";
        }

        public static void RuntimeWriteLine(BeaconJobWriter beaconconsole) {
            bool LastCheck = false;
            while (RunThread == true || LastCheck == true) {
                int offsetWritten = 0;
                int currentCycleMemoryStreamLength = Convert.ToInt32(MemoryStreamPC.Length);
                if (currentCycleMemoryStreamLength > offsetWritten) {
                    try {
                        var byteArrayRaw = new byte[currentCycleMemoryStreamLength];
                        int count = MemoryStreamPC.Read(byteArrayRaw, offsetWritten, currentCycleMemoryStreamLength);

                        if (count > 0) {
                            // Need to stop at last new line otherwise it will run into encoding errors in the Beacon logs.
                            int lastNewLine = 0;
                            for (int i = 0; i < byteArrayRaw.Length; i++) {
                                if (byteArrayRaw[i] == '\n') {
                                    lastNewLine = i;
                                }
                            }
                            if (LastCheck) {
                                // If last run ensure all remaining MemoryStream data is obtained.
                                lastNewLine = currentCycleMemoryStreamLength;
                            }
                            if (lastNewLine > 0) {
                                var byteArrayToLastNewline = new byte[lastNewLine];
                                Buffer.BlockCopy(byteArrayRaw, 0, byteArrayToLastNewline, 0, lastNewLine);
                                beaconconsole.WriteLine(Encoding.ASCII.GetString(byteArrayToLastNewline));
                                offsetWritten = offsetWritten + lastNewLine;
                            }
                        }
                    }
                    catch (Exception e) {
                        beaconconsole.WriteLine($"[!] BOFNET threw an exception when returning captured console output: {e}");
                    }
                }
                Thread.Sleep(50);
                if (LastCheck) {
                    break;
                }
                if (RunThread == false && LastCheck == false) {
                    LastCheck = true;
                }
            }
        }

        // Code taken from Polity at: https://stackoverflow.com/questions/12328245/memorystream-have-one-thread-write-to-it-and-another-read
        // Provides means to have multiple threads reading and writing from and to the same MemoryStream
        public class ProducerConsumerStream : Stream {
            private readonly MemoryStream innerStream;
            private long readPosition;
            private long writePosition;

            public ProducerConsumerStream() {
                innerStream = new MemoryStream();
            }

            public override bool CanRead { get { return true; } }

            public override bool CanSeek { get { return false; } }

            public override bool CanWrite { get { return true; } }

            public override void Flush() {
                lock (innerStream) {
                    innerStream.Flush();
                }
            }

            public override long Length {
                get {
                    lock (innerStream) {
                        return innerStream.Length;
                    }
                }
            }

            public override long Position {
                get { throw new NotSupportedException(); }
                set { throw new NotSupportedException(); }
            }

            public override int Read(byte[] buffer, int offset, int count) {
                lock (innerStream) {
                    innerStream.Position = readPosition;
                    int red = innerStream.Read(buffer, offset, count);
                    readPosition = innerStream.Position;

                    return red;
                }
            }

            public override long Seek(long offset, SeekOrigin origin) {
                throw new NotSupportedException();
            }

            public override void SetLength(long value) {
                throw new NotImplementedException();
            }

            public override void Write(byte[] buffer, int offset, int count) {
                lock (innerStream) {
                    innerStream.Position = writePosition;
                    innerStream.Write(buffer, offset, count);
                    writePosition = innerStream.Position;
                }
            }
        }
    }
}