# set up the P4API source directories on Linux
#  this script is configured for the internal development environment at Perforce,
#   you will need to edit it to fix the paths for use in your environment.

#set -x
mkdir p4api
tar xvfz /opt/perforce/clients/perforce/builds/main/p4-bin/bin.linux26x86_64/p4api-glibc2.3-openssl3.tgz --directory=p4api
ln -s p4api-2022.2.2425796.MAIN-TEST_ONLY/include p4api/include
ln -s p4api-2022.2.2425796.MAIN-TEST_ONLY/lib p4api/lib
cp /opt/perforce/clients/perforce/3rd_party/cpp_libraries/openssl/3-latest/artifacts/lib.linux26x86_64/*.a p4api/lib

# tar xvfz /opt/perforce/clients/perforce/builds/p20.2/p4-bin/bin.linux26x86_64/p4api-glibc2.3-openssl3.tgz --directory=p4api

