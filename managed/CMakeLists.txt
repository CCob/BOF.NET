project (BOFNET_Managed)
include(ExternalProject)

ExternalProject_Add(bofnet_managed
    SOURCE_DIR ${CMAKE_CURRENT_SOURCE_DIR}
    INSTALL_COMMAND ${CMAKE_COMMAND} -E copy ${CMAKE_CURRENT_SOURCE_DIR}/BOFNET/bin/Release/net35/BOFNET.dll ${CMAKE_INSTALL_PREFIX}
    BUILD_IN_SOURCE TRUE
    BUILD_COMMAND msbuild /p:Configuration=Release /m BOFNET.sln
)
