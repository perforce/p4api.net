#include "stdafx.h"
#include "UnitTestFrameWork.h"

#include "../p4bridge/P4BridgeClient.h"
#include "../p4bridge/P4BridgeServer.h"

#include "TestP4BridgeServerUtf8.h"

#include "UnitTestConfig.h"

#ifdef OS_NT
#include <conio.h>
#else
#define MAX_PATH 260
#include <sys/types.h>
#include <sys/stat.h>
#include <unistd.h>
#include <iostream>
#endif

CREATE_TEST_SUITE(TestP4BridgeServerUtf8)

TestP4BridgeServerUtf8::TestP4BridgeServerUtf8()
{
    UnitTestSuite::RegisterTest(ServerConnectionTest, "ServerConnectionTest");
    UnitTestSuite::RegisterTest(TestNonUnicodeClientToUnicodeServer, "TestNonUnicodeClientToUnicodeServer");
    UnitTestSuite::RegisterTest(TestUntaggedCommand, "TestUntaggedCommand");
    UnitTestSuite::RegisterTest(TestUnicodeUserName, "TestUnicodeUserName");
    UnitTestSuite::RegisterTest(TestTaggedCommand, "TestTaggedCommand");
    UnitTestSuite::RegisterTest(TestTextOutCommand, "TestTextOutCommand");
    UnitTestSuite::RegisterTest(TestBinaryOutCommand, "TestBinaryOutCommand");
    UnitTestSuite::RegisterTest(TestErrorOutCommand, "TestErrorOutCommand");
}


TestP4BridgeServerUtf8::~TestP4BridgeServerUtf8()
{
}

char unitTestSrcDir8[MAX_PATH];
char unitTestZip8[MAX_PATH];

#ifdef OS_NT
const char * testClient8 = "admin_space8";

const char * TestDir8 = MYTESTDIR8;
const char* newTicketFile8 = MYTESTDIR8 "\\.p4tickets.txt";
const char* newEnviroFile8 = MYTESTDIR8 "\\.p4enviro.txt";

const char * TestZip8 = MYTESTDIR8 "\\u.tar";
const char * utar_cmd8 = "TAR xvf u.tar";
const char * rcp_cmd8 =  P4D " -r " MYTESTDIR8 " -jr checkpoint.1";
const char * udb_cmd8 =  P4D " -r " MYTESTDIR8 " -xu";
const char * p4d_cmd8 =  P4D " -v security=0 -p6666 -IdUnitTestServer -r " MYTESTDIR8 " -Llog";
const char * tarBall8 = "\\u.tar";
#endif

#if defined(OS_MACOSX)
const char *testClient8 = "osx_space8";
#endif

#if defined (OS_LINUX)
const char *testClient8 = "linux_space8";

#endif
#if defined(OS_MACOSX) || defined(OS_LINUX)
const char * TestDir8 = MYTESTDIR8;
const char* newTicketFile8 = MYTESTDIR8 "/.p4tickets.txt";
const char* newEnviroFile8 = MYTESTDIR8 "/.p4enviro.txt";

const char * TestZip8 = MYTESTDIR8 "/u.tar";
const char * utar_cmd8 = TAR " xf u.tar";
const char * rcp_cmd8 = P4D " -C1 -r " MYTESTDIR8 " -jr checkpoint.1";
const char * udb_cmd8 = P4D " -C1 -r " MYTESTDIR8 " -xu";
const char * p4d_cmd8 = P4D " -C1 -v security=0 -p6666 -IdUnitTestServer -r" MYTESTDIR8 " -Jjournal -Llog";
const char * tarBall8 = "/u.tar";
#endif

#ifdef OS_NT
LPPROCESS_INFORMATION pi8 = NULL;
#else
int pi8 = 0;
#endif

P4BridgeServer * ps8 = NULL;

// check for connection failure to p4d
bool TestP4BridgeServerUtf8::CheckConnection(P4BridgeServer *ps, P4ClientError *connectionError )
{
    if( !ps->connected( &connectionError ) )
    {
        char buff[256];
        snprintf(buff, sizeof(buff) - 1, "Connection error: %s", connectionError->Message.c_str());
        // Abort if the connect did not succeed
        ASSERT_FAIL(buff);
    }
    return true;
}

