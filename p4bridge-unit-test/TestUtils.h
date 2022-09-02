#pragma once
#include "UnitTestFrameWork.h"
class TestUtils :
    public UnitTestSuite
{
public:
    TestUtils(void);
    ~TestUtils(void);

    DECLARE_TEST_SUITE(TestUtils)

    bool Setup();

    bool TearDown(const char* testName);

    static bool TestAllocString();
};

