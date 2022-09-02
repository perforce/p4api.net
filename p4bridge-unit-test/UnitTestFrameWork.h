#pragma once

namespace std {
	class exception;
}

typedef bool UnitTest(void);

#ifdef OS_NT
struct _PROCESS_INFORMATION;
typedef struct _PROCESS_INFORMATION * LPPROCESS_INFORMATION;

inline void CleanResults(LPPROCESS_INFORMATION pi) {
    if (pi != nullptr)
    {
        CloseHandle(pi->hProcess);
        CloseHandle(pi->hThread);
        delete (pi);
        pi = nullptr;
    }
}

#else
#define STDCALL
#define CleanResults(pi)
#endif

typedef struct testList
{
    const char * TestName;
    UnitTest * Test;
    testList * pNext;
} TestList;

class UnitTestSuite
{
private:
    UnitTestSuite * pNextTestSuite;

    TestList * pFirstTest;
    TestList * pLastTest;

    static bool breakOnFailure;
    static bool endOnFailure;

    static char rootbuf[4096];

protected:
    void RegisterTest(UnitTest * test, const char* testName);

    static void ReportException(std::exception& e);

#ifdef OS_NT
    LPPROCESS_INFORMATION RunProgram(const char * cmdLine, const char * cwd, bool newConsole, bool waitForExit);
    bool EndProcess(LPPROCESS_INFORMATION pi);
#else
    int RunProgram(const char *cmdLine, const char *cwd, bool newConsole, bool waitForExit);
    bool EndProcess(int pid);
#endif
    static bool rmDir(const char * path);

    static bool getCwd(char *buf, int bufsize);

    static bool mkDir(const char * path);

    static bool chDir(const char *path);

    static bool copyFile(const char *src, const char *dest);

    static bool getRoot(char *buf, int bufsize);

public:
    UnitTestSuite();
    ~UnitTestSuite();

    virtual bool Setup() { return true; }
    virtual bool TearDown(const char* testName) { return true; }

    UnitTestSuite * NextTestSuite() { return pNextTestSuite; }
    void NextTestSuite(UnitTestSuite * pNew) { pNextTestSuite = pNew; }

    void RunTests();

    static bool Assert(bool condition, const char* FailStr, int Line, const char * file);

    static bool BreakOnFailure() { return breakOnFailure; }
    static void BreakOnFailure(bool bNew) { breakOnFailure = bNew; }

    static bool EndOnFailure() { return endOnFailure; }
    static void EndOnFailure(bool bNew) { endOnFailure = bNew; }
};

class UnitTestFrameWork
{
private:
    static int testsPassed;
    static int testsFailed;
	static std::string matchName;
    static std::string srcDir;  // full path to source directory

public:
    UnitTestFrameWork(void);

    static UnitTestSuite * pFirstTestSuite;
    static UnitTestSuite * pLastTestSuite;

    static void SetSourceDirectory(const char *dir);  // where we find a.tar and u.tar
    static bool getSrcDir(char *dir , int bufsize);

    static void RegisterTestSuite(UnitTestSuite * pSuite);

	static void AddTestMatch(const char* regex) {
		matchName = regex;
	}

	// true if the test should be skipped
	static bool isSkipTest(const char* pTest);

    static void RunTests();
    
    static void IncrementTestsPassed() { testsPassed++; }
    static void IncrementTestsFailed() { testsFailed++; }
};

#define DECLARE_TEST_SUITE(t) static t * TestInstance; \
    static t * Create();

#define CREATE_TEST_SUITE(t) \
    t * t::Create() \
    { \
        if (!TestInstance) \
        { \
            TestInstance = new t(); \
            UnitTestFrameWork::RegisterTestSuite(TestInstance); \
        } \
        return TestInstance; \
    } \
    t * t::TestInstance = t::Create();

#define DELETE_TEST_SUITE(t) \
    void t::Delete() \
    { \
        if (TestInstance) \
        { \
            delete TestInstance; \
        } \
    } \

#define ASSERT_FAIL(a) if (!UnitTestSuite::Assert(false, a, __LINE__, __FILE__)) return false;

#define ASSERT_TRUE(a) if (!UnitTestSuite::Assert((a), "ASSERT_TRUE Failed", __LINE__, __FILE__)) return false;
#define ASSERT_FALSE(a) if (!UnitTestSuite::Assert((!a), "ASSERT_FALSE Failed", __LINE__, __FILE__)) return false;

#define ASSERT_INT_TRUE(a) if (!UnitTestSuite::Assert((a != 0), "ASSERT_TRUE Failed", __LINE__, __FILE__)) return false;
#define ASSERT_INT_FALSE(a) if (!UnitTestSuite::Assert((a == 0), "ASSERT_FALSE Failed", __LINE__, __FILE__)) return false;

#define ASSERT_EQUAL(a, b) if (!UnitTestSuite::Assert((a) == (b), "ASSERT_EQUAL Failed", __LINE__, __FILE__)) return false;
#define ASSERT_NOT_EQUAL(a, b) if (!UnitTestSuite::Assert((a != b), "ASSERT_NOT_EQUAL Failed", __LINE__, __FILE__)) return false;

#define ASSERT_NULL(a) if (!UnitTestSuite::Assert((a == NULL), "ASSERT_NULL Failed", __LINE__, __FILE__)) return false;
#define ASSERT_NOT_NULL(a) if (!UnitTestSuite::Assert((a != NULL), "ASSERT_NOT_NULL Failed", __LINE__, __FILE__)) return false;

#define ASSERT_STRING_EQUAL(a, b) if (!UnitTestSuite::Assert((strcmp((a),(b)) == 0), "ASSERT_STRING_EQUAL Failed", __LINE__, __FILE__)) return false;
#define ASSERT_STRING_NOT_EQUAL(a, b) if (!UnitTestSuite::Assert((strcmp((a),(b)) != 0), "ASSERT_STRING_NOT_EQUAL Failed", __LINE__, __FILE__)) return false;
#define ASSERT_STRING_STARTS_WITH(a, b) if (!UnitTestSuite::Assert((strncmp((a),(b), strlen(b)) == 0), "ASSERT_STRING_STARTS_WITH Failed", __LINE__, __FILE__)) return false;

#define ASSERT_W_STRING_EQUAL(a, b) if (!UnitTestSuite::Assert((wcscmp((a),(b)) == 0), "ASSERT_W_STRING_EQUAL Failed", __LINE__, __FILE__)) return false;
#define ASSERT_W_STRING_STARTS_WITH(a, b) if (!UnitTestSuite::Assert((wcsncmp((a),(b), strlen(b)) == 0), "ASSERT_STRING_STARTS_WITH Failed", __LINE__, __FILE__)) return false;

#define DELETE_OBJECT(obj) if( obj != NULL ) { delete obj; obj = NULL; }
#define DELETE_ARRAY(obj) if( obj != NULL ) { delete[] obj; obj = NULL; }
