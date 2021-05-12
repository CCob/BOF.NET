using System.IO;

namespace BOFNET.Bofs.Boo {
    public interface BooExecutor {
        void Init(TextWriter beaconConsole);
        string Execute(string code, string[] args);
    }
}
