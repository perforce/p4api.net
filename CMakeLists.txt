cmake_minimum_required(VERSION 3.19)

set(CMAKE_VERBOSE_MAKEFILE ON)   # extra noise from cmake
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)
set(CMAKE_COLOR_MAKEFILE ON)

# top level cmake file for P4API.NET
project(P4ApiNet  DESCRIPTION "p4api.net project"  LANGUAGES CSharp CPP)

message( NOTICE "Platform: ${CMAKE_SYSTEM_NAME}" ) # 'Windows' 'Linux' 'Darwin'
message( NOTICE "Version: ${P4ApiNet_VERSION}" )
message( NOTICE "Build Type: ${CMAKE_BUILD_TYPE}" )

if (WIN32)
  set(CMAKE_SHARED_LIBRARY_PREFIX "")
endif()

add_subdirectory(p4bridge)
add_subdirectory(p4bridge-unit-test)
add_subdirectory(p4bridgeStressTest)
add_subdirectory(p4api.net)
add_subdirectory(p4api.net-unit-test)

add_custom_target(bridgetests)
add_dependencies(bridgetests p4bridge-unit-test p4bridgeStressTest)

add_custom_target(tests)
add_dependencies(tests p4api.net-unit-test)
set(P4APINET_SOURCES )

add_executable(P4ApiNet ${P4APINET_SOURCES})

