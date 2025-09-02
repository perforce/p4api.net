cmake_minimum_required(VERSION 3.19)

set(CMAKE_VERBOSE_MAKEFILE ON)   # extra noise from cmake
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)
set(CMAKE_COLOR_MAKEFILE ON)

# top level cmake file for P4API.NET
project(P4ApiNet  DESCRIPTION "p4api.net project")
message( NOTICE "Platform: ${CMAKE_SYSTEM_NAME}" ) # 'Windows' 'Linux' 'Darwin'
message( NOTICE "Version: ${P4ApiNet_VERSION}" )
message( NOTICE "Build Type: ${CMAKE_BUILD_TYPE}" )

if (WIN32)
  set(CMAKE_SHARED_LIBRARY_PREFIX "")
endif()

add_subdirectory(p4bridge)
add_subdirectory(p4bridge-unit-test)

set(add_bridge_test ON)
if (EXISTS p4bridgeStressTest/CMakeLists.txt)
  add_subdirectory(p4bridgeStressTest)
else()
  set(add_bridge_test OFF)
endif()
add_subdirectory(p4api.net)

if (EXISTS p4api.net-unit-test/CMakeLists.txt)
  add_subdirectory(p4api.net-unit-test)
  add_custom_target(tests)
  add_dependencies(tests p4api.net-unit-test)
else()
  set(add_bridge_test OFF)
endif()

if (add_bridge_test)
  add_custom_target(bridgetests)
  add_dependencies(bridgetests p4bridge-unit-test p4bridgeStressTest)
endif()

# set(P4APINET_SOURCES )

# add_executable(P4ApiNet ${P4APINET_SOURCES})

