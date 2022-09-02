rem Build all four varieties of Windows p4bridge using cmake
cmake --preset=x64-Release
cmake --build out/x64/Release --config Release
cmake --preset=x64-Debug
cmake --build out/x64/Debug --config Debug
cmake --preset=x86-Release
cmake --build out/x86/Release --config Release
cmake --preset=x86-Debug 
cmake --build out/x86/Debug --config Debug
