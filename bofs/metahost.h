

/* this ALWAYS GENERATED file contains the definitions for the interfaces */


 /* File created by MIDL compiler version 8.01.0622 */
/* at Tue Jan 19 03:14:07 2038
 */
/* Compiler settings for metahost.idl:
    Oicf, W1, Zp8, env=Win32 (32b run), target_arch=X86 8.01.0622 
    protocol : dce , ms_ext, c_ext, robust
    error checks: allocation ref bounds_check enum stub_data 
    VC __declspec() decoration level: 
         __declspec(uuid()), __declspec(selectany), __declspec(novtable)
         DECLSPEC_UUID(), MIDL_INTERFACE()
*/
/* @@MIDL_FILE_HEADING(  ) */

#pragma warning( disable: 4049 )  /* more than 64k source lines */


/* verify that the <rpcndr.h> version is high enough to compile this file*/
#ifndef __REQUIRED_RPCNDR_H_VERSION__
#define __REQUIRED_RPCNDR_H_VERSION__ 475
#endif

#include "rpc.h"
#include "rpcndr.h"

#ifndef __RPCNDR_H_VERSION__
#error this stub requires an updated version of <rpcndr.h>
#endif /* __RPCNDR_H_VERSION__ */

#ifndef COM_NO_WINDOWS_H
#include "windows.h"
#include "ole2.h"
#endif /*COM_NO_WINDOWS_H*/

#ifndef __metahost_h__
#define __metahost_h__

#if defined(_MSC_VER) && (_MSC_VER >= 1020)
#pragma once
#endif

/* Forward Declarations */ 

#ifndef __ICLRRuntimeInfo_FWD_DEFINED__
#define __ICLRRuntimeInfo_FWD_DEFINED__
typedef interface ICLRRuntimeInfo ICLRRuntimeInfo;

#endif 	/* __ICLRRuntimeInfo_FWD_DEFINED__ */


#ifndef __ICLRMetaHost_FWD_DEFINED__
#define __ICLRMetaHost_FWD_DEFINED__
typedef interface ICLRMetaHost ICLRMetaHost;

#endif 	/* __ICLRMetaHost_FWD_DEFINED__ */


/* header files for imported files */
#include "unknwn.h"
#include "oaidl.h"
#include "ocidl.h"
#include "mscoree.h"

#ifdef __cplusplus
extern "C"{
#endif 


/* interface __MIDL_itf_metahost_0000_0000 */
/* [local] */ 

#ifdef WINE_NO_UNICODE_MACROS
#undef LoadLibrary
#endif


extern RPC_IF_HANDLE __MIDL_itf_metahost_0000_0000_v0_0_c_ifspec;
extern RPC_IF_HANDLE __MIDL_itf_metahost_0000_0000_v0_0_s_ifspec;

#ifndef __ICLRRuntimeInfo_INTERFACE_DEFINED__
#define __ICLRRuntimeInfo_INTERFACE_DEFINED__

/* interface ICLRRuntimeInfo */
/* [uuid][local][object] */ 


EXTERN_C const IID IID_ICLRRuntimeInfo;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("bd39d1d2-ba2f-486a-89b0-b4b0cb466891")
    ICLRRuntimeInfo : public IUnknown
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE GetVersionString( 
            /* [size_is][out] */ LPWSTR pwzBuffer,
            /* [out][in] */ DWORD *pcchBuffer) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetRuntimeDirectory( 
            /* [size_is][out] */ LPWSTR pwzBuffer,
            /* [out][in] */ DWORD *pcchBuffer) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE IsLoaded( 
            /* [in] */ HANDLE hndProcess,
            /* [retval][out] */ BOOL *pbLoaded) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE LoadErrorString( 
            /* [in] */ UINT iResourceID,
            /* [size_is][out] */ LPWSTR pwzBuffer,
            /* [out][in] */ DWORD *pcchBuffer,
            /* [in] */ LONG iLocaleid) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE LoadLibrary( 
            /* [in] */ LPCWSTR pwzDllName,
            /* [retval][out] */ HMODULE *phndModule) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetProcAddress( 
            /* [in] */ LPCSTR pszProcName,
            /* [retval][out] */ LPVOID *ppProc) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetInterface( 
            /* [in] */ REFCLSID rclsid,
            /* [in] */ REFIID riid,
            /* [retval][iid_is][out] */ LPVOID *ppUnk) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE IsLoadable( 
            /* [retval][out] */ BOOL *pbLoadable) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE SetDefaultStartupFlags( 
            /* [in] */ DWORD dwStartupFlags,
            /* [in] */ LPCWSTR pwzHostConfigFile) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetDefaultStartupFlags( 
            /* [out] */ DWORD *pdwStartupFlags,
            /* [size_is][out] */ LPWSTR pwzHostConfigFile,
            /* [out][in] */ DWORD *pcchHostConfigFile) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE BindAsLegacyV2Runtime( void) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE IsStarted( 
            /* [out] */ BOOL *pbStarted,
            /* [out] */ DWORD *pdwStartupFlags) = 0;
        
    };
    
    
