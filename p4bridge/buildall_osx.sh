# Build all varieties of p4bridge for osx using cmake
cmake --preset=osx-Debug
cmake --build out/osx-Debug
cmake --preset=osx-Release
cmake --build out/osx-Release
