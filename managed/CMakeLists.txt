cmake_minimum_required (VERSION 2.8.11)
project (BOFNET_Managed)
include(ExternalProject)

if(${CMAKE_HOST_SYSTEM_NAME} STREQUAL "Windows")
	ExternalProject_Add(bofnet_managed
		SOURCE_DIR ${CMAKE_CURRENT_SOURCE_DIR}
		INSTALL_COMMAND ${CMAKE_COMMAND} -E copy ${CMAKE_CURRENT_SOURCE_DIR}/BOFNET/bin/Release/net35/BOFNET.dll ${CMAKE_INSTALL_PREFIX}/net20 && ${CMAKE_COMMAND} -E copy ${CMAKE_CURRENT_SOURCE_DIR}/BOFNET/bin/Release/net40/BOFNET.dll ${CMAKE_INSTALL_PREFIX}/net40
		BUILD_IN_SOURCE TRUE
		BUILD_ALWAYS TRUE
		BUILD_COMMAND msbuild /p:Configuration=Release /m BOFNET.sln
	)
else()
	ExternalProject_Add(bofnet_managed
		SOURCE_DIR ${CMAKE_CURRENT_SOURCE_DIR}
		INSTALL_COMMAND ${CMAKE_COMMAND} -E copy ${CMAKE_CURRENT_SOURCE_DIR}/BOFNET/bin/Release/net35/BOFNET.dll ${CMAKE_INSTALL_PREFIX}/net20 && ${CMAKE_COMMAND} -E copy ${CMAKE_CURRENT_SOURCE_DIR}/BOFNET/bin/Release/net40/BOFNET.dll ${CMAKE_INSTALL_PREFIX}/net40
		BUILD_IN_SOURCE TRUE
		BUILD_ALWAYS TRUE
		BUILD_COMMAND dotnet restore COMMAND dotnet msbuild -clp:disableconsolecolor BOFNET.sln -property:Configuration=Release -target:BOFNET
	)
endif()


