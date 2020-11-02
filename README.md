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

| Command                   | Description                                                              |
|---------------------------|--------------------------------------------------------------------------|
| bofnet_init               | Initialises the BOF.NET runtime inside the beacon process                |
| bofnet_list               | List's all executable BOF.NET classes                                    |
| bofnet_listassembiles     | List assemblies currently loaded into the BOF.NET runtime                |
| bofnet_execute [args]     | Execute a BOF.NET class, supplying optional arguments                    |
| bofnet_load assembly_path | Load an additional .NET assembly from memory into the BOF.NET runtime.   |
| bofnet_shutdown           | Shutdown the BOF.NET runtime                                             |

## Caveats

BOF.NET has only been tested with .NET 2.  There is more work needed for .NET v4+.  There is a high probability of memory leaks and potential vulnerabilities within the native runtime as it has had little testing and needs further polishing.  Use at your own risk!

## How BOF.NET Works?

TODO

## Building

TODO

## References

TODO

