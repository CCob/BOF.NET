project (BOFNET_Managed)
include(ExternalProject)

ExternalProject_Add(bofnet_managed
    SOURCE_DIR ${CMAKE_CURRENT_SOURCE_DIR}
    INSTALL_COMMAND ${CMAKE_COMMAND} -E copy ${CMAKE_CURRENT_SOURCE_DIR}/BOFNET/bin/Release/BOFNET.dll ${CMAKE_BINARY_DIR}
    BUILD_IN_SOURCE TRUE
    BUILD_COMMAND msbuild /p:Configuration=Release /target:Compile /m BOFNET.sln
)
