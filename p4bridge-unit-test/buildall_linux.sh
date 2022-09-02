# build all linux varieties of p4bridge-unit-test using cmake
cmake --preset=linux-Debug
cmake --build out/linux-Debug
cmake --preset=linux-Release
cmake --build out/linux-Release