bool TestP4BridgeServerUtf8::Setup()
{
    // remove the test directory if it exists
    if (! UnitTestSuite::rmDir( TestDir8 ) )
        return false;

    UnitTestFrameWork::getSrcDir(unitTestSrcDir8, sizeof(unitTestSrcDir8));
    
    strcpy(unitTestZip8, unitTestSrcDir8);
    strcat( unitTestZip8, tarBall8);

    UnitTestSuite::mkDir( TestDir8 );

    if (! UnitTestSuite::copyFile(unitTestZip8, TestZip8))
        return false;

    UnitTestSuite::chDir(TestDir8);

    pi8= UnitTestSuite::RunProgram(utar_cmd8, TestDir8, true, true);
    if (!pi8)
    {
        UnitTestSuite::chDir(unitTestSrcDir8);
        return false;
    }
    CleanResults(pi8);

    pi8 = UnitTestSuite::RunProgram(rcp_cmd8, TestDir8, true, true);
    if (!pi8) 
	{
        UnitTestSuite::chDir(unitTestSrcDir8);
        return false;
	}
    CleanResults(pi8);

    pi8 = UnitTestSuite::RunProgram(udb_cmd8, TestDir8, true, true);
    if (!pi8)
    {
        UnitTestSuite::chDir(unitTestSrcDir8);
        return false;
    }
    CleanResults(pi8);

    //server deployed by u.tar is already in Unicode mode
    //pi8 = UnitTestSuite::RunProgram(p4d_xi_cmd8, TestDir8, false, true);
    //if (!pi8) return false;

    //CleanResults(pi8);

    pi8 = UnitTestSuite::RunProgram(p4d_cmd8, TestDir8, false, false);
    if (!pi8)
    {
        UnitTestSuite::chDir(unitTestSrcDir8);
        return false;
    }

    // change default .p4enviro and .p4tickets file locations
    // so we don't trash the build system environment during our tests.

#ifdef OS_NT
    SetEnvironmentVariable("P4TICKETS", newTicketFile8);
    SetEnvironmentVariable("P4ENVIRO", newEnviroFile8);
#else
    setenv("P4TICKETS", newTicketFile8, 1);
    setenv("P4ENVIRO", newEnviroFile8, 1);
#endif


#ifndef OS_NT
    sleep(2);  // give the server time to get ready...
#endif
    return true;
}

bool TestP4BridgeServerUtf8::TearDown(const char* testName)
{
    if (pi8)
        UnitTestSuite::EndProcess( pi8 );

    if (ps8)
    {
        delete ps8;
        ps8 = NULL;

#ifdef _DEBUG_MEMORY
        p4base::DumpMemoryState("After deleting server");
#endif
    }
    UnitTestSuite::chDir( unitTestSrcDir8 );

    UnitTestSuite::rmDir(TestDir8);

#ifdef _DEBUG_MEMORY
	p4base::PrintMemoryState(testName);
#endif
    return true;
}

bool TestP4BridgeServerUtf8::ServerConnectionTest()
{
    P4ClientError* connectionError = NULL;
    // create a new server
    ps8 = new P4BridgeServer("localhost:6666", "admin", "", "");

    bool rv = [&] {
        ASSERT_NOT_NULL(ps8);

        // connect and see if the api returned an error.
        CheckConnection(ps8, connectionError);

        ASSERT_INT_TRUE(ps8->unicodeServer());
        ps8->set_charset("utf8", "utf16le");

        return true;
    }();
    return rv;
}

bool TestP4BridgeServerUtf8::TestNonUnicodeClientToUnicodeServer()
{
    P4ClientError* connectionError = NULL;
    // create a new server
    ps8 = new P4BridgeServer("localhost:6666", "admin", "", testClient8 );
 
    bool rv = [&] {
        ASSERT_NOT_NULL(ps8);

        // connect and see if the api returned an error.
        CheckConnection(ps8, connectionError);

        ASSERT_INT_TRUE(ps8->unicodeServer());

        const char *const params[] = {"//depot/mycode/*"};
        ASSERT_FALSE(ps8->run_command("files", 5, 0, params, 1))

        P4ClientError *out = ps8->get_ui(5)->GetErrorResults();

        ASSERT_STRING_STARTS_WITH(out->Message.c_str(), "Unicode server permits only unicode enabled clients.")

        return true;
    }();
    return rv;
}

