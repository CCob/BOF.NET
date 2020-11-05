# BOF.NET - A .NET Runtime for Cobalt Strike's Beacon Object Files

## Introduction

BOF.NET is a small native BOF object combined with the BOF.NET managed runtime that enables the development of Cobalt Strike BOFs directly in .NET.  BOF.NET removes the complexity of native compilation along with the headaches of manually importing native API.  Testing BOF.NET assemblies is also generally much easier, since the .NET assemblies can be debugged using a traditional managed debugger.

## Getting started

Implementing you first BOF.NET class is simple.  Add a reference to the BOF.NET runtime DLL from the dist folder and create a class that inherits from `BeaconObject`.  A mandatory constructor with a  `BeaconApi` object as the only parameter is needed.  This should be passed along to the `BeaconObject` base constructor.

Finally override the `Go` function.  Arguments will be pre-processed for you exactly how a `Main` function behaves inside a normal .NET assembly. 

```C#
namespace BOFNET.Bofs {
    public class HelloWorld : BeaconObject {
        public HelloWorld(BeaconApi api) : base(api) { }
        public override void Go(string[] args) {
            BeaconConsole.WriteLine($"[+] Welcome to BOF.NET { (args.Length > 0 ? args[0] : "anonymous" )}");
        }
    }
}
```

Once you have compiled your BOF.NET assembly, you can load the bofnet.cna aggresor script from the dist folder into Cobalt Strike and being using your BOF.NET class.

Before any BOF.NET class can be used, the BOF.NET runtime needs to be initialised within the beacon instance.

```shell
bofnet_init
```

Once the runtime has loaded, you can proceed to load further .NET assemblies including other BOF.NET classes.

```shell
bofnet_load /path/to/bofnet/HelloWorld.dll
```

You can confirm the library has been loaded using the `bofnet_listassemblies` alias.  A complete list of classes that implmements `BeaconObject` can be shows by executing the `bofnet_list` alias.

Finally, once you have confirmed your assembly is loaded and the BOF.NET class is available you can execute it

```shell
bofnet_execute BOFNET.Bofs.HelloWorld @_EthicalChaos_
```

You can also use the shorthand method of just the class name, but this will only work if there is only one BOF.NET class present with that name.

```shell
bofnet_execute HelloWorld @_EthicalChaos_
```

## Beacon Command Reference

| Command                                | Description                                                              |
|----------------------------------------|--------------------------------------------------------------------------|
| bofnet_init                            | Initialises the BOF.NET runtime inside the beacon process                |
| bofnet_list                            | List's all executable BOF.NET classes                                    |
| bofnet_listassembiles                  | List assemblies currently loaded into the BOF.NET runtime                |
| bofnet_execute *bof_name* [*args*]     | Execute a BOF.NET class, supplying optional arguments                    |
| bofnet_load *assembly_path*            | Load an additional .NET assembly from memory into the BOF.NET runtime.   |
| bofnet_shutdown                        | Shutdown the BOF.NET runtime                                             |
| bofnet_job *bof_name* [*args*]         | Execute a BOF.NET class as a background job (thread)                     |
| bofnet_jobs                            | List all currently active BOF.NET jobs                                   |
| bofnet_jobstatus *job_id*               | Dump any pending console buffer from the background job                  |

## Caveats

BOF.NET has only been tested with .NET 2.  There is more work needed for .NET v4+.  There is a high probability of memory leaks and potential vulnerabilities within the native runtime as it has had little testing and needs further polishing.  Use at your own risk!

BOF.NET will follow the same restrictions as it's native BOF counterpart.  Execution of a BOF.NET class internally uses the `inline_execute` functionality.  Therefore, any BOF.NET invocations will block beacon until it finishes.  

BOF.NET does have the added benefit that loaded assemblies remain persistent.  This facilitates the use of threads within your BOF.NET class without the worry of the assembly being unloaded after the `Go` function finishes. But you **cannot** write to the beacon console or use any other beacon BOF API's since these are long gone and released by Cobalt Strike after the BOF returns.

## How BOF.NET Works?

BOF.NET contains a small native BOF that acts as a bridge into the managed world.  When `bofnet_init` is called, this will start the managed CLR runtime within the process that beacon is running from.  Once the CLR is started, a separate .NET AppDomain is created to host all assemblies loaded by BOF.NET.  Following on from this, the BOF.NET runtime assembly is loaded into the AppDomain from memory to facilitate the remaining features of BOF.NET.  No .NET assemblies are loaded from disk.

All future BOF.NET calls from here on out are typically handled by the `InvokeBof` method from the `BOFNET.Runtime` class.  This keeps the native BOF code small and concise and pushes all runtime logic into the managed BOF.NET runtime.

## Building

BOF.NET uses the CMake build system along with MinGW GCC compiler for generating BOF files and uses msbuild for building the managed runtime.  So prior to building, all these prerequisites need to be satisfied and available on the PATH.

From the checkout directory, issue the following commands 

```shell
mkdir build
cd build
cmake -DCMAKE_BUILD_TYPE=MinSizeRel -G "MinGW Makefiles" ..
cmake --build .
cmake --install .
```

Once the steps are complete, the `build\dist` folder should contain the artifacts of the build and should be ready to use within Cobalt Strike

## References

* https://modexp.wordpress.com/2019/05/10/dotnet-loader-shellcode/ - CLR creation using native raw COM interfaces
* https://gist.github.com/sysenter-eip/1a985a224c67aa78f62be83f190b6e86 - Great trick for declaring BOF imports
