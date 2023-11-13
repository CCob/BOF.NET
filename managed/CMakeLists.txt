cmake_minimum_required(VERSION 3.18)
project (BOFNET_Managed NONE)
include(ExternalProject)

if(${CMAKE_HOST_SYSTEM_NAME} STREQUAL "Windows")
	ExternalProject_Add(bofnet_managed
		SOURCE_DIR ${CMAKE_CURRENT_SOURCE_DIR}
                INSTALL_COMMAND msbuild /p:Configuration=Release /t:BOFNET:pack  && ${CMAKE_COMMAND} -E copy ${CMAKE_CURRENT_SOURCE_DIR}/BOFNET/bin/Release/net35/BOFNET.dll ${CMAKE_INSTALL_PREFIX}/net20 && ${CMAKE_COMMAND} -E copy ${CMAKE_CURRENT_SOURCE_DIR}/BOFNET/bin/Release/net480/BOFNET.dll ${CMAKE_INSTALL_PREFIX}/net40 && ${CMAKE_COMMAND} -E copy ${CMAKE_CURRENT_SOURCE_DIR}/BOFNET/bin/Release/BOFNET.1.2.0.nupkg ${CMAKE_INSTALL_PREFIX}/
		BUILD_IN_SOURCE TRUE
		BUILD_ALWAYS TRUE
                BUILD_COMMAND msbuild /p:Configuration=Release /m BOFNET.sln -target:restore COMMAND msbuild /p:Configuration=Release /m BOFNET.sln -target:BOFNET:rebuild
	)
else()
	ExternalProject_Add(bofnet_managed
		SOURCE_DIR ${CMAKE_CURRENT_SOURCE_DIR}
                INSTALL_COMMAND dotnet msbuild -clp:disableconsolecolor BOFNET.sln -property:Configuration=Release -target:BOFNET:pack && ${CMAKE_COMMAND} -E copy ${CMAKE_CURRENT_SOURCE_DIR}/BOFNET/bin/Release/net35/BOFNET.dll ${CMAKE_INSTALL_PREFIX}/net20 && ${CMAKE_COMMAND} -E copy ${CMAKE_CURRENT_SOURCE_DIR}/BOFNET/bin/Release/net480/BOFNET.dll ${CMAKE_INSTALL_PREFIX}/net40
		BUILD_IN_SOURCE TRUE
		BUILD_ALWAYS TRUE
		BUILD_COMMAND dotnet restore COMMAND dotnet msbuild -clp:disableconsolecolor BOFNET.sln -property:Configuration=Release -target:BOFNET 
	)
endif()