#else 	/* C style interface */

    typedef struct ICLRRuntimeInfoVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            ICLRRuntimeInfo * This,
            /* [in] */ REFIID riid,
            /* [annotation][iid_is][out] */ 
            _COM_Outptr_  void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            ICLRRuntimeInfo * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            ICLRRuntimeInfo * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetVersionString )( 
            ICLRRuntimeInfo * This,
            /* [size_is][out] */ LPWSTR pwzBuffer,
            /* [out][in] */ DWORD *pcchBuffer);
        
        HRESULT ( STDMETHODCALLTYPE *GetRuntimeDirectory )( 
            ICLRRuntimeInfo * This,
            /* [size_is][out] */ LPWSTR pwzBuffer,
            /* [out][in] */ DWORD *pcchBuffer);
        
        HRESULT ( STDMETHODCALLTYPE *IsLoaded )( 
            ICLRRuntimeInfo * This,
            /* [in] */ HANDLE hndProcess,
            /* [retval][out] */ BOOL *pbLoaded);
        
        HRESULT ( STDMETHODCALLTYPE *LoadErrorString )( 
            ICLRRuntimeInfo * This,
            /* [in] */ UINT iResourceID,
            /* [size_is][out] */ LPWSTR pwzBuffer,
            /* [out][in] */ DWORD *pcchBuffer,
            /* [in] */ LONG iLocaleid);
        
        HRESULT ( STDMETHODCALLTYPE *LoadLibrary )( 
            ICLRRuntimeInfo * This,
            /* [in] */ LPCWSTR pwzDllName,
            /* [retval][out] */ HMODULE *phndModule);
        
        HRESULT ( STDMETHODCALLTYPE *GetProcAddress )( 
            ICLRRuntimeInfo * This,
            /* [in] */ LPCSTR pszProcName,
            /* [retval][out] */ LPVOID *ppProc);
        
        HRESULT ( STDMETHODCALLTYPE *GetInterface )( 
            ICLRRuntimeInfo * This,
            /* [in] */ REFCLSID rclsid,
            /* [in] */ REFIID riid,
            /* [retval][iid_is][out] */ LPVOID *ppUnk);
        
        HRESULT ( STDMETHODCALLTYPE *IsLoadable )( 
            ICLRRuntimeInfo * This,
            /* [retval][out] */ BOOL *pbLoadable);
        
        HRESULT ( STDMETHODCALLTYPE *SetDefaultStartupFlags )( 
            ICLRRuntimeInfo * This,
            /* [in] */ DWORD dwStartupFlags,
            /* [in] */ LPCWSTR pwzHostConfigFile);
        
        HRESULT ( STDMETHODCALLTYPE *GetDefaultStartupFlags )( 
            ICLRRuntimeInfo * This,
            /* [out] */ DWORD *pdwStartupFlags,
            /* [size_is][out] */ LPWSTR pwzHostConfigFile,
            /* [out][in] */ DWORD *pcchHostConfigFile);
        
        HRESULT ( STDMETHODCALLTYPE *BindAsLegacyV2Runtime )( 
            ICLRRuntimeInfo * This);
        
        HRESULT ( STDMETHODCALLTYPE *IsStarted )( 
            ICLRRuntimeInfo * This,
            /* [out] */ BOOL *pbStarted,
            /* [out] */ DWORD *pdwStartupFlags);
        
        END_INTERFACE
    } ICLRRuntimeInfoVtbl;

    interface ICLRRuntimeInfo
    {
        CONST_VTBL struct ICLRRuntimeInfoVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define ICLRRuntimeInfo_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define ICLRRuntimeInfo_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define ICLRRuntimeInfo_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define ICLRRuntimeInfo_GetVersionString(This,pwzBuffer,pcchBuffer)	\
    ( (This)->lpVtbl -> GetVersionString(This,pwzBuffer,pcchBuffer) ) 

#define ICLRRuntimeInfo_GetRuntimeDirectory(This,pwzBuffer,pcchBuffer)	\
    ( (This)->lpVtbl -> GetRuntimeDirectory(This,pwzBuffer,pcchBuffer) ) 

#define ICLRRuntimeInfo_IsLoaded(This,hndProcess,pbLoaded)	\
    ( (This)->lpVtbl -> IsLoaded(This,hndProcess,pbLoaded) ) 

#define ICLRRuntimeInfo_LoadErrorString(This,iResourceID,pwzBuffer,pcchBuffer,iLocaleid)	\
    ( (This)->lpVtbl -> LoadErrorString(This,iResourceID,pwzBuffer,pcchBuffer,iLocaleid) ) 

#define ICLRRuntimeInfo_LoadLibrary(This,pwzDllName,phndModule)	\
    ( (This)->lpVtbl -> LoadLibrary(This,pwzDllName,phndModule) ) 

#define ICLRRuntimeInfo_GetProcAddress(This,pszProcName,ppProc)	\
    ( (This)->lpVtbl -> GetProcAddress(This,pszProcName,ppProc) ) 

#define ICLRRuntimeInfo_GetInterface(This,rclsid,riid,ppUnk)	\
    ( (This)->lpVtbl -> GetInterface(This,rclsid,riid,ppUnk) ) 

#define ICLRRuntimeInfo_IsLoadable(This,pbLoadable)	\
    ( (This)->lpVtbl -> IsLoadable(This,pbLoadable) ) 

#define ICLRRuntimeInfo_SetDefaultStartupFlags(This,dwStartupFlags,pwzHostConfigFile)	\
    ( (This)->lpVtbl -> SetDefaultStartupFlags(This,dwStartupFlags,pwzHostConfigFile) ) 

#define ICLRRuntimeInfo_GetDefaultStartupFlags(This,pdwStartupFlags,pwzHostConfigFile,pcchHostConfigFile)	\
    ( (This)->lpVtbl -> GetDefaultStartupFlags(This,pdwStartupFlags,pwzHostConfigFile,pcchHostConfigFile) ) 

#define ICLRRuntimeInfo_BindAsLegacyV2Runtime(This)	\
    ( (This)->lpVtbl -> BindAsLegacyV2Runtime(This) ) 

#define ICLRRuntimeInfo_IsStarted(This,pbStarted,pdwStartupFlags)	\
    ( (This)->lpVtbl -> IsStarted(This,pbStarted,pdwStartupFlags) ) 

#endif /* COBJMACROS */


#endif 	/* C style interface */




#endif 	/* __ICLRRuntimeInfo_INTERFACE_DEFINED__ */


/* interface __MIDL_itf_metahost_0000_0001 */
/* [local] */ 

typedef HRESULT ( __stdcall *CallbackThreadSetFnPtr )( void);

typedef HRESULT ( __stdcall *CallbackThreadUnsetFnPtr )( void);

typedef void ( __stdcall *RuntimeLoadedCallbackFnPtr )( 
    ICLRRuntimeInfo *pRuntimeInfo,
    CallbackThreadSetFnPtr pfnCallbackThreadSet,
    CallbackThreadUnsetFnPtr pfnCallbackThreadUnset);

DEFINE_GUID(CLSID_CLRDebuggingLegacy, 0xDF8395B5,0xA4BA,0x450b,0xA7,0x7C,0xA9,0xA4,0x77,0x62,0xC5,0x20);
DEFINE_GUID(CLSID_CLRMetaHost, 0x9280188d,0x0e8e,0x4867,0xb3,0x0c,0x7f,0xa8,0x38,0x84,0xe8,0xde);


extern RPC_IF_HANDLE __MIDL_itf_metahost_0000_0001_v0_0_c_ifspec;
extern RPC_IF_HANDLE __MIDL_itf_metahost_0000_0001_v0_0_s_ifspec;

#ifndef __ICLRMetaHost_INTERFACE_DEFINED__
#define __ICLRMetaHost_INTERFACE_DEFINED__

/* interface ICLRMetaHost */
/* [uuid][local][object] */ 


EXTERN_C const IID IID_ICLRMetaHost;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("d332db9e-b9b3-4125-8207-a14884f53216")
    ICLRMetaHost : public IUnknown
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE GetRuntime( 
            /* [in] */ LPCWSTR pwzVersion,
            /* [in] */ REFIID iid,
            /* [retval][iid_is][out] */ LPVOID *ppRuntime) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetVersionFromFile( 
            /* [in] */ LPCWSTR pwzFilePath,
            /* [size_is][out] */ LPWSTR pwzBuffer,
            /* [out][in] */ DWORD *pcchBuffer) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE EnumerateInstalledRuntimes( 
            /* [retval][out] */ IEnumUnknown **ppEnumerator) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE EnumerateLoadedRuntimes( 
            /* [in] */ HANDLE hndProcess,
            /* [retval][out] */ IEnumUnknown **ppEnumerator) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE RequestRuntimeLoadedNotification( 
            /* [in] */ RuntimeLoadedCallbackFnPtr pCallbackFunction) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE QueryLegacyV2RuntimeBinding( 
            /* [in] */ REFIID riid,
            /* [retval][iid_is][out] */ LPVOID *ppUnk) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE ExitProcess( 
            /* [in] */ INT32 iExitCode) = 0;
        
    };
    
    
