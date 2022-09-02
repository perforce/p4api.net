cmake -DCMAKE_BUILD_TYPE=Debug -DCMAKE_CXX_COMPILER=/opt/perforce/clients/perforce/3rd_party/compilers/gcc/8.1.0/artifacts/bin.linux26x86_64/bin/g++ -G "CodeBlocks - Unix Makefiles" /opt/perforce/clients/dotnet/p4api.net/p4bridge
cmake --build cmake-build-debug --target all -- -j 1