bool TestP4BridgeServerUtf8::TestUntaggedCommand()
{
    P4ClientError* connectionError = NULL;
    // create a new server
    ps8 = new P4BridgeServer("localhost:6666", "admin", "", testClient8 );

    bool rv = [&] {
        ASSERT_NOT_NULL(ps8);

        // connect and see if the api returned an error.
        CheckConnection(ps8, connectionError);

        ASSERT_INT_TRUE(ps8->unicodeServer());
        ps8->set_charset("utf8", "utf16le");

        const char *const params[] = {"//depot/mycode/*"};

        ASSERT_INT_TRUE(ps8->run_command("files", 7, 0, params, 1))

        P4ClientInfoMsg *out = ps8->get_ui(7)->GetInfoResults();

        ASSERT_STRING_EQUAL(out->Message.c_str(), "//depot/MyCode/ReadMe.txt#1 - add change 1 (text)")
        ASSERT_NOT_NULL(out->Next)
        ASSERT_STRING_EQUAL(out->Next->Message.c_str(), "//depot/MyCode/Silly.bmp#1 - add change 1 (binary)")
        ASSERT_NOT_NULL(out->Next->Next)
        ASSERT_STRING_EQUAL(out->Next->Next->Message.c_str(),
                            "//depot/MyCode/\xD0\x9F\xD1\x8E\xD0\xBF.txt#1 - add change 3 (utf16)")

        return true;
    }();
    return rv;
}


// Callback to provide password for "login" in TestUnicodeUserName()
void STDCALL ProvidePassword(int cmdId, const char *message, char *response, int responseSize, int noEcho)
{
	strncpy(response,"pass",responseSize);
}


bool TestP4BridgeServerUtf8::TestUnicodeUserName()
{
    P4ClientError* connectionError = NULL;
    // create a new server using the alexi client
    //Алексей = "\xD0\x90\xD0\xbb\xD0\xB5\xD0\xBA\xD1\x81\xD0\xB5\xD0\xB9\0" IN utf-8
    ps8 = new P4BridgeServer("localhost:6666", "\xD0\x90\xD0\xBB\xD0\xB5\xD0\xBA\xD1\x81\xD0\xB5\xD0\xB9\0", "pass", "\xD0\x90\xD0\xbb\xD0\xB5\xD0\xBA\xD1\x81\xD0\xB5\xD0\xB9\0");

    bool rv = [&] {
        ASSERT_NOT_NULL(ps8);

        // connect and see if the api returned an error.
        CheckConnection(ps8, connectionError);

        ASSERT_INT_TRUE(ps8->unicodeServer());
        ps8->set_charset("utf8", "utf16le");

        bool needLogin = ps8->UseLogin();

        P4BridgeClient *myClient = ps8->get_ui(7);

        if (needLogin) 
        {
            ps8->SetPromptCallbackFn(ProvidePassword);

            ps8->set_user("\xD0\x90\xD0\xBB\xD0\xB5\xD0\xBA\xD1\x81\xD0\xB5\xD0\xB9\0");
            ps8->set_password("pass");

            const char *const loginArgs[] = {"-a"};
            ps8->run_command("login", 7, 0, loginArgs, 1);

            P4ClientInfoMsg *lout = ps8->get_ui(7)->GetInfoResults();
        }

        const char *const params[] = {"//depot/mycode/*"};

        ASSERT_INT_TRUE(ps8->run_command("files", 7, 0, params, 1))

        P4ClientInfoMsg *out = ps8->get_ui(7)->GetInfoResults();

        ASSERT_STRING_EQUAL(out->Message.c_str(), "//depot/MyCode/ReadMe.txt#1 - add change 1 (text)")
        ASSERT_NOT_NULL(out->Next)
        ASSERT_STRING_EQUAL(out->Next->Message.c_str(), "//depot/MyCode/Silly.bmp#1 - add change 1 (binary)")
        ASSERT_NOT_NULL(out->Next->Next)
        ASSERT_STRING_EQUAL(out->Next->Next->Message.c_str(),
                            "//depot/MyCode/\xD0\x9F\xD1\x8E\xD0\xBF.txt#1 - add change 3 (utf16)")

        return true;
    }();
    return rv;
}

