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

Once the runtime has loaded, you can proceed to load further .NET assemblies including other BOF.NET classes.  BOF.NET now chunks the loading of Assemblies, therefore large assemblies can also be loaded (1M+)

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

## Cobalt Strike Client Integrations

The BeaconObject class implements functionality to allow custom implementations of screen capture, file downloads (from memory 😊), keylogger and hash dumps.  If, for example, the built in keylogger or screen capture implementation is causing Windows Defender or other AV engines to kill your beacon, you can implement your own.  The relevant functions are documented below.

```C#
void SendScreenShot(byte[] jpgData, int session, string userName, string title)
```
* `jpgData` Raw JPEG image data.
* `session` User session id the screen capture was taken from.
* `userName` The user name running under the session.
* `title` The title of the window to name for the screen shot.

```C#
SendKeystrokes(string keys, int session, string userName, string title)
```

* `keys` The sequence of keys captured.
* `session` User session id the screen capture was taken from.
* `userName` The user name running under the session.
* `title` The title of the window to application the keys were captured from.

```C#
DownloadFile(string fileName, Stream fileData)
```

* `fileName` The file name to use for the metadata within beacon.
* `fileData` A readable stream that will be used for the file content.

`DownloadFile` will lock beacon and become unresponsive until the download completes!


```C#
SendHashes(UserHash[] userHashes)
```

* `userHashes` A collection of usernames that have been captured.

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
| bofnet_jobstatus *job_id*              | Dump any pending console buffer from the background job                  |
| bofnet_jobkill *job_id*                | Dump any pending console buffer from the background job then kill it.  Warning, can cause deadlocks when terminating a thread that have transitioned into native code                  |
| bofnet_boo *booscript.boo*             | Compile and execute Boo script in seperate temporary AppDomain           |
| bofnet_executeassembly *assembly_name* [*args*]    | Execute a standard .NET assembly calling the entry point, supplying optional arguments           |    

## Caveats

Depending on the target operating system will depend on which distribution should be used (net20/net461).  The runtime will attempt to create a .NET v4 CLR using the `CLRCreateInstance` function that was made available as part of .NET v4.  If the function cannot be found, the older mechanism is used to initialise .NET v2.  Currently the native component cannot determine which managed runtime to load dynamically, so make sure you use the correct distribution folder.  A fully up to date Windows 7 will generally have .NET 4 installed, so on most occasions you will need the net461 folder from inside the dist folder.  Older operating systems like XP will depend on what is installed.

BOF.NET will follow the same restrictions as it's native BOF counterpart.  Execution of a BOF.NET class internally uses the `inline_execute` functionality.  Therefore, any BOF.NET invocations will block beacon until it finishes.  

BOF.NET does have the added benefit that loaded assemblies remain persistent.  This facilitates the use of threads within your BOF.NET class without the worry of the assembly being unloaded after the `Go` function finishes. But you **cannot** write to the beacon console or use any other beacon BOF API's since these are long gone and released by Cobalt Strike after the BOF returns.

If you want to execute your BOF.NET class as a background job using a thread, use the `bofnet_job` command.  This wraps the invocation in a separate thread and handles `BeaconConsole` writes transparently for you.  Be careful with long running jobs and lots of console output, since the console buffer will cached until a call to `bofnet_jobstatus` is invoked.  

## How BOF.NET Works?

BOF.NET contains a small native BOF that acts as a bridge into the managed world.  When `bofnet_init` is called, this will start the managed CLR runtime within the process that beacon is running from.  Once the CLR is started, a separate .NET AppDomain is created to host all assemblies loaded by BOF.NET.  Following on from this, the BOF.NET runtime assembly is loaded into the AppDomain from memory to facilitate the remaining features of BOF.NET.  No .NET assemblies are loaded from disk.

All future BOF.NET calls from here on out are typically handled by the `InvokeBof` method from the `BOFNET.Runtime` class.  This keeps the native BOF code small and concise and pushes all runtime logic into the managed BOF.NET runtime.

## Building

BOF.NET uses the CMake build system along with MinGW GCC compiler for generating BOF files and uses the .NET core msbuild project type for building the managed runtime.  So prior to building, all these prerequisites need to be satisfied and available on the PATH.

From the root of the checkout directory, issue the following commands:

### Windows

```shell
mkdir build
cd build
cmake -DCMAKE_BUILD_TYPE=MinSizeRel -G "MinGW Makefiles" ..
cmake --build .
cmake --install .
```

### Linux

On Linux we utilise a CMake toolchain file to cross compile the native BOF object using the mingw compiler.  For the managed component, please make sure the dotnet command line tool is also installed from .NET core

```shell
mkdir build
cd build
cmake -DCMAKE_INSTALL_PREFIX=$PWD/install -DCMAKE_BUILD_TYPE=MinSizeRel -DCMAKE_TOOLCHAIN_FILE=../toolchain/Linux-mingw64.cmake ..
cmake --build .
cmake --install .
```

### Docker

If you'd rather build using a docker image on Linux with all the build dependencies pre installed, you can use the `ccob/windows_cross` image.

```shell
docker run --rm -it -v $(pwd):/root/bofnet ccob/windows_cross:latest /bin/bash -c "cd /root/bofnet; mkdir build; cd build; cmake -DCMAKE_INSTALL_PREFIX=$PWD/install -DCMAKE_BUILD_TYPE=MinSizeRel -DCMAKE_TOOLCHAIN_FILE=../toolchain/Linux-mingw64.cmake ..; cmake --build .; cmake --install ."
```

Once the steps are complete, the `build\dist` folder should contain the artifacts of the build and should be ready to use within Cobalt Strike

## References

* https://modexp.wordpress.com/2019/05/10/dotnet-loader-shellcode/ - CLR creation using native raw COM interfaces
* https://gist.github.com/sysenter-eip/1a985a224c67aa78f62be83f190b6e86 - Great trick for declaring BOF imports
