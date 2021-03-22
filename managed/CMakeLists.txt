project (BOFNET_Managed)
include(ExternalProject)


if(WIN32)
	ExternalProject_Add(bofnet_managed
		SOURCE_DIR ${CMAKE_CURRENT_SOURCE_DIR}
		INSTALL_COMMAND ${CMAKE_COMMAND} -E copy ${CMAKE_CURRENT_SOURCE_DIR}/BOFNET/bin/Release/net35/BOFNET.dll ${CMAKE_INSTALL_PREFIX}/net35 && ${CMAKE_COMMAND} -E copy ${CMAKE_CURRENT_SOURCE_DIR}/BOFNET/bin/Release/net461/BOFNET.dll ${CMAKE_INSTALL_PREFIX}/net461
		BUILD_IN_SOURCE TRUE
		BUILD_ALWAYS TRUE
		BUILD_COMMAND msbuild /p:Configuration=Release /m BOFNET.sln
	)
endif()

if(UNIX)
	ExternalProject_Add(bofnet_managed
		SOURCE_DIR ${CMAKE_CURRENT_SOURCE_DIR}
		INSTALL_COMMAND ${CMAKE_COMMAND} -E copy ${CMAKE_CURRENT_SOURCE_DIR}/BOFNET/bin/Release/netstandard2.0/BOFNET.dll ${CMAKE_INSTALL_PREFIX}/netstandard2.0 && ${CMAKE_COMMAND} -E copy ${CMAKE_CURRENT_SOURCE_DIR}/BOFNET/bin/Release/net461/BOFNET.dll ${CMAKE_INSTALL_PREFIX}/net461
		BUILD_IN_SOURCE TRUE
		BUILD_ALWAYS TRUE
		BUILD_COMMAND dotnet msbuild /p:Configuration=Release /m BOFNET.sln
	)
endif()