bool TestP4BridgeServerUtf8::TestTaggedCommand()
{
    P4ClientError* connectionError = NULL;
    // create a new server
    ps8 = new P4BridgeServer("localhost:6666", "admin", "", testClient8 );

    bool rv = [&] {
        ASSERT_NOT_NULL(ps8);

        // connect and see if the api returned an error.
        CheckConnection(ps8, connectionError);

        ASSERT_INT_TRUE(ps8->unicodeServer());
        ps8->set_charset("utf8", "utf16le");

        const char *const params[] = {"//depot/mycode/*"};

        ASSERT_INT_TRUE(ps8->run_command("files", 7, 1, params, 1))

        StrDictListIterator *out = ps8->get_ui(7)->GetTaggedOutput();

        ASSERT_NOT_NULL(out);

        int itemCnt = 0;
        while (StrDictList *pItem = out->GetNextItem()) 
        {
            int entryCnt = 0;

            while (KeyValuePair *pEntry = out->GetNextEntry()) 
            {
                if ((itemCnt == 0) && (strcmp(pEntry->key.c_str(), "depotFile") == 0))
                    ASSERT_STRING_EQUAL(pEntry->value.c_str(), "//depot/MyCode/ReadMe.txt")
                if ((itemCnt == 1) && (strcmp(pEntry->key.c_str(), "depotFile") == 0))
                    ASSERT_STRING_EQUAL(pEntry->value.c_str(), "//depot/MyCode/Silly.bmp")
                if ((itemCnt == 2) && (strcmp(pEntry->key.c_str(), "depotFile") == 0))
                    ASSERT_STRING_EQUAL(pEntry->value.c_str(), "//depot/MyCode/\xD0\x9F\xD1\x8E\xD0\xBF.txt")
                entryCnt++;
            }
            ASSERT_NOT_EQUAL(entryCnt, 0);
            itemCnt++;
        }
        ASSERT_EQUAL(itemCnt, 3);

        delete out;

        return true;
    }();
    return rv;
}

bool TestP4BridgeServerUtf8::TestTextOutCommand()
{
    P4ClientError* connectionError = NULL;
    // create a new server
    ps8 = new P4BridgeServer("localhost:6666", "admin", "", testClient8 );

    bool rv = [&] {
        ASSERT_NOT_NULL(ps8);

        // connect and see if the api returned an error.
        CheckConnection(ps8, connectionError);

        ASSERT_INT_TRUE(ps8->unicodeServer());
        ps8->set_charset("utf8", "utf16le");

        const char *const params[] = {"//depot/MyCode/ReadMe.txt"};
        ASSERT_INT_TRUE(ps8->run_command("print", 7, 1, params, 1))

        const char *out = ps8->get_ui(7)->GetTextResults();

        ASSERT_NOT_NULL(out);
#ifndef OS_NT
        ASSERT_STRING_EQUAL(out, "Don't Read This!\r\n\r\nIt's Secret!")
#else
        ASSERT_STRING_EQUAL(out, "Don't Read This!\n\nIt's Secret!")
#endif

        return true;
    }();
    return rv;
}

bool TestP4BridgeServerUtf8::TestBinaryOutCommand()
{
    P4ClientError* connectionError = NULL;
    // create a new server
    ps8 = new P4BridgeServer("localhost:6666", "admin", "", testClient8 );
   
    bool rv = [&] {
        ASSERT_NOT_NULL(ps8);

        // connect and see if the api returned an error.
        CheckConnection(ps8, connectionError);

        ASSERT_INT_TRUE(ps8->unicodeServer());
        ps8->set_charset("utf8", "utf16le");

        const char *const params[] = {"//depot/MyCode/Silly.bmp"};

        ASSERT_INT_TRUE(ps8->run_command("print", 3, 1, params, 1))

        int cnt = ps8->get_ui(3)->GetBinaryResultsCount();

        ASSERT_EQUAL(cnt, 3126)

        const unsigned char *out = ps8->get_ui(3)->GetBinaryResults();

        ASSERT_NOT_NULL(out);
        ASSERT_EQUAL((*(((unsigned char *) out) + 1)), 0x4d)

        return true;
    }();
    return rv;
}

bool TestP4BridgeServerUtf8::TestErrorOutCommand()
{
    P4ClientError* connectionError = NULL;
    // create a new server
    ps8 = new P4BridgeServer("localhost:6666", "admin", "", testClient8 );
   
    bool rv = [&] {
        ASSERT_NOT_NULL(ps8);

        // connect and see if the api returned an error.
        CheckConnection(ps8, connectionError);

        ASSERT_INT_TRUE(ps8->unicodeServer());
        ps8->set_charset("utf8", "utf16le");

        const char *const params[] = {"//depot/MyCode/Billy.bmp"};

        // run a command against a nonexistent file
        // Should fail
        ASSERT_FALSE(ps8->run_command("rent", 88, 1, params, 1))

        P4ClientError *out = ps8->get_ui(88)->GetErrorResults();

        ASSERT_NOT_NULL(out);

        ASSERT_EQUAL(out->ErrorCode, 805379098);
        ASSERT_NULL(out->Next)

        return true;
    }();
    return rv;
}
