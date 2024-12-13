cmake_minimum_required(VERSION 3.17)

set(CMAKE_CXX_STANDARD 11)

set(CMAKE_VERBOSE_MAKEFILE ON) # extra noise from cmake
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

project(BridgeUnit VERSION 21.1.0 DESCRIPTION "p4api.net bridge-unit-test" LANGUAGES CXX)

message( NOTICE "Platform: ${CMAKE_SYSTEM_NAME}" ) # 'Windows'  'Linux'  'Darwin'
message( NOTICE "Version: ${BridgeUnit_VERSION}")
message( NOTICE "Build Type: ${CMAKE_BUILD_TYPE}")

if (${CMAKE_SYSTEM_NAME} STREQUAL "Windows")
    set_property(GLOBAL PROPERTY USE_FOLDERS ON)
    message( NOTICE "Build Arch: ${MSVC_CXX_ARCHITECTURE_ID}")
    set(WINDOWS "true")
	add_compile_definitions(OS_NT $<$<CONFIG:Debug>:_DEBUG> _WINDOWS _CRT_SECURE_NO_WARNINGS )
	set(SslLibs libssl libcrypto crypt32 Ws2_32 )
    set(ApiLibs libclient libsupp libp4api )
	set(PlatformLibs oldnames kernel32 user32 gdi32 winspool comdlg32 advapi32 shell32 ole32 oleaut32 uuid odbc32 odbccp32 )
    string(COMPARE EQUAL "${MSVC_CXX_ARCHITECTURE_ID}" "X86" _is_x86)
    link_directories( "../p4api$<${_is_x86}:_x86>_$<IF:$<CONFIG:Debug>,debug,release>/lib" )
    include_directories( "../p4api$<${_is_x86}:_x86>_$<IF:$<CONFIG:Debug>,debug,release>/include/p4" )
    set(LIBEXTENSION ".dll")
endif()

if (${CMAKE_SYSTEM_NAME} STREQUAL "Linux")
    set(LINUX "true")
	set(ARCHITECTURE ${CMAKE_SYSTEM_PROCESSOR})
	add_compile_definitions(OS_LINUX ARCHITECTURE="${ARCHITECTURE}" $<$<CONFIG:Debug>:_DEBUG>)
	set(SslLibs ssl crypto)
    set(ApiLibs client supp p4api )
	set(PlatformLibs pthread dl rt)
    link_directories( "../p4api/lib" )
    include_directories("../p4api/include/p4")
    set(LIBEXTENSION ".so")
endif()

if (${CMAKE_SYSTEM_NAME} STREQUAL "Darwin")
    set(MACOS "true")
    add_compile_definitions(OS_MACOSX  $<$<CONFIG:Debug>:_DEBUG> _POSIX_THREADS )
    set(SslLibs ssl crypto )
    set(ApiLibs client supp p4api )
    find_library(Foundation_lib Foundation )
    find_library(CoreServices_lib CoreServices )
    find_library(CoreGraphics_lib CoreGraphics )
    set(PlatformLibs ${Foundation_lib} ${CoreServices_lib} ${CoreGraphics_lib})
    link_directories( "../p4api/lib" )
    include_directories("../p4api/include/p4")
    set(LIBEXTENSION ".dylib")
endif()

# add the bridge dll to the link list
include_directories(../p4bridge )

set(Test_Sources "")
add_subdirectory(../p4bridge p4bridge)
message( NOTICE "Test_Sources: ${Test_Sources}")


add_executable(BridgeUnit
	p4bridge-unit-test.cpp
	TestP4Base.cpp
	TestP4BridgeClient.cpp
	TestP4BridgeServer.cpp
	TestP4BridgeServerLogging.cpp
	TestP4BridgeServerUtf8.cpp
	UnitTestFrameWork.cpp
	TextEncoder.cpp
	TestUtils.cpp
	)

target_sources(BridgeUnit PRIVATE ${Test_Sources})

target_link_libraries(BridgeUnit PUBLIC p4bridge ${ApiLibs} ${SslLibs} ${PlatformLibs} )

if (WINDOWS)
    # set_target_properties(p4bridge PROPERTIES LINKER_LANGUAGE CXX)
    set_property(TARGET BridgeUnit PROPERTY MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>") # set -MTD or -MT for MSVC compiler
    message( NOTICE "Copy DLL src: ${p4bridge_BINARY_DIR}" )
    message( NOTICE "Copy DLL dest: ${BridgeUnit_BINARY_DIR}" )
    message( NOTICE "Lib Extension is ${LIBEXTENSION}")
    set( BridgeUnitLinkFlags ${CMAKE_CURRENT_SOURCE_DIR}/p4bridge-unit-test.res )
    set_target_properties(BridgeUnit PROPERTIES LINK_FLAGS ${BridgeUnitLinkFlags})
endif()

if (LINUX OR MACOS)
    message( NOTICE "Copy DLL src: ${p4bridge_BINARY_DIR}" )
    message( NOTICE "Copy DLL dest: ${BridgeUnit_BINARY_DIR}" )
    message( NOTICE "Lib Extension is ${LIBEXTENSION}")
    # copy the DLL to the runtime directory
    add_custom_command(TARGET BridgeUnit 
	POST_BUILD COMMAND "${CMAKE_COMMAND}" -E copy "${p4bridge_BINARY_DIR}/*${LIBEXTENSION}" "${BridgeUnit_BINARY_DIR}"
    )
endif()
