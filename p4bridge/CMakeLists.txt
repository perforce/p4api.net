cmake_minimum_required(VERSION 3.19)

set(CMAKE_CXX_STANDARD 11)

set(CMAKE_VERBOSE_MAKEFILE ON) # extra noise from cmake
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

project(p4bridge VERSION 21.2.0 DESCRIPTION "p4api.net bridge" LANGUAGES CXX)

message( NOTICE "Platform: ${CMAKE_SYSTEM_NAME}" ) # 'Windows'  'Linux'  'Darwin'
message( NOTICE "Version: ${p4bridge_VERSION}")
message( NOTICE "Build Type: ${CMAKE_BUILD_TYPE}")
message( NOTICE "Cmake Src: ${CMAKE_SOURCE_DIR} Cmake Bin: ${CMAKE_BINARY_DIR}")

if (${CMAKE_SYSTEM_NAME} STREQUAL "Windows")
    message( NOTICE "Build Arch: ${MSVC_CXX_ARCHITECTURE_ID}")
    set(WINDOWS "true")
    add_compile_definitions(OS_NT $<$<CONFIG:Debug>:_DEBUG> _WINDOWS _USRDLL P4BRIDGE_EXPORTS _CRT_SECURE_NO_WARNINGS )
    set(SslLibs libcrypto libssl crypt32 Ws2_32 )
    set(ApiLibs libclient libsupp libp4api )
    set(PlatformLibs oldnames Ws2_32 kernel32 user32 gdi32 winspool comdlg32 advapi32 shell32 ole32 oleaut32 uuid odbc32 odbccp32 )
    string(COMPARE EQUAL "${MSVC_CXX_ARCHITECTURE_ID}" "X86" _is_x86)
    link_directories( "../p4api$<${_is_x86}:_x86>_$<IF:$<CONFIG:Debug>,debug,release>/lib" )
    include_directories( "../p4api$<${_is_x86}:_x86>_$<IF:$<CONFIG:Debug>,debug,release>/include/p4" )
endif()

if (${CMAKE_SYSTEM_NAME} STREQUAL "Linux")
    set(LINUX "true")
    add_compile_definitions(OS_LINUX  $<$<CONFIG:Debug>:_DEBUG>)
    set(SslLibs ssl crypto)
    set(ApiLibs client supp p4api )
    set(PlatformLibs pthread dl rt)
    link_directories( "../p4api/lib" )
    include_directories("../p4api/include/p4")
endif()

if (${CMAKE_SYSTEM_NAME} STREQUAL "Darwin")
    set(MACOS "true")
    add_compile_definitions(OS_MACOSX  $<$<CONFIG:Debug>:_DEBUG> _POSIX_THREADS )
    add_compile_options( -fvisibility=hidden )
    # add_compile_definitions(OS_MACOSX _POSIX_THREADS )
    set(SslLibs ssl crypto )
    set(ApiLibs client supp p4api )
    find_library(Foundation_lib Foundation )
    find_library(CoreServices_lib CoreServices )
    find_library(CoreGraphics_lib CoreGraphics )
	find_library(Security_lib Security )
    set(PlatformLibs ${Foundation_lib} ${CoreServices_lib} ${CoreGraphics_lib} ${Security_lib})
    link_directories( "../p4api/lib" )
    include_directories("../p4api/include/p4")
endif()

option(KW "Use Klocwork wrapper" OFF)
if (KW)
    set(KWWRAP_PATH "kwwrap")   # Specify kwwrap full path
    set(KWWRAP_TRACE_PATH "${PROJECT_BINARY_DIR}/../kw.trace")              # Specify path to Klocwork trace file
    set(KWWRAP_TARGET "${KWWRAP_PATH} -o ${KWWRAP_TRACE_PATH}")
    
    set(CMAKE_C_COMPILE_OBJECT "${KWWRAP_TARGET} ${CMAKE_C_COMPILE_OBJECT}")
    set(CMAKE_C_LINK_EXECUTABLE "${KWWRAP_TARGET} ${CMAKE_C_LINK_EXECUTABLE}")
    set(CMAKE_C_CREATE_SHARED_LIBRARY "${KWWRAP_TARGET} ${CMAKE_C_CREATE_SHARED_LIBRARY}")
    set(CMAKE_C_CREATE_SHARED_MODULE "${KWWRAP_TARGET} ${CMAKE_C_CREATE_SHARED_MODULE}")
    set(CMAKE_C_CREATE_STATIC_LIBRARY "${KWWRAP_TARGET} ${CMAKE_C_CREATE_STATIC_LIBRARY}")
    
    set(CMAKE_CXX_COMPILE_OBJECT "${KWWRAP_TARGET} ${CMAKE_CXX_COMPILE_OBJECT}")
    set(CMAKE_CXX_LINK_EXECUTABLE "${KWWRAP_TARGET} ${CMAKE_CXX_LINK_EXECUTABLE}")
    set(CMAKE_CXX_CREATE_SHARED_LIBRARY "${KWWRAP_TARGET} ${CMAKE_CXX_CREATE_SHARED_LIBRARY}")
    set(CMAKE_CXX_CREATE_SHARED_MODULE "${KWWRAP_TARGET} ${CMAKE_CXX_CREATE_SHARED_MODULE}")
    set(CMAKE_CXX_CREATE_STATIC_LIBRARY "${KWWRAP_TARGET} ${CMAKE_CXX_CREATE_STATIC_LIBRARY}")
    message("Klocwork scan enabled")
endif()
unset(KW CACHE)


set(HEADER_FILES 
    Lock.h 
    p4base.h 
    P4BridgeClient.h 
    P4BridgeServer.h 
    P4Connection.h 
    stdafx.h 
    targetver.h 
    ticket.h 
    utils.h )

set(SRC_FILES         
    Lock.cpp
    p4base.cpp
    P4BridgeClient.cpp
    P4BridgeServer.cpp
    P4Connection.cpp
    p4bridge-api.cpp
    p4map-api.cpp
    stdafx.cpp
    utils.cpp )

# The Unit test needs to build the source, not just link to the DLL
get_directory_property(hasParent PARENT_DIRECTORY)
if (hasParent) 
    list(TRANSFORM SRC_FILES PREPEND "${CMAKE_CURRENT_SOURCE_DIR}/" OUTPUT_VARIABLE Test_Sources )
    set(Test_Sources ${Test_Sources} PARENT_SCOPE )
endif()

# declare our DLL target
add_library(p4bridge SHARED ${HEADER_FILES} ${SRC_FILES} p4bridge.rc)

set_source_files_properties(p4bridge.def PROPERTIES HEADER_FILE_ONLY TRUE )

target_link_libraries(p4bridge
        PRIVATE ${ApiLibs}
        PRIVATE ${SslLibs} ${PlatformLibs}  
        )

target_include_directories(p4bridge PUBLIC "${CMAKE_CURRENT_LIST_DIR}")

if (WINDOWS)
    # set -MTD or -MT for MSVC compiler
    set_property(TARGET p4bridge PROPERTY MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")
endif()

# set_target_properties(p4bridge PROPERTIES LIBRARY_OUTPUT_DIRECTORY ${PROJECT_BINARY_DIR}/${PLATFORM_BIN})

