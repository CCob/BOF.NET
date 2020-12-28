using System.Runtime.Remoting;

namespace BOFNET.Bofs.Boo {
    public interface BooExecutor {
        void Init(ObjectHandle consoleFacade);
        string Execute(string code);
    }
}
