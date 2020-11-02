#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <winternl.h>
#include <wincrypt.h>

#ifdef _BOF_
extern "C" __declspec(dllimport) int msvcrt$sprintf_s(char* str, size_t len, const char* format, ... );
extern "C" __declspec(dllimport) int msvcrt$sscanf_s(const char* str, const char* format, ... );
#define sprintf_s msvcrt$sprintf_s
#define sscanf_s msvcrt$sscanf_s
#define log(fmt, ...) BeaconPrintf(CALLBACK_OUTPUT, fmt, __VA_ARGS__)
#else
#define log(fmt, ...) printf(fmt, __VA_ARGS__)
#endif

#include <comutil.h>

EXTERN_C NTSTATUS NTAPI NtTerminateProcess(HANDLE, NTSTATUS);
EXTERN_C NTSTATUS NTAPI NtReadVirtualMemory(HANDLE, PVOID, PVOID, ULONG, PULONG);
EXTERN_C NTSTATUS NTAPI NtWriteVirtualMemory(HANDLE, PVOID, PVOID, ULONG, PULONG);
EXTERN_C NTSTATUS NTAPI NtGetContextThread(HANDLE, PCONTEXT);
EXTERN_C NTSTATUS NTAPI NtSetContextThread(HANDLE, PCONTEXT);
EXTERN_C NTSTATUS NTAPI NtUnmapViewOfSection(HANDLE, PVOID);
EXTERN_C NTSTATUS NTAPI NtResumeThread(HANDLE, PULONG);

struct LoadFileParams{
    char pipeName[MAX_PATH];
    unsigned char* data;
    unsigned int expectedSize;
    DWORD WINAPI (*pGetLastError)(void);
    BOOL WINAPI (*pReadFile)(HANDLE hFile, LPVOID lpBuffer, DWORD nNumberOfBytesToRead, LPDWORD lpNumberOfBytesRead, LPOVERLAPPED lpOverlapped);
    BOOL WINAPI (*pCloseHandle)(HANDLE hObject);
    BOOL WINAPI (*pConnectNamedPipe)(HANDLE hNamedPipe, LPOVERLAPPED lpOverlapped);
    HANDLE WINAPI (*pCreateNamedPipeA)(LPCSTR lpName,DWORD dwOpenMode,DWORD dwPipeMode, DWORD nMaxInstances, DWORD nOutBufferSize, DWORD nInBufferSize,
                                DWORD  nDefaultTimeOut, LPSECURITY_ATTRIBUTES lpSecurityAttributes);
    BOOL WINAPI (*pDisconnectNamedPipe)(HANDLE hNamedPipe);
    HANDLE hThread;
};

#ifdef _BOF_    
    #define BOF_REDECLARE_CXX(mod, func) __declspec(dllimport) decltype(func) mod ## $ ## func
    #define BOF_REDECLARE(mod, func) extern "C" __declspec(dllimport) decltype(func) mod ## $ ## func
    #define BOF_LOCAL(mod, func) decltype(func) * func = mod ## $ ## func
#else
    #include <stdio.h>
    #define BOF_REDECLARE(mod, func)
    #define BOF_LOCAL(mod, func)
#endif

BOF_REDECLARE(KERNEL32, GetCurrentProcess);
BOF_REDECLARE(KERNEL32, CreateFileA);
BOF_REDECLARE(KERNEL32, GetVolumeInformationW);
BOF_REDECLARE(KERNEL32, CreateNamedPipeA);
BOF_REDECLARE(KERNEL32, ConnectNamedPipe);
BOF_REDECLARE(KERNEL32, CloseHandle);
BOF_REDECLARE(KERNEL32, ReadFile);
BOF_REDECLARE(KERNEL32, WriteFile);
BOF_REDECLARE(KERNEL32, DisconnectNamedPipe);
BOF_REDECLARE(KERNEL32, GetLastError);
BOF_REDECLARE(KERNEL32, CreateThread);
BOF_REDECLARE(KERNEL32, VirtualProtect);
BOF_REDECLARE(KERNEL32, VirtualAlloc);
BOF_REDECLARE(KERNEL32, SetEnvironmentVariableA);
BOF_REDECLARE(KERNEL32, GetEnvironmentVariableA);
BOF_REDECLARE(KERNEL32, WaitForSingleObject);
BOF_REDECLARE(KERNEL32, GetExitCodeThread);
BOF_REDECLARE(KERNEL32, VirtualAllocEx);
BOF_REDECLARE(KERNEL32, CreatePipe);
BOF_REDECLARE(KERNEL32, CreateProcessA);
BOF_REDECLARE(KERNEL32, VirtualFree);
BOF_REDECLARE(KERNEL32, GetExitCodeProcess);

BOF_REDECLARE(NTDLL, NtTerminateProcess);
BOF_REDECLARE(NTDLL, NtClose);
BOF_REDECLARE(NTDLL, NtWaitForSingleObject);
BOF_REDECLARE(NTDLL, NtResumeThread);
BOF_REDECLARE(NTDLL, NtSetContextThread);
BOF_REDECLARE(NTDLL, NtGetContextThread);
BOF_REDECLARE(NTDLL, NtWriteVirtualMemory);
BOF_REDECLARE(NTDLL, NtReadVirtualMemory);
BOF_REDECLARE(NTDLL, NtUnmapViewOfSection);

BOF_REDECLARE(Crypt32, CryptStringToBinaryA);

BOF_REDECLARE(OLE32, CoInitializeEx);
BOF_REDECLARE(OLE32, CoCreateInstance);
BOF_REDECLARE(OLE32, CLSIDFromString);

BOF_REDECLARE(OleAut32, SafeArrayGetElement);
BOF_REDECLARE(OleAut32, SafeArrayDestroy);
BOF_REDECLARE(OleAut32, SafeArrayPutElement);
BOF_REDECLARE(OleAut32, SafeArrayCreate);
BOF_REDECLARE(OleAut32, SafeArrayCreateVector);
BOF_REDECLARE(OleAut32, SysAllocString);

BOF_REDECLARE(msvcrt, calloc);
BOF_REDECLARE(msvcrt, free);
BOF_REDECLARE(msvcrt, memcpy);
BOF_REDECLARE(msvcrt, memset);
BOF_REDECLARE(msvcrt, strcat);
BOF_REDECLARE(msvcrt, wcscmp);
BOF_REDECLARE(msvcrt, strcmp);
BOF_REDECLARE(msvcrt, strchr);
BOF_REDECLARE(msvcrt, strlen);




