#include "stdafx.h"
#include "UnitTestFrameWork.h"

#include <exception>
#include <typeinfo>
#include <string>
using namespace std;

#ifdef OS_NT
#include <excpt.h>
#include <Shellapi.h>
#else
#include <unistd.h>
#include <limits.h>
#include <sys/wait.h>
#include <signal.h>
#include <ftw.h>
#include <csignal>
#include <iostream>
#include <sstream>
#include <vector>
#endif

void UnitTestSuite::RegisterTest(UnitTest * test, const char * testName)
{
	TestList * newTest = new TestList();
	newTest->TestName = testName;
	newTest->Test = test;
	newTest->pNext = 0;

	if (pLastTest)
	{
		pLastTest->pNext = newTest;
		pLastTest = newTest;
	}
	else
	{
		pFirstTest = newTest;
		pLastTest = newTest;
	}
}

UnitTestSuite::UnitTestSuite()
{
	pFirstTest = 0;
	pLastTest = 0;

	pNextTestSuite = 0;

	// save our current directory for further use, before tests start changing it.
	getCwd(rootbuf,4096);
}

UnitTestSuite::~UnitTestSuite()
{
	TestList * pCurTest = pFirstTest;
	while(pCurTest)
	{
		pFirstTest = pFirstTest->pNext;
		delete pCurTest;
		pCurTest = pFirstTest;
	}

	pFirstTest = 0;
	pLastTest = 0;

	pNextTestSuite = 0;
}

/*******************************************************************************
*
*  UnitTestSuite::ReportException
*
*  Report any C++ Exceptions.
*
******************************************************************************/
void UnitTestSuite::ReportException(std::exception& e)
{
	// Report the exception	
	printf("UnitTest Exception Detected %s : %s\n", e.what(), typeid(e).name());
	}

#ifdef OS_NT
LPPROCESS_INFORMATION UnitTestSuite::RunProgram(const char * cmdLine, const char * cwd, bool newConsole, bool waitForExit)
{
	auto *si = new STARTUPINFOA;
	auto *pi = new PROCESS_INFORMATION;

	ZeroMemory( si, sizeof(STARTUPINFOA) );
	si->cb = sizeof(si);
	ZeroMemory( pi, sizeof(PROCESS_INFORMATION) );

	// Start the child process. 
	if( !CreateProcessA( 
		NULL,           // No module name (use command line)
		(LPSTR) cmdLine,        // Command line
		NULL,           // Process handle not inheritable
		NULL,           // Thread handle not inheritable
		FALSE,          // Set handle inheritance to FALSE
		newConsole?CREATE_NEW_CONSOLE:0,              // No creation flags
		NULL,           // Use parent's environment block
		cwd,            // Starting directory 
		si,             // Pointer to STARTUPINFO structure
		pi )            // Pointer to PROCESS_INFORMATION structure
	) 
	{
		printf( "CreateProcess failed (%d).\n", GetLastError() );
		return NULL;
	}
	if (waitForExit)
	{
		// Wait until child process exits.
		WaitForSingleObject( pi->hProcess, INFINITE );

		// Cleanup is done by CleanResults(pi);
	}
	delete si;

	return pi;
}

bool UnitTestSuite::EndProcess(LPPROCESS_INFORMATION pi)
{
	TerminateProcess( pi->hProcess, 0);
	
	// Wait until child process exits.
	WaitForSingleObject( pi->hProcess, INFINITE );

	// Close process and thread handles. 
	CloseHandle( pi->hProcess );
	CloseHandle( pi->hThread );

	delete pi;
	pi = nullptr;

	return true;
}

#else

int UnitTestSuite::RunProgram(const char *cmdLine, const char *cwd, bool newConsole, bool waitForExit)
{
    // in linux we ignore newConsole since it requires a gui
    if (chdir(cwd))
    {
        printf("Error, failed to change directory to %s\n",cwd);
        return 0;
    }

    pid_t parent = getpid();
    pid_t pid = fork();

    if (pid < 0)
    {
        printf("Error, failed to fork\n %s",strerror(errno));
        return 0;
    }
    else if (pid > 0)
    {
        //  we are the caller, wait for the child?
        if (waitForExit)
        {           
            pid_t w;
            int status;
            while((w = waitpid(pid,&status, 0)) == -1){
                if (errno == EINTR) // ignore interrupts
                    continue;
                else {
                    perror("waitpid");
                    return 0;
                }
            }
        }
    }
    else
    {
        // we are the child
        string line = cmdLine;

        istringstream iss(line);
        vector<string> args;
        for(string s; iss >> s;)
            args.push_back(s);
        int count = args.size();

        const char * argv[count + 1];
        for(int i = 0; i < count; i++){
            argv[i] = args[i].c_str();
        }
        argv[count] = nullptr;
        if (execv(argv[0], (char * const *) argv) < 0)
            printf("\nexecv %s error %s\n",argv[0], strerror(errno));
    }
    return pid;
}

