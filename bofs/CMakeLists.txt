file(GLOB BOF_SRC_CXX *.cpp)
file(GLOB BOF_SRC_C *.c)
file(GLOB BOF_HDR *.h)


add_library(bofs_x86 OBJECT ${BOF_SRC_CXX} ${BOF_SRC_C} ${BOF_HDR})
add_library(bofs_x64 OBJECT ${BOF_SRC_CXX} ${BOF_SRC_C} ${BOF_HDR})

target_compile_options(bofs_x86 PRIVATE
     $<$<OR:$<CXX_COMPILER_ID:Clang>,$<CXX_COMPILER_ID:AppleClang>,$<CXX_COMPILER_ID:GNU>>: -fdollars-in-identifiers> -m32)

target_compile_options(bofs_x64 PRIVATE
      $<$<OR:$<CXX_COMPILER_ID:Clang>,$<CXX_COMPILER_ID:AppleClang>,$<CXX_COMPILER_ID:GNU>>: -fdollars-in-identifiers> -m64)

target_compile_definitions(bofs_x86 PRIVATE _BOF_)
target_compile_definitions(bofs_x64 PRIVATE _BOF_)

install(TARGETS bofs_x86
    OBJECTS DESTINATION ${CMAKE_BINARY_DIR})

install(TARGETS bofs_x64
    OBJECTS DESTINATION ${CMAKE_BINARY_DIR})

install(FILES ${CMAKE_BINARY_DIR}/objects-${CMAKE_BUILD_TYPE}/bofs_x86/bofnet_execute.cpp.obj DESTINATION ${CMAKE_INSTALL_PREFIX}/net20 RENAME bofnet_execute.cpp.x86.obj)
install(FILES ${CMAKE_BINARY_DIR}/objects-${CMAKE_BUILD_TYPE}/bofs_x64/bofnet_execute.cpp.obj DESTINATION ${CMAKE_INSTALL_PREFIX}/net20 RENAME bofnet_execute.cpp.x64.obj)

install(FILES ${CMAKE_BINARY_DIR}/objects-${CMAKE_BUILD_TYPE}/bofs_x86/bofnet_execute.cpp.obj DESTINATION ${CMAKE_INSTALL_PREFIX}/net461 RENAME bofnet_execute.cpp.x86.obj)
install(FILES ${CMAKE_BINARY_DIR}/objects-${CMAKE_BUILD_TYPE}/bofs_x64/bofnet_execute.cpp.obj DESTINATION ${CMAKE_INSTALL_PREFIX}/net461 RENAME bofnet_execute.cpp.x64.obj)
