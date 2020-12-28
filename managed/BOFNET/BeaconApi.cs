using System.IO;


namespace BOFNET {
    public interface BeaconApi {
        TextWriter Console { get; }
        Runtime.LoadAssembyInAppDomainDelegate LoadAssemblyInAppDomain { get; }
    }
}
