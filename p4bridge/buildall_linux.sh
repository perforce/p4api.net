# Build all varieties of p4bridge using cmake
cmake --preset=linux-Debug
cmake --build out/linux-Debug
cmake --preset=linux-Release
cmake --build out/linux-Release
