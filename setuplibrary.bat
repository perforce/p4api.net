:  Windows Only, look for *.sh for other platforms...
: This script requires that cygwin 'cp' and 'tar' commands are installed and on the path.
: This script does the initial set up of 4 P4API library directories.
:    After running, each directory will contain an SDK for a different build type.
: Build types are: debug x64, release x64, debug x86 and release x86
: You must be logged into Perforce (using p4 login) before using this script.
: This script uses the Perforce internal build environment,  
: The paths used by this script will need to change for your environment. 

mkdir p4api_debug
mkdir p4api_release
mkdir p4api_x86_release
mkdir p4api_x86_debug

: last sync on 18/04/2023
set BRANCH=p22.2
set RELNAME=2022.2
set CHANGEID=2425796
set VSVER=vs2019

: remove the comment below to debug using internal perforce branch build tree
: if set, this script assumes that the c:\dev\internal\depot\%branch%\p4-bin\buildsdkg.bat has been run (to create the SDK)
: set DEBUG_INTERNAL=true
set INTERNAL_P4BIN=c:/dev/internal/depot/%BRANCH%/p4-bin

set RELEASE_SDK_ZIP=p4api_%VSVER%_static_openssl3.zip
set DEBUG_SDK_ZIP=p4api_%VSVER%_static_vsdebug_openssl3.zip

set API_RELEASE=p4api-%RELNAME%.%CHANGEID%
set API_IDENT=%API_RELEASE%.PREP-TEST_ONLY-%VSVER%_static

set SSL_ARTIFACTS=c:/dev/internal/3rd_party/cpp_libraries/openssl/3-latest/artifacts
set BUILD_P4BIN=c:/dev/internal/builds/%BRANCH%/p4-bin

set CWD1=%~dp0
set CWD=%CWD1:~0,-1%
echo %CWD%

: sync them?  Each release of the SDK has a unique API_IDENT, so it needs to match!!!, Log into Perforce first!
: also, either delete, or move the old libraries out of the way.
cd c:/dev/internal/builds
p4 sync %BUILD_P4BIN%/bin.ntx64/%DEBUG_SDK_ZIP%
p4 sync %BUILD_P4BIN%/bin.ntx64/%RELEASE_SDK_ZIP%
p4 sync %BUILD_P4BIN%/bin.ntx86/%DEBUG_SDK_ZIP%
p4 sync %BUILD_P4BIN%/bin.ntx86/%RELEASE_SDK_ZIP%
cd %CWD%

: These lines will copy the current debug build (from branch source build) 
:  They can be replaced by the SDK extraction lines commented out below
mkdir p4api_debug

IF DEFINED DEBUG_INTERNAL (
	cp -r %INTERNAL_P4BIN%/bin.ntx64/g/%API_RELEASE%.main-static_g/include p4api_debug
	cp -r %INTERNAL_P4BIN%/bin.ntx64/g/%API_RELEASE%.main-static_g/lib p4api_debug
	cd p4api_debug
) ELSE (
	tar xvfz %BUILD_P4BIN%/bin.ntx64/%DEBUG_SDK_ZIP% --directory p4api_debug
	cd p4api_debug
	mklink /d include %API_IDENT%_vsdebug\include
	mklink /d lib %API_IDENT%_vsdebug\lib
)

cp %SSL_ARTIFACTS%/lib.ntx64/vs15/d/*.lib lib
cp %SSL_ARTIFACTS%/lib.ntx64/vs15/d/*.pdb lib
cd ..

tar xvfz %BUILD_P4BIN%/bin.ntx64/%RELEASE_SDK_ZIP% --directory p4api_release
cd p4api_release
mklink /d include %API_IDENT%\include
mklink /d lib %API_IDENT%\lib
cp %SSL_ARTIFACTS%/lib.ntx64/vs15/*.lib lib
cp %SSL_ARTIFACTS%/lib.ntx64/vs15/*.pdb lib
cd ..

tar xvfz c:/dev/internal/builds/%BRANCH%/p4-bin/bin.ntx86/%DEBUG_SDK_ZIP% --directory p4api_x86_debug
cd p4api_x86_debug
mklink /d include %API_IDENT%_vsdebug\include
mklink /d lib %API_IDENT%_vsdebug\lib
cp %SSL_ARTIFACTS%/lib.ntx86/vs15/d/*.lib lib
cp %SSL_ARTIFACTS%/lib.ntx86/vs15/d/*.pdb lib
cd ..

tar xvfz c:/dev/internal/builds/%BRANCH%/p4-bin/bin.ntx86/%RELEASE_SDK_ZIP% --directory p4api_x86_release
cd p4api_x86_release
mklink /d include %API_IDENT%\include
mklink /d lib %API_IDENT%\lib 
cp %SSL_ARTIFACTS%/lib.ntx86/vs15/*.lib lib
cp %SSL_ARTIFACTS%/lib.ntx86/vs15/*.pdb lib
cd ..
