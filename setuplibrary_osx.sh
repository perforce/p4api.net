
# This script sets up the P4API source directory for OSX.
#  All paths are set up for the internal build environment at Perforce,
#  You will need to modify the paths, BRANCH and WORKSPACE used.
#                                                                         

#!/bin/bash

# should remove p4api directory before running this.
BRANCH=r22.2
P4WORKSPACE=/Users/user-name/Perforce/workspace

mkdir p4api

tar xvfz ${P4WORKSPACE}/builds/${BRANCH}/p4-bin/bin.macosx1015x86_64/p4api-openssl3.tgz -C ./p4api --strip-components 1
cp -R ${P4WORKSPACE}/3rd_party/cpp_libraries/openssl/3-latest/artifacts/lib.macosx1015x86_64/* p4api/lib

