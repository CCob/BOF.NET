
#include <windows.h>
#include <stdio.h>
#include <vector>
#include <cinttypes>
#include <sstream>
#include <iostream>

#define CALLBACK_OUTPUT      0x0
#define CALLBACK_OUTPUT_OEM  0x1e
#define CALLBACK_ERROR       0x0d
#define CALLBACK_OUTPUT_UTF8 0x20

#define REVERSE_SHORT(n) ((unsigned short) (((n & 0xFF) << 8) | \
                                            ((n & 0xFF00) >> 8)))
#define REVERSE_LONG(n) ((unsigned long) (((n & 0xFF) << 24) | \
                                          ((n & 0xFF00) << 8) | \
                                          ((n & 0xFF0000) >> 8) | \
                                          ((n & 0xFF000000) >> 24)))

typedef std::vector<char> ByteArray;

extern "C" {

    void go(char* args , int len);

    void BeaconPrintf(int type, const char* fmt, ...){

        switch(type){
            case CALLBACK_OUTPUT:
            case CALLBACK_OUTPUT_OEM:
            case CALLBACK_ERROR:
            case CALLBACK_OUTPUT_UTF8:{
                    va_list args;
                    int len;
                    char * buffer;

                    va_start( args, fmt );
                    len = _vscprintf( fmt, args ) // _vscprintf doesn't count
                                                + 1; // terminating '\0'

                    buffer = (char *) malloc( len * sizeof(char));
                    if ( NULL != buffer )
                    {
                       vsprintf_s( buffer, len, fmt, args );
                       puts(buffer);
                       free( buffer );
                    }
                    va_end( args );
            }
        }
    }

    void BeaconOutput(int type, const char* data, int len){
        BeaconPrintf(type, data);
    }

    int toWideChar(char* src, wchar_t* dst, int max){
        auto result = MultiByteToWideChar(CP_UTF8, 0, src, strlen(src), dst, max);
        return result;
    }

    BOOL BeaconUseToken(HANDLE token){
        return ImpersonateLoggedOnUser(token);
    }

    void  BeaconRevertToken(){
        RevertToSelf();
    }

    void BeaconInjectProcess(HANDLE hProc, int pid, char* payload, int p_len, int p_offset, char* arg, int a_len){
    }

    void BeaconGetSpawnTo(BOOL x86, char* buffer, int length){
    }
}


struct PrefixedBuffer{
    int len;
    char buffer[0];
};

ByteArray readAllData(const char* fileName){

    FILE* fp = fopen(fileName, "rb");
    fseek(fp, 0L, SEEK_END);
    int sz = ftell(fp);
    fseek(fp, 0L, SEEK_SET);
    ByteArray data;
    data.resize(sz);
    fread(&data[0], 1, sz, fp);
    fclose(fp);
    return data;
}

ByteArray loadAssembly(const char* filename, const std::wstring& loaderId){

    std::stringstream ss;

    ByteArray assemblyData = readAllData(filename);
    const char commandName[] = "BOFNET.Bofs.AssemblyLoader";

    int nameLen = REVERSE_LONG(loaderId.length());
    int chunkSize = REVERSE_LONG(assemblyData.size() + 1);
    int dataSize = REVERSE_LONG(assemblyData.size());

    ss.write(commandName, sizeof(commandName));
    ss.write((char*)&nameLen, 4);
    ss.write((char*)loaderId.data(), (loaderId.length() + 1) * 2);
    ss.write((char*)&chunkSize, 4);
    ss.write((char*)&dataSize, 4);
    ss.write(&assemblyData[0], assemblyData.size());

    std::string result = ss.str();
    return ByteArray(result.begin(), result.end());

}

ByteArray executeBoo(const char* booScript, const wchar_t* scriptArgs){

    std::stringstream ss;

    const char commandName[] = "BOFNET.Bofs.Boo.BooRunner";

    int scriptLen = REVERSE_LONG(strlen(booScript));
    int argsLen = REVERSE_LONG(wcslen(scriptArgs) + 1);

    ss.write(commandName, sizeof(commandName));
    ss.write((char*)&scriptLen, 4);
    ss.write((char*)booScript, strlen(booScript));
    ss.write((char*)&argsLen, 4);
    ss.write((char*)scriptArgs, (wcslen(scriptArgs) + 1) * 2);

    std::string result = ss.str();
    return ByteArray(result.begin(), result.end());

}

ByteArray execute(const std::string& bofnetCmd){
    return ByteArray(bofnetCmd.begin(), bofnetCmd.end() + 1);
}

void goBA(const ByteArray& data){
    go((char*)data.data(), data.size());
}


int main(int argc, char** argv){

    const char init[] = "BOFNET.Bofs.Initializer\x00";
    ByteArray runtime = readAllData("bofnet.dll");
    ByteArray args;

    args.resize(sizeof(init) + runtime.size());
    memcpy(&args[0], init, sizeof(init));
    memcpy(&args[sizeof(init)-1], &runtime[0], runtime.size());

    go(args.data(), args.size());

    goBA(executeBoo("print 'Yo!'", L"args"));

    goBA(execute("HelloWorld Yolo"));

    getchar();
}
