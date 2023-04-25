// Configuration File for the bridge Unit Tests
// Should be updated before running unit tests in any new environment

#ifdef OS_NT
#define P4D "C:\\mount\\p4-bin\\bin.ntx64\\p4d.exe"
#define TAR "tar"
#define MYTESTDIR  "C:\\mount\\MyTestDirBridge"
#define MYTESTDIR8 "C:\\mount\\MyTestDirBridge8"
#endif

#if defined(OS_MACOSX)
#define P4D "/Users/admin/p4d/r22.2/p4d"
#define TAR "/usr/bin/tar"
#define MYTESTDIR "/tmp/mytestdirbridge"
#define MYTESTDIR8 "/tmp/mytestdirbridge8"
#endif

#if defined (OS_LINUX)
#define P4D "/home/mount/p4-bin/bin.linux26x86_64/p4d"
#define TAR "/bin/tar"
#define MYTESTDIR "/tmp/MyTestDirBridge"
#define MYTESTDIR8 "/tmp/MyTestDirBridge"
#endif
