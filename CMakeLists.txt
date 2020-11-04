
cmake_minimum_required (VERSION 2.8.11)
project (BOFNET)

set(CMAKE_CXX_STANDARD 14)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_INSTALL_PREFIX ${CMAKE_BINARY_DIR}/dist)
set(CMAKE_INSTALL_OBJECTSDIR "")

file(MAKE_DIRECTORY ${CMAKE_BINARY_DIR}/dist)

install(FILES bofnet.cna DESTINATION ${CMAKE_INSTALL_PREFIX})

add_subdirectory(managed)
add_subdirectory(bofs)
