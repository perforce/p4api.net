#pragma once

#include "UnitTestFrameWork.h"

#ifdef OS_NT
#define STDCALL __stdcall
#else
#define STDCALL
#endif

class TestP4BridgeServerLogging :
    public UnitTestSuite
{
public:
    TestP4BridgeServerLogging(void);
    ~TestP4BridgeServerLogging(void);

    DECLARE_TEST_SUITE(TestP4BridgeServerLogging)

    bool Setup();

    bool TearDown(const char* testName);

    static int STDCALL LogCallback(int level, const char* file, int line, const char* message);

    static bool LogMessageTest();
    static bool BadLogFnPtrTest();
};

