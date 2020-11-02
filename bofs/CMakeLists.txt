file(GLOB BOF_SRC_CXX *.cpp)
file(GLOB BOF_SRC_C *.c)
file(GLOB BOF_HDR *.h)


add_library(bofs OBJECT ${BOF_SRC_CXX} ${BOF_SRC_C} ${BOF_HDR})

target_compile_options(bofs PRIVATE
     $<$<OR:$<CXX_COMPILER_ID:Clang>,$<CXX_COMPILER_ID:AppleClang>,$<CXX_COMPILER_ID:GNU>>: -fdollars-in-identifiers>)

target_compile_definitions(bofs PRIVATE _BOF_)

install(TARGETS bofs
    OBJECTS DESTINATION ${CMAKE_BINARY_DIR})