bool UnitTestSuite::EndProcess(pid_t pi)
{
    int rv = kill(pi,SIGKILL);  // do we need to wait for this like in OS_NT?
    return(! rv);
}
#endif

#ifdef OS_NT
bool directoryExists(const char* dirname)
{
	const DWORD attribs = ::GetFileAttributesA(dirname);
	if (attribs == INVALID_FILE_ATTRIBUTES)
	{
		return false;
	}
	return (attribs & FILE_ATTRIBUTE_DIRECTORY);
}
#endif

#ifndef OS_NT
// a callback used by rmDir
int unlink_cb(const char *fpath, const struct stat *sb, int typeflag, struct FTW *ftwbuf)
{
    return remove(fpath);
}
#endif

/*
 * A cross platform rmDir
 */
bool UnitTestSuite::rmDir(const char * path)
{
#ifdef OS_NT
	char szDir[MAX_PATH+1];  // +1 for the double null terminate

	if (!directoryExists(path))
		return true;
	
	SHFILEOPSTRUCTA fos = {0};

	strcpy(szDir, path);
	int len = static_cast<int>(strlen(szDir));
	szDir[len+1] = 0; // double null terminate for SHFileOperation

	// delete the folder and everything inside
	fos.wFunc = FO_DELETE;
	fos.pFrom = szDir;
	fos.fFlags = FOF_NO_UI;
	int rv = SHFileOperation(&fos);
	if (rv != 0) 
		printf("delete of %s failed %02x\n", path, rv);
	return(rv != 0);
#else
	struct stat info;
	if (stat(path,&info) != -1) {
        if (S_ISDIR(info.st_mode)) {  // is directory
            nftw(path, unlink_cb, 64, FTW_DEPTH | FTW_PHYS);
        } else {
            remove(path); // is file
        }
    }

	// Validate it is really gone!
	sleep(2);
	if (stat(path, &info) != -1)
    {
	    printf("rmDir: Unable to remove %s! \n",path);
	    return false;
    }
#endif
	return true;
}



/*
 * A cross platform getCwd()
 */
bool UnitTestSuite::getCwd(char *buf, int bufsize)
{
#ifdef OS_NT
    GetCurrentDirectory(bufsize, buf);
#else
    if (getcwd(buf,bufsize) == nullptr)
        return false;
#endif
    return true;
}

/*
 * A cross platform mkDir()
 *     returns 'true' if successful
 */
bool UnitTestSuite::mkDir(const char *path)
{
#ifdef OS_NT
    if (!CreateDirectory( path, NULL ))
    {
		DWORD err = GetLastError();
		if (err == 183) // already exists
		{
			return true;
		} 
		printf("mkDir: GetLastError returns %ld\n",err);
		return false;
    }
#else
    int status = mkdir(path, S_IRWXU | S_IRWXG | S_IROTH | S_IXOTH);
    if (status < 0)
    {
        printf("mkDir: %s\n", strerror(errno));
        return false;
    }
#endif
    return true;
}

/*
 * A cross platform chDir()
 *     returns 'true' if successful
 */
bool UnitTestSuite::chDir(const char *path)
{
#ifdef OS_NT
    if (! SetCurrentDirectory(path)){
       DWORD err = GetLastError();
       printf("chDir: GetLastError returns %ld\n",err);
       return false;
    }
#else
    if (chdir(path) < 0){
        printf("chDir: %s\n",strerror(errno));
        return false;
    }
#endif
    return true;
}


/*
 * A simple file copy in ANSI C
 */
bool UnitTestSuite::copyFile(const char *source, const char *destination)
{
    char buf[8000];
    FILE *src = fopen(source, "rb");
    FILE *dest = fopen(destination, "wb");
    if (src == nullptr || dest == nullptr){
	char pbuf[100];
        printf("Error Copying file from %s to %s\n",source, destination);
	getCwd(pbuf, 100);
	printf("pwd: %s\n",pbuf);
        return false;
    }

    size_t size = fread(buf, 1, sizeof(buf), src);
    while(size){
        fwrite(buf,1,size,dest);
        size = fread(buf, 1, sizeof(buf), src);
    }
    fclose(src);
    fclose(dest);
    return true;
}

bool UnitTestFrameWork::isSkipTest(const char* pTestName) {
	if (matchName.empty())
		return false;
	// TODO: more complex regex or something
	return (matchName != pTestName);
}

