using System.IO;
using System.Text;

namespace BOFNET {
    public abstract class BeaconOutputWriter : StreamWriter {

        protected BeaconOutputWriter(Stream stream) : base(stream) {
            AutoFlush = false;
        }

        public override Encoding Encoding {
            get { return Encoding.UTF8; }
        }
    }
}