#else 	/* C style interface */

    typedef struct ICLRMetaHostVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            ICLRMetaHost * This,
            /* [in] */ REFIID riid,
            /* [annotation][iid_is][out] */ 
            _COM_Outptr_  void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            ICLRMetaHost * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            ICLRMetaHost * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetRuntime )( 
            ICLRMetaHost * This,
            /* [in] */ LPCWSTR pwzVersion,
            /* [in] */ REFIID iid,
            /* [retval][iid_is][out] */ LPVOID *ppRuntime);
        
        HRESULT ( STDMETHODCALLTYPE *GetVersionFromFile )( 
            ICLRMetaHost * This,
            /* [in] */ LPCWSTR pwzFilePath,
            /* [size_is][out] */ LPWSTR pwzBuffer,
            /* [out][in] */ DWORD *pcchBuffer);
        
        HRESULT ( STDMETHODCALLTYPE *EnumerateInstalledRuntimes )( 
            ICLRMetaHost * This,
            /* [retval][out] */ IEnumUnknown **ppEnumerator);
        
        HRESULT ( STDMETHODCALLTYPE *EnumerateLoadedRuntimes )( 
            ICLRMetaHost * This,
            /* [in] */ HANDLE hndProcess,
            /* [retval][out] */ IEnumUnknown **ppEnumerator);
        
        HRESULT ( STDMETHODCALLTYPE *RequestRuntimeLoadedNotification )( 
            ICLRMetaHost * This,
            /* [in] */ RuntimeLoadedCallbackFnPtr pCallbackFunction);
        
        HRESULT ( STDMETHODCALLTYPE *QueryLegacyV2RuntimeBinding )( 
            ICLRMetaHost * This,
            /* [in] */ REFIID riid,
            /* [retval][iid_is][out] */ LPVOID *ppUnk);
        
        HRESULT ( STDMETHODCALLTYPE *ExitProcess )( 
            ICLRMetaHost * This,
            /* [in] */ INT32 iExitCode);
        
        END_INTERFACE
    } ICLRMetaHostVtbl;

    interface ICLRMetaHost
    {
        CONST_VTBL struct ICLRMetaHostVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define ICLRMetaHost_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define ICLRMetaHost_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define ICLRMetaHost_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define ICLRMetaHost_GetRuntime(This,pwzVersion,iid,ppRuntime)	\
    ( (This)->lpVtbl -> GetRuntime(This,pwzVersion,iid,ppRuntime) ) 

#define ICLRMetaHost_GetVersionFromFile(This,pwzFilePath,pwzBuffer,pcchBuffer)	\
    ( (This)->lpVtbl -> GetVersionFromFile(This,pwzFilePath,pwzBuffer,pcchBuffer) ) 

#define ICLRMetaHost_EnumerateInstalledRuntimes(This,ppEnumerator)	\
    ( (This)->lpVtbl -> EnumerateInstalledRuntimes(This,ppEnumerator) ) 

#define ICLRMetaHost_EnumerateLoadedRuntimes(This,hndProcess,ppEnumerator)	\
    ( (This)->lpVtbl -> EnumerateLoadedRuntimes(This,hndProcess,ppEnumerator) ) 

#define ICLRMetaHost_RequestRuntimeLoadedNotification(This,pCallbackFunction)	\
    ( (This)->lpVtbl -> RequestRuntimeLoadedNotification(This,pCallbackFunction) ) 

#define ICLRMetaHost_QueryLegacyV2RuntimeBinding(This,riid,ppUnk)	\
    ( (This)->lpVtbl -> QueryLegacyV2RuntimeBinding(This,riid,ppUnk) ) 

#define ICLRMetaHost_ExitProcess(This,iExitCode)	\
    ( (This)->lpVtbl -> ExitProcess(This,iExitCode) ) 

#endif /* COBJMACROS */


#endif 	/* C style interface */




#endif 	/* __ICLRMetaHost_INTERFACE_DEFINED__ */


/* interface __MIDL_itf_metahost_0000_0002 */
/* [local] */ 

HRESULT WINAPI CLRCreateInstance(REFCLSID clsid, REFIID riid, LPVOID *ppInterface);


extern RPC_IF_HANDLE __MIDL_itf_metahost_0000_0002_v0_0_c_ifspec;
extern RPC_IF_HANDLE __MIDL_itf_metahost_0000_0002_v0_0_s_ifspec;

/* Additional Prototypes for ALL interfaces */

/* end of Additional Prototypes */

#ifdef __cplusplus
}
#endif

#endif