void UnitTestSuite::RunTests()
{
	TestList * pCurrentTest = pFirstTest;

	while (pCurrentTest)
	{
		// if we are we skipping tests, do it now
		if (UnitTestFrameWork::isSkipTest(pCurrentTest->TestName)) {
			pCurrentTest = pCurrentTest->pNext;
			continue;
		}

		unsigned int code = 0;

		int itemCounts[p4typesCount];

		try 
		{
			printf("Test %s:\n", pCurrentTest->TestName);

#ifdef _DEBUG
			// record the current ItemCounts to make sure that we're freeing everything
			for (int i = 0; i < p4typesCount; i++)
				itemCounts[i] = p4base::GetItemCount(i);
			
			int iAllocs = Utils::AllocCount(), iFrees = Utils::FreeCount();
#endif
			if (!Setup())
			{
				printf("\tSetup  Failed!!\n");
				UnitTestFrameWork::IncrementTestsFailed();

				if (endOnFailure)
					return;
			}
			else
			{
				bool passed = (*pCurrentTest->Test)();
				bool tearDownPassed = TearDown(pCurrentTest->TestName);
#ifdef _DEBUG
				// check that the ItemCounts match
				int itemMismatch = 0;
				for (int i = 0; i < p4typesCount; i++)
				{
					if (p4base::GetItemCount(i) != itemCounts[i])
					{
						printf("\t<<<<*** Item count for %s mismatch: %d/%d\n", p4base::GetTypeStr(i), itemCounts[i], p4base::GetItemCount(i));
						itemMismatch += p4base::GetItemCount(i) - itemCounts[i];
					}

				}

				// check the string alloc counts
				int stringAllocs = (Utils::AllocCount() - iAllocs), stringFrees = (Utils::FreeCount() - iFrees);
				if (stringAllocs != stringFrees)
					printf("\t<<<<*** String alloc count mismatch: %d/%d\n", stringAllocs, stringFrees);

				passed = passed && (itemMismatch == 0) && (stringAllocs == stringFrees);
#endif
				if (passed)
					printf("\tPassed\n");
				else 
					printf("\t<<<<***Failed!!***>>>>\n");

				if (!tearDownPassed)
					printf("\tTearDown Failed!!\n");

				if (!passed || !tearDownPassed)
				{
					UnitTestFrameWork::IncrementTestsFailed();
					if (endOnFailure)
						return;
				}
				UnitTestFrameWork::IncrementTestsPassed();
			}
		} 
		catch (std::exception& e)
		{
			ReportException(e);
			UnitTestFrameWork::IncrementTestsFailed();
			if (endOnFailure)
				return;
		}
		pCurrentTest = pCurrentTest->pNext;
	}

	if (pNextTestSuite)
		pNextTestSuite->RunTests();
}

bool UnitTestSuite::Assert(bool condition, const char* FailStr, int Line, const char * FileName)
{
	if (condition) // It's as expected
		return true;

	if (breakOnFailure)
        {
#ifdef OS_NT
		__debugbreak();
#else
	    raise(SIGTRAP);
#endif
        }

	printf("\t%s: Line: %d, %s:\n", FailStr, Line, FileName);
	return false;
}

bool UnitTestSuite::breakOnFailure = false;

bool UnitTestSuite::endOnFailure = false;

char UnitTestSuite::rootbuf[4096];

UnitTestSuite * UnitTestFrameWork::pFirstTestSuite = 0;
UnitTestSuite * UnitTestFrameWork::pLastTestSuite = 0;

/*
 * UnitTestFramework, provides high level interface to tests
 */

UnitTestFrameWork::UnitTestFrameWork(void)
{
	SetSourceDirectory(".."); // usually overridden by -s argument
}

// passed from -s argument on command line
void UnitTestFrameWork::SetSourceDirectory(const char *dir){
#ifndef OS_NT
	char resolved_path[PATH_MAX];
	srcDir = realpath(dir, resolved_path);
#else
	srcDir = dir;
#endif
}

bool UnitTestFrameWork::getSrcDir(char *dir , int bufsize)
{

    strncpy(dir, UnitTestFrameWork::srcDir.c_str(), bufsize);
    return true;
}

void UnitTestFrameWork::RegisterTestSuite(UnitTestSuite * pSuite)
{
	if (pLastTestSuite)
	{
		pLastTestSuite->NextTestSuite(pSuite);
		pLastTestSuite = pSuite;
	}
	else
	{
		pFirstTestSuite = pSuite;
		pLastTestSuite = pSuite;
	}
}

int UnitTestFrameWork::testsPassed = 0;
int UnitTestFrameWork::testsFailed = 0;
std::string UnitTestFrameWork::matchName;
std::string UnitTestFrameWork::srcDir;

void UnitTestFrameWork::RunTests()
{
	testsPassed = 0;
	testsFailed = 0;

	if (pFirstTestSuite)
		pFirstTestSuite->RunTests();

	printf("Tests Passed %d, TestFailed: %d\n", testsPassed, testsFailed);

	// delete all the test suites
	while (pFirstTestSuite)
	{
		UnitTestSuite * pCurrentTestSuite = pFirstTestSuite;
		pFirstTestSuite = pCurrentTestSuite->NextTestSuite();
		delete pCurrentTestSuite;
	}
	p4base::Cleanup();
}
