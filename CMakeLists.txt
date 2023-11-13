
cmake_minimum_required(VERSION 3.18)
project (BOFNET)

set(CMAKE_CXX_STANDARD 14)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_INSTALL_PREFIX ${CMAKE_BINARY_DIR}/dist)
set(CMAKE_INSTALL_OBJECTSDIR "")

file(MAKE_DIRECTORY ${CMAKE_BINARY_DIR}/dist/net20)
file(MAKE_DIRECTORY ${CMAKE_BINARY_DIR}/dist/net40)

install(FILES bofnet.cna DESTINATION ${CMAKE_INSTALL_PREFIX}/net20)
install(FILES bofnet.cna DESTINATION ${CMAKE_INSTALL_PREFIX}/net40)

add_subdirectory(bofs)
add_subdirectory(managed)
