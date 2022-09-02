# build all osx varieties of p4bridge-unit-test using cmake
cmake --preset=osx-Debug
cmake --build out/osx-Debug
cmake --preset=osx-Release
cmake --build out/osx-Release

