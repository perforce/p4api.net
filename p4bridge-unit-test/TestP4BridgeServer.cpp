#include "stdafx.h"
#include "UnitTestFrameWork.h"
#include "TestP4BridgeServer.h"
#include "UnitTestConfig.h"
#include "TextEncoder.h"
#include "enviro.h"

#include "../p4bridge/P4BridgeClient.h"
#include "../p4bridge/P4BridgeServer.h"
#include "../p4bridge/P4Connection.h"

#include <sys/types.h>
#include <sys/stat.h>

#ifdef OS_NT
#include <io.h>
#include <conio.h>
#else
#define MAX_PATH 260

#include <sys/types.h>
#include <sys/stat.h>
#include <unistd.h>
#include <fstream>
#include <iostream>

#endif

#include <ios>
#include <string>
#include <sstream>
#include <fstream>
#include <stdlib.h>

#ifdef OS_NT
// Does not return the right value, but is available in VS2010, and does most of what we want
#define snprintf(buf,len, format,...) _snprintf_s(buf, len,len, format, __VA_ARGS__)
#endif

CREATE_TEST_SUITE(TestP4BridgeServer)

TestP4BridgeServer::TestP4BridgeServer()
{
    UnitTestSuite::RegisterTest(TestGetConfig, "TestGetConfig");
    UnitTestSuite::RegisterTest(TestSetCwd, "TestSetCwd");
    UnitTestSuite::RegisterTest(TestGetSet, "TestGetSet");
    UnitTestSuite::RegisterTest(ServerConnectionTest, "ServerConnectionTest");
    UnitTestSuite::RegisterTest(TestUnicodeClientToNonUnicodeServer, "TestUnicodeClientToNonUnicodeServer");
    UnitTestSuite::RegisterTest(TestUnicodeUserName, "TestUnicodeUserName");
    UnitTestSuite::RegisterTest(TestUntaggedCommand, "TestUntaggedCommand");
    UnitTestSuite::RegisterTest(TestTaggedCommand, "TestTaggedCommand");
    UnitTestSuite::RegisterTest(TestTextOutCommand, "TestTextOutCommand");
    UnitTestSuite::RegisterTest(TestBinaryOutCommand, "TestBinaryOutCommand");
    UnitTestSuite::RegisterTest(TestErrorOutCommand, "TestErrorOutCommand");
    UnitTestSuite::RegisterTest(TestEnviro, "TestEnviro");
    UnitTestSuite::RegisterTest(TestConnectSetClient, "TestConnectSetClient");
    UnitTestSuite::RegisterTest(TestIsIgnored, "TestIsIgnored");
    UnitTestSuite::RegisterTest(TestSetVars, "TestSetVars");
    UnitTestSuite::RegisterTest(TestParallelSync, "TestParallelSync");
    UnitTestSuite::RegisterTest(TestDefaultProgramNameAndVersion, "TestDefaultProgramNameAndVersion");
    UnitTestSuite::RegisterTest(TestGetTicketFile, "TestGetTicketFile");
    UnitTestSuite::RegisterTest(TestSetTicketFile, "TestSetTicketFile");
#if !defined(OS_LINUX)
    UnitTestSuite::RegisterTest(TestParallelSyncCallback, "TestParallelSyncCallback");
#endif
    UnitTestSuite::RegisterTest(TestSetProtocol, "TestSetProtocol");
}


TestP4BridgeServer::~TestP4BridgeServer()
{
}

char unitTestSrcDir[MAX_PATH];
char unitTestZip[MAX_PATH];


#ifdef OS_NT
const char* testClient = "admin_space";
const char* testClient2 = "admin_space2";

const char* TestDir = MYTESTDIR;
const char* TestZip = MYTESTDIR "\\a.tar";
const char* utar_cmd = TAR " xvf a.tar";
const char* rcp_cmd = P4D " -r " MYTESTDIR " -jr checkpoint.1";
const char* udb_cmd = P4D " -r " MYTESTDIR " -xu";
const char* p4d_cmd = P4D " -p6666 -IdUnitTestServer -r" MYTESTDIR " -Llog";

const char* TestLog = MYTESTDIR "\\log";
const char* testCfgFile = MYTESTDIR "\\admin_space\\myP4Config.txt";
const char* testTicketFile = MYTESTDIR "\\admin_space\\p4tickets.txt";
const char* newTicketFile = MYTESTDIR "\\admin_space\\.p4tickets.txt";
const char* newEnviroFile = MYTESTDIR "\\admin_space\\.p4enviro.txt";
const char* testCfgDir = MYTESTDIR "\\admin_space";
const char* testCfgDir2 = MYTESTDIR "\\admin_space2";
const char* testIgnoreFile = MYTESTDIR "\\admin_space\\myP4Ignore.txt";
const char* testIgnoredFile1 = MYTESTDIR "\\admin_space\\foofoofoo.foo";
const char* testIgnoredFile2 = MYTESTDIR "\\admin_space\\moomoomoo.moo";
const char* testParallelDir1 = MYTESTDIR "\\admin_space\\TestData\\parallel\\";
const char* testParallelDir2 = MYTESTDIR "\\admin_space2\\TestData\\parallel\\";
const char* defaultApplication = "Perforce .NET API Bridge Unit Tests";
const char* defaultVersion = "2021.1.0.0";
const char* tarBall = "\\a.tar";
#endif

#if defined(OS_MACOSX)
const char* testClient = "osx_space";
const char* testClient2 = "osx_space2";
const char* utar_cmd = TAR " -xf a.tar";
#endif

#if defined(OS_LINUX)
const char* testClient = "linux_space";
const char* testClient2 = "linux_space2";
const char* utar_cmd = TAR " --warning=no-unknown-keyword -xf a.tar";
#endif

#if defined(OS_MACOSX) || defined(OS_LINUX)
const char* TestDir = MYTESTDIR;
const char* TestZip = MYTESTDIR "/a.tar";
const char* rcp_cmd = P4D " -C1 -r " MYTESTDIR " -jr checkpoint.1";
const char* udb_cmd = P4D " -C1 -r " MYTESTDIR " -xu";
const char* p4d_cmd = P4D " -C1 -p6666 -IdUnitTestServer -r" MYTESTDIR " -L log";
const char* TestLog = MYTESTDIR "/log";
const char* testCfgFile = MYTESTDIR "/admin_space/myP4Config.txt";
const char* testTicketFile = MYTESTDIR "/admin_space/p4tickets.txt";
const char* newTicketFile = MYTESTDIR "/admin_space/.p4tickets.txt";
const char* newEnviroFile = MYTESTDIR "/admin_space/.p4enviro.txt";
const char* testCfgDir = MYTESTDIR "/admin_space";
const char* testCfgDir2 = MYTESTDIR "/admin_space2";
const char* testIgnoreFile = MYTESTDIR "/admin_space/myP4Ignore.txt";
const char* testIgnoredFile1 = MYTESTDIR "/admin_space/foofoofoo.foo";
const char* testIgnoredFile2 = MYTESTDIR "/admin_space/moomoomoo.moo";
const char* testParallelDir1 = MYTESTDIR "/admin_space/TestData/parallel/";
const char* testParallelDir2 = MYTESTDIR "/admin_space2/TestData/parallel/";
const char* defaultVersion = "1.0.0.1";
const char* tarBall = "/a.tar";
#endif

const char* testProgramName = "BridgeUnitTests";
const char* testProgramVer = "1.2.3.4.A.b.C";

#ifdef OS_NT
LPPROCESS_INFORMATION pi = nullptr;
#else
int pi = 0;
#endif

P4BridgeServer* ps = nullptr;

// check for connection failure to p4d
bool TestP4BridgeServer::CheckConnection(P4BridgeServer* ps, P4ClientError* connectionError) {
    if (!ps->connected(&connectionError)) {
        char buff[256];
        snprintf(buff, sizeof(buff) - 1, "Connection error: %s", connectionError->Message.c_str());
        // Abort if the connect did not succeed
        ASSERT_FAIL(buff);
    }
    return true;
}

string oldConfig;
string oldIgnore;

bool TestP4BridgeServer::Setup()
{
    // remove the test directory if it exists
    if (!UnitTestSuite::rmDir(TestDir))
        return false;

    UnitTestFrameWork::getSrcDir(unitTestSrcDir, sizeof(unitTestSrcDir));
    strcpy(unitTestZip, unitTestSrcDir);
    strcat(unitTestZip, tarBall);

    if (!UnitTestSuite::mkDir(TestDir))
        return false;

    if (!UnitTestSuite::copyFile(unitTestZip, TestZip))
        return false;

    UnitTestSuite::chDir(TestDir);

    pi = UnitTestSuite::RunProgram(utar_cmd, TestDir, true, true);
    if (!pi)
    {
        UnitTestSuite::chDir(unitTestSrcDir);
        return false;
    }
    CleanResults(pi);

    pi = UnitTestSuite::RunProgram(rcp_cmd, TestDir, true, true);
    if (!pi)
    {
        UnitTestSuite::chDir(unitTestSrcDir);
        return false;
    }
    CleanResults(pi);

    pi = UnitTestSuite::RunProgram(udb_cmd, TestDir, true, true);
    if (!pi) {
        UnitTestSuite::chDir(unitTestSrcDir);
        return false;
    }
    CleanResults(pi);

    pi = UnitTestSuite::RunProgram(p4d_cmd, TestDir, false, false);
    if (!pi)
    {
        UnitTestSuite::chDir(unitTestSrcDir);
        return false;
    }

#ifndef OS_NT
    sleep(2);  // give the server time to get ready...
#endif

// change default .p4enviro and .p4tickets file locations
// so we don't trash the build system environment during our tests.

#ifdef OS_NT
    SetEnvironmentVariable("P4TICKETS", newTicketFile);
    SetEnvironmentVariable("P4ENVIRO", newEnviroFile);
#else
    setenv("P4TICKETS", newTicketFile, 1);
    setenv("P4ENVIRO", newEnviroFile, 1);
#endif

    return true;
}

int TestP4BridgeServer::LogCallback(int level, const char* file, int line, const char* msg)
{
    printf("LOG: %d %s:%d %s\n", level, file, line, msg);
    return 1;
}

bool TestP4BridgeServer::TearDown(const char* testName)
{
    UnitTestSuite::rmDir(testCfgFile);
    UnitTestSuite::rmDir(testIgnoreFile);
    if (!oldConfig.empty())
    {
        P4BridgeServer::Set("P4CONFIG", oldConfig.c_str());
        oldConfig.clear();
    }
    if (!oldIgnore.empty())
    {
        P4BridgeServer::Set("P4IGNORE", oldIgnore.c_str());
        oldIgnore.clear();
    }
    if (pi)
        UnitTestSuite::EndProcess(pi);
    if (ps)
    {
        delete ps;
        ps = nullptr;

#ifdef _DEBUG_MEMORY
        p4base::DumpMemoryState("After deleting server");
#endif
    }
    UnitTestSuite::chDir(unitTestSrcDir);
    UnitTestSuite::rmDir(TestDir);

#ifdef _DEBUG_MEMORY
    p4base::PrintMemoryState(testName);
#endif
    return true;
}

bool TestP4BridgeServer::ServerConnectionTest()
{
    P4ClientError* connectionError = nullptr;
    // create a new server
    ps = new P4BridgeServer("localhost:6666", "admin", "", "");
    bool rv = [&] {
        ASSERT_NOT_NULL(ps);
        //ps->SetLogCallFn((LogCallbackFn *) &TestP4BridgeServer::LogCallback);

        // connect and see if the api returned an error. 
        bool rval = CheckConnection(ps, connectionError);
        return rval;
    }();
    return rv;
}

bool TestP4BridgeServer::CreateClient(const char* name, const char* workspace_root)
{
    P4ClientError* connectionError = nullptr;
    ps = new P4BridgeServer("localhost:6666", "admin", "", "");

    // connect and see if the api returned an error.
    if (!CheckConnection(ps, connectionError))
        return false;

    const char* const params[] = { "-o", name };

    bool rv = ps->run_command("client", 2345, 1, params, 2);

    StrDictListIterator* it = ps->get_ui(2345)->GetTaggedOutput();
    if (it == nullptr)
        return false;

    // use a StrBuf for the edited client spec
    StrBuf new_client;

    const char* eol = "\n";
    int itemCnt = 0;
    while (StrDictList* pItem = it->GetNextItem())
    {
        while (KeyValuePair* pEntry = it->GetNextEntry()) {
            const char* key = pEntry->key.c_str();
            if (!strcmp(key, "Host") || !strcmp(key, "specdef"))
            {
                // Skip any Host or specdef entry
            }
            else if (!strcmp(key, "Client"))
            {
                new_client << key << ":\t" << name << eol;
            }
            else if (!strcmp(key, "Description"))
            {
                new_client << key << ":\n\t" << "Created by TestP4BridgeServer.cpp" << eol;
            }
            else if (!strcmp(key, "Root"))
            {
                new_client << key << ":\t" << workspace_root << eol;
            }
            else if (!strncmp(pEntry->key.c_str(), "View", 4))
            {
                new_client << "View:\n\t" << "//depot/..." << " " << "//" << name << "/..." << eol;
            }
            else
            {
                new_client << key << ":\t" << pEntry->value.c_str() << eol;
            }
        }
    }
    delete it;

    // use the new_client StrBuf for input.
    ps->get_ui()->SetDataSet(new_client.Text());
    const char* const params1[] = { "-i" };

    rv = ps->run_command("client", 2345, 0, params1, 1);
    delete ps;
    ps = nullptr;

    return rv;
}

bool TestP4BridgeServer::TestUnicodeClientToNonUnicodeServer()
{
    P4ClientError* connectionError = nullptr;

    // create a new server
    ps = new P4BridgeServer("localhost:6666", "admin", "", testClient);

    bool rv = [&] {
        ASSERT_NOT_NULL(ps);

        // connect and see if the api returned an error. 
        if (!CheckConnection(ps, connectionError))
            return false;

        ASSERT_NOT_EQUAL(ps->unicodeServer(), 1);
        ps->set_charset("utf8", "utf16le");

        const char* const params[] = { "//depot/MyCode/*" };

        ASSERT_FALSE(ps->run_command("files", 3456, 0, params, 1))

            P4ClientError* out = ps->get_ui(3456)->GetErrorResults();

        ASSERT_STRING_STARTS_WITH(out->Message.c_str(), "Unicode clients require a unicode enabled server.")
            return true;
    }();

    return rv;
}

bool TestP4BridgeServer::TestUnicodeUserName()
{

    P4ClientError* connectionError = nullptr;

    bool rv = [&] {
        // create a new server
        //Aleksey (Alexei) in Cyrillic = "\xD0\x90\xD0\xbb\xD0\xB5\xD0\xBA\xD1\x81\xD0\xB5\xD0\xB9\0" IN utf-8
        ps = new P4BridgeServer("localhost:6666", "\xD0\x90\xD0\xBB\xD0\xB5\xD0\xBA\xD1\x81\xD0\xB5\xD0\xB9\0", "pass",
            "\xD0\x90\xD0\xbb\xD0\xB5\xD0\xBA\xD1\x81\xD0\xB5\xD0\xB9\0");
        ASSERT_NOT_NULL(ps);

        // connect and see if the api returned an error. 
        if (!CheckConnection(ps, connectionError))
            return false;

        ASSERT_FALSE(ps->unicodeServer());
        ps->set_charset("utf8", "utf16le");
        // ps->set_charset("none", "none");                                  // This disables client P4CHARSET

        const char* const params[] = { "//depot/MyCode/*" };

        ASSERT_FALSE(ps->run_command("files", 7, 0, params, 1))

            P4ClientError* out = ps->get_ui(7)->GetErrorResults();

        ASSERT_STRING_STARTS_WITH(out->Message.c_str(), "Unicode clients require a unicode enabled server.")

            return true;
    }();
    return rv;
}

bool TestP4BridgeServer::TestUntaggedCommand()
{
    P4ClientError* connectionError = nullptr;

    ps = new P4BridgeServer("localhost:6666", "admin", "", testClient);

    bool rv = [&] {
        ASSERT_NOT_NULL(ps);

        // connect and see if the api returned an error. 
        if (!CheckConnection(ps, connectionError))
            return false;

        const char* const params[] = { "//depot/MyCode/*" };

        ASSERT_INT_TRUE(ps->run_command("files", 7, 0, params, 1))

            P4ClientInfoMsg* out = ps->get_ui(7)->GetInfoResults();

        ASSERT_NOT_NULL(out);
        ASSERT_STRING_STARTS_WITH(out->Message.c_str(), "//depot/MyCode/")

            return true;
    }();

    return rv;
}

bool TestP4BridgeServer::TestTaggedCommand()
{
    P4ClientError* connectionError = nullptr;
    // create a new server
    ps = new P4BridgeServer("localhost:6666", "admin", "", testClient);

    bool rv = [&] {
        ASSERT_NOT_NULL(ps);

        // connect and see if the api returned an error. 
        if (!CheckConnection(ps, connectionError))
            return false;

        const char* const params[] = { "//depot/MyCode/*" };

        ASSERT_INT_TRUE(ps->run_command("files", 7, 1, params, 1))

            StrDictListIterator* out = ps->get_ui(7)->GetTaggedOutput();

        ASSERT_NOT_NULL(out);

        int itemCnt = 0;
        while (StrDictList* pItem = out->GetNextItem())
        {
            int entryCnt = 0;

            while (KeyValuePair* pEntry = out->GetNextEntry())
            {
                ASSERT_TRUE(pEntry->key != "func");
                if ((itemCnt == 0) && (strcmp(pEntry->key.c_str(), "depotFile") == 0))
                    ASSERT_STRING_STARTS_WITH(pEntry->value.c_str(), "//depot/MyCode/")
                    if ((itemCnt == 1) && (strcmp(pEntry->key.c_str(), "depotFile") == 0))
                        ASSERT_STRING_STARTS_WITH(pEntry->value.c_str(), "//depot/MyCode/")
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

bool TestP4BridgeServer::TestTextOutCommand()
{
    P4ClientError* connectionError = nullptr;
    // create a new server
    ps = new P4BridgeServer("localhost:6666", "admin", "", testClient);

    bool rv = [&] {
        ASSERT_NOT_NULL(ps);

        // connect and see if the api returned an error. 
        if (!CheckConnection(ps, connectionError))
            return false;

        const char* const params[] = { "//depot/MyCode/ReadMe.txt" };

        ASSERT_INT_TRUE(ps->run_command("print", 7, 1, params, 1))

            const char* out = ps->get_ui(7)->GetTextResults();

        ASSERT_NOT_NULL(out);

#ifdef OS_NT
        ASSERT_STRING_EQUAL(out, "Don't Read This!\n\nIt's Secret!")
#else
        ASSERT_STRING_EQUAL(out, "Don't Read This!\r\n\r\nIt's Secret!")
#endif
            return true;
    }();

    return rv;
}

bool TestP4BridgeServer::TestBinaryOutCommand()
{
    P4ClientError* connectionError = nullptr;
    // create a new server
    ps = new P4BridgeServer("localhost:6666", "admin", "", testClient);

    bool rv = [&] {
        ASSERT_NOT_NULL(ps);

        // connect and see if the api returned an error. 
        if (!CheckConnection(ps, connectionError))
            return false;

        const char* const params[] = { "//depot/MyCode/Silly.bmp" };

        ASSERT_INT_TRUE(ps->run_command("print", 7, 1, params, 1))

            size_t cnt = ps->get_ui(7)->GetBinaryResultsCount();

        ASSERT_EQUAL(cnt, 3126)

            const unsigned char* out = ps->get_ui(7)->GetBinaryResults();

        ASSERT_NOT_NULL(out);
        ASSERT_EQUAL((*(((unsigned char*)out) + 1)), 0x4d)

            return true;
    }();
    return rv;
}

bool TestP4BridgeServer::TestErrorOutCommand()
{
    P4ClientError* connectionError = nullptr;
    // create a new server
    ps = new P4BridgeServer("localhost:6666", "admin", "", testClient);

    bool rv = [&] {
        ASSERT_NOT_NULL(ps);

        // connect and see if the api returned an error. 
        if (!CheckConnection(ps, connectionError))
            return false;

        const char* const params[] = { "//depot/MyCode/Billy.bmp" };
        // run a command against a nonexistent file
        // Should fail
        ASSERT_FALSE(ps->run_command("rent", 7, 1, params, 1))

            P4ClientError* out = ps->get_ui(7)->GetErrorResults();

        ASSERT_NOT_NULL(out);

        //ASSERT_STRING_STARTS_WITH(out->Message, "Unknown command.  Try 'p4 help' for info")
        ASSERT_EQUAL(out->ErrorCode, 805379098);
        ASSERT_NULL(out->Next)
            return true;
    }();

    return rv;
}

bool TestP4BridgeServer::TestGetSet()
{
    const char* expected = "C:\\login.bat";

    const char* test_charset = "utf16";

    // first thing: delete P4FOOBAR from the registry
    P4BridgeServer::Set("P4FOOBAR", nullptr);
    P4BridgeServer::Reload();

    P4ClientError* connectionError = nullptr;
    // create a new server
    ps = new P4BridgeServer("localhost:6666", "admin", "", "");

    bool rv = [&] {
        ASSERT_NOT_NULL(ps);
        //ps->SetLogCallFn((LogCallbackFn *) &TestP4BridgeServer::LogCallback);
        ASSERT_NOT_NULL(ps);

        if (!CheckConnection(ps, connectionError))
            return false;

        // set server to use test_charset for file translation
        ps->set_charset(test_charset);

        // get charset from server
        string cset = ps->get_charset();

        ASSERT_STRING_EQUAL(test_charset, cset.c_str());

        // Test the P4BridgeEnviro Update() function
        // Any values set with Update should be returned
        // before any set in the Environment or the Registry or the settings file
        P4BridgeServer::Update("P4FOOBAR", expected);

        const char* result = P4BridgeServer::Get("P4FOOBAR");

        ASSERT_STRING_EQUAL(expected, result);

        // remove any Update() settings,
        P4BridgeServer::Reload();

        // should be null (unless P4FOOBAR is set in the environment or registry, which is unlikely)
        result = P4BridgeServer::Get("P4FOOBAR");
        ASSERT_NULL(result);

        const wchar_t* w_expected = L"C:\\login<АБВГ>.bat";
        expected = TextEncoder::Utf16ToUtf8(w_expected);

        P4BridgeServer::Set("P4FOOBAR", expected);

        result = P4BridgeServer::Get("P4FOOBAR");

        ASSERT_NOT_NULL(result)
            ASSERT_STRING_EQUAL(expected, result)
            delete[] expected;

        wchar_t* w_result = TextEncoder::Utf8ToUtf16(result);

        ASSERT_W_STRING_EQUAL(w_expected, w_result);
        delete[] w_result;

        P4BridgeServer::Set("P4FOOBAR", NULL);
        result = P4BridgeServer::Get("P4FOOBAR");
        // P4-16150, Set + Get on Windows returns a cached value
        //	ASSERT_NULL(result);  // test disabled until the P4API bug is fixed

#ifdef OS_NT
    // Clear any existing P4CONFIG environment variable
        ASSERT_FALSE(_putenv("P4CONFIG="))
#else
        char p4config[] = "P4CONFIG";
        unsetenv(p4config);  // clear P4CONFIG from environment
#endif
        P4BridgeServer::GetEnviro()->Reload();

        // P4BridgeServer::ListEnviro();
        // calling ::Set below will change the allocation of the
        // strptr that ::Get is pointing at.  you have to cache
        // the value (string object) if you need it later
        const char* _orig = P4BridgeServer::Get("P4CONFIG");
        string orig = (_orig == NULL) ? "" : _orig;
        string actual = "not_a_real_config_setting";

        P4BridgeServer::Set("P4CONFIG", actual.c_str());

        string getResult = P4BridgeServer::Get("P4CONFIG");
        ASSERT_STRING_EQUAL(actual.c_str(), getResult.c_str())

            P4BridgeServer::Set("P4CONFIG", orig.c_str());
        const char* _getResult = P4BridgeServer::Get("P4CONFIG");
        if (!_getResult)
        {
            ASSERT_NULL(_orig);
        }
        else
        {
            // This test fails on Windows because you cannot "clear" an existing value using an empty string
            // Disabled until P4-16150 job092355 gets fixed
#ifndef OS_NT
            ASSERT_STRING_EQUAL(orig.c_str(), _getResult);
#endif
        }

#ifdef OS_NT
        ASSERT_FALSE(_putenv("P4ENVIRO="))
#else
        char p4enviro[] = "P4ENVIRO";
        unsetenv(p4enviro);   //Restore the default .p4enviro location
#endif

        return true;
    }();
    return rv;
}

bool TestP4BridgeServer::TestEnviro()
{
    bool rv = [] {
        // remove any Local override
        P4BridgeServer::Reload();

        // Save the existing value
        const char* _existing = P4BridgeServer::Get("P4CONFIG");
        string existing = (_existing) ? _existing : "";

        // Override the value with Update()
        P4BridgeServer::Update("P4CONFIG", "myP4Config.txt");
        string result1 = P4BridgeServer::Get("P4CONFIG");
        printf("result1: %s\n", result1.c_str());

        // Check with a value not in the environment
        P4BridgeServer::Update("P4NOENV", "myP4NOENV.txt");
        string result2 = P4BridgeServer::Get("P4NOENV");
        printf("result2: %s\n", result2.c_str());

        ASSERT_STRING_EQUAL(result2.c_str(), "myP4NOENV.txt");

        // Now try to clear the value locally
        P4BridgeServer::Update("P4CONFIG", "");
        const char* pResult = P4BridgeServer::Get("P4CONFIG");
        ASSERT_NULL(pResult);

        // Now again remove the Local override
        P4BridgeServer::Reload();
        const char* result4 = P4BridgeServer::Get("P4CONFIG");

        // we should get what we had originally
        if (!result4)
        {
            ASSERT_NULL(_existing);
        }
        else {
            ASSERT_STRING_EQUAL(result4, existing.c_str());
        }

        return true;
    }();
    return rv;
}

bool TestP4BridgeServer::TestConnectSetClient()
{
    ps = new P4BridgeServer("localhost:6666", "admin", NULL, NULL);
    ASSERT_NOT_NULL(ps);
    ps->set_client(testClient);

    // check to see that the client is set correctly
    string client = ps->get_client();
    ASSERT_STRING_EQUAL(client.c_str(), testClient);

    return true;
}

void WriteConfigFile(const char* file)
{
    std::filebuf fb;
    fb.open(file, std::ios::out);
    std::ostream os(&fb);
    os << "P4PORT=localhost:6666\n";
    os << "P4USER=admin\n";
    os << "P4CLIENT=testClient\n";
    fb.close();
}

// this function is in p4bridge-api.cpp
extern "C" P4BridgeServer * ConnectionFromPath(const char* cwd);

bool TestP4BridgeServer::TestGetConfig()
{
    bool rv = [] {

        ASSERT_TRUE(TestP4BridgeServer::CreateClient(testClient, testCfgDir));

        // grab the global enviro's P4CONFIG
        const char* _oldConfig = P4BridgeServer::Get("P4CONFIG");
        string oldConfigPath;
        if (_oldConfig == NULL || *_oldConfig == 0)
        {
            _oldConfig = "noconfig";
            oldConfigPath = _oldConfig;
        }
        else
        {
            #ifdef OS_NT
                oldConfigPath = (string(testCfgDir) + "\\" + _oldConfig);
            #else
                oldConfigPath = (string(testCfgDir) + "/" + _oldConfig);
            #endif
        }

        // write 2 config files for the enviro to pick up
        WriteConfigFile(testCfgFile);
        WriteConfigFile(oldConfigPath.c_str());

        const char* oldConfig = P4BridgeServer::Get("P4CONFIG");

        // NOTE: Update() merely caches a value local to the underlying enviro object
        // This overrides environment and registry.
        // If you call Get() it will return that value
        // If you cause the enviro to reload, the cached value will be lost
        P4BridgeServer::Update("P4CONFIG", "myP4Config.txt");
        string newConfig = P4BridgeServer::Get("P4CONFIG");
        ASSERT_STRING_EQUAL(newConfig.c_str(), "myP4Config.txt");

        // get_config should not use the "default" enviro object
        // other than to get the relevant P4CONFIG value
        string result = P4BridgeServer::get_config(testCfgDir);
        ASSERT_STRING_EQUAL(testCfgFile, result.c_str());

        // Make sure that get_config didn't change P4CONFIG value
        string newConfig1 = P4BridgeServer::Get("P4CONFIG");
        ASSERT_STRING_EQUAL(newConfig1.c_str(), "myP4Config.txt");

        // create a new server
        ps = new P4BridgeServer("localhost:6666", "admin", "", testClient);
        ASSERT_NOT_NULL(ps);

        // Associate the server with a directory
        ps->set_cwd(testCfgDir);
        string psconfig = ps->get_config();  // what config does the BridgeServer Have?

        // uncomment the next line if you are after detailed logging for the whole test
        // the logCallbackFn * is static :)
        //  ps->SetLogCallFn((LogCallbackFn *)&TestP4BridgeServer::LogCallback);

        // set the cwd to null (should not fail)
        ps->set_cwd(NULL);

        // Lets check p4bridge-api.cpp ConnectionFromPath(cwd)
        string newConfig3 = P4BridgeServer::GetEnviro()->Get("P4CONFIG");
        ASSERT_STRING_EQUAL(newConfig3.c_str(), "myP4Config.txt");

        P4BridgeServer* svr = ConnectionFromPath(testCfgDir);

        // after ConnectionFromPath the P4BridgeServer returned should have P4Config file settings
        string t_port = svr->get_port();
        string t_client = svr->get_client();
        svr->close_connection();

        delete svr;

        // Make sure that ConnectionFromPath didn't change the previous P4CONFIG UPDATE value
        string newConfig2 = P4BridgeServer::Get("P4CONFIG");
        ASSERT_STRING_EQUAL(newConfig2.c_str(), "myP4Config.txt");

        // this will reset ps's enviro, which means P4CONFIG will become the default again
        P4BridgeServer::Reload();

        const char* cfg1 = P4BridgeServer::Get("P4CONFIG");

        // get_config should pick up the changed P4CONFIG and find a different config path
        string result1 = P4BridgeServer::get_config(testCfgDir);
        ASSERT_STRING_EQUAL(oldConfigPath.c_str(), result1.c_str());

        // after the reload, reassociate the bridge server with a directory
        // this will allow the P4CONFIG change to be picked up.
        ps->set_cwd(testCfgDir);

        const char* cfg2 = P4BridgeServer::Get("P4CONFIG");
        result = ps->get_config();

        ASSERT_STRING_EQUAL(oldConfigPath.c_str(), result.c_str());

        return true;
    }();
    return rv;
}

bool TestP4BridgeServer::TestSetCwd()
{
    bool rv = [] {

        ASSERT_TRUE(TestP4BridgeServer::CreateClient(testClient, testCfgDir));

        // create a new server
        ps = new P4BridgeServer("localhost:6666", "admin", "", testClient);
        ASSERT_NOT_NULL(ps);

        // Associate the server with a directory
        string cwdBefore_TestCfgDir = ps->get_cwd();
        ps->set_cwd(testCfgDir);
        string cwdAfter_TestCfgDir = ps->get_cwd();

        ASSERT_STRING_EQUAL(testCfgDir, cwdAfter_TestCfgDir.c_str());

        // uncomment the next line if you are after detailed logging for the whole test
        // the logCallbackFn * is static :)
        //  ps->SetLogCallFn((LogCallbackFn *)&TestP4BridgeServer::LogCallback);

        // set the cwd to null (should not fail)
        string cwdBefore_Null = ps->get_cwd();
        ps->set_cwd(NULL);
        string cwdAfter_Null = ps->get_cwd();

        ASSERT_STRING_EQUAL(cwdBefore_TestCfgDir.c_str(), cwdAfter_Null.c_str());

        return true;
    }();
    return rv;
}


bool TestP4BridgeServer::TestIsIgnored()
{
    bool rv = [] {

        const char* pIgnore = P4BridgeServer::Get("P4IGNORE");
        oldIgnore = (pIgnore ? pIgnore : "");

        std::ofstream out(testIgnoreFile);
        out << "foofoofoo.foo\n";
        out.close();

        // Clear any existing environment variable for P4IGNORE
#ifdef OS_NT
        ASSERT_FALSE(_putenv("P4IGNORE="))
#else
        char p4ignore[] = "P4IGNORE";
        unsetenv(p4ignore);  // clear P4IGNORE from environment
#endif
    // need to use Update, because in linux or osx an existing environment variable may exist, which is higher precedence than a Set() call.
        P4BridgeServer::Update("P4IGNORE", "myP4Ignore.txt");
        //P4BridgeServer::GetEnviro()->List();

        ASSERT_INT_TRUE(P4BridgeServer::IsIgnored(StrRef(testIgnoredFile1)));
        ASSERT_FALSE(P4BridgeServer::IsIgnored(StrRef(testIgnoredFile2)));

        return true;
    }();
    return rv;
}

//    Some experimentation with command options happened in this test.
//     There is a lot of supporting code which is still there, but unneeded for just the test
bool TestP4BridgeServer::TestSetVars()
{
    Error e;

    ASSERT_TRUE(TestP4BridgeServer::CreateClient(testClient, testCfgDir));

    // test data for reading Protocols (these are read after client::Init())
    const char* name = "apilevel";
    int value = 38;

    const char* name1 = "server";
    const char* value1 = "53";    // 2018.2=(46)  2020.2=(51) 2021.1=(52) 2021.2=(53) see "server protocol levels" in Helix Core Server Administrator Guide

    const char* name2 = "nocase";
    const char* value2 = "1";

    const char* name3 = "security";
    const char* value3 = "0";

    const char* name4 = "unicode";
    const char* value4 = "0";

    P4ClientError* connectionError = nullptr;
    StrPtr* pptr = nullptr;

    // create a new server
    ps = new P4BridgeServer("localhost:6666", "admin", "", testClient);

    bool rv = [&] {
        ASSERT_NOT_NULL(ps);

        // enable logging of C++ events
        //ps->SetLogCallFn((LogCallbackFn *)&TestP4BridgeServer::LogCallback);  // Works

        // Protocol options must be set before client::Init()
        // -Z options are set by Protocol
        //ps->SetProtocol("dbstat","");    // "track", "dbstat", "app=name", "proxyverbose"
        //ps->SetProtocol(P4Tag::v_tag, "yes");  // enable tagged output  WORKS
        // ps->SetProtocol("track", ""); // enable tracking WORKS

        // connect and see if the api returned an error. 
        if (!CheckConnection(ps, connectionError))
            return false;

        P4Connection* client = ps->getConnection(0);
        P4BridgeClient* bridge_ui = ps->get_ui(0);

        // What command to send?
        const char* cmdname = "users";
        const char* const v[] = { "-m", "10" };
        client->SetArgv(2, (char* const*)v);

        // Interestingly,  the "tag" option works as both a Var and a Protocol
        //client->SetVar(P4Tag::v_tag, "yes");  // enable tagged output  WORKS

        // -z options maxLockTime maxOpenFiles maxResults maxScanRows
        //client->SetVarV("maxOpenFiles=1");  

        // -v options are passed by p4debug.SetLevel() calls.
        // p4debug.SetLevel("rpc=3");   // This will spew debug output to stdout WORKS
        // p4debug.SetLevel(5);		// This will spew ALL debug output to stdout WORKS

    //p4debug.SetLevel("track=1");

        ps->Run_int(client, cmdname, bridge_ui);

        //p4debug.SetLevel(0);   // Disable Debug output WORKS

        StrDictListIterator* tagout = bridge_ui->GetTaggedOutput();
        const char* tout = bridge_ui->GetTextResults();
        StrPtr* tcount = bridge_ui->GetDataSet();

        const unsigned char* bout = bridge_ui->GetBinaryResults();
        size_t bcount = bridge_ui->GetBinaryResultsCount();

        P4ClientInfoMsg* iout = bridge_ui->GetInfoResults();
        int icount = bridge_ui->GetInfoResultsCount();
        P4ClientError* eout = bridge_ui->GetErrorResults();

        if (icount > 0 && iout) {
            P4ClientInfoMsg* imsg = iout;
            while (imsg)
            {
                printf("info: %d %s\n", imsg->Level, imsg->Message.c_str());
                imsg = imsg->Next;
            }

        }
        //ASSERT_NOT_NULL(tagout);

        if (tagout)  // Tagged output
        {
            int itemCnt = 0;
            while (StrDictList* pItem = tagout->GetNextItem())
            {
                int entryCnt = 0;

                while (KeyValuePair* pEntry = tagout->GetNextEntry())
                {
                    printf("tag: %d %s => %s\n", itemCnt, pEntry->key.c_str(), pEntry->value.c_str());
                    entryCnt++;
                }
                itemCnt++;
            }
        }

        if (tout) // Text output
        {
            printf("text: %s", tout);
        }

        int level = ps->APILevel();

        if (level < value)   // Server2 38 or larger is ok
        {
            char buff[256];
            snprintf(buff, sizeof(buff) - 1, "API mismatch %d < %d", level, value);
            ASSERT_FAIL(buff);
        }

        pptr = client->GetProtocol(name1);
        ASSERT_NOT_NULL(pptr);

        if (strcmp(value1, pptr->Value()))
        {
            char buff[256];
            snprintf(buff, sizeof(buff) - 1, "Value1 mismatch %s != %s", value1, pptr->Value());
            ASSERT_FAIL(buff);
        }

        pptr = client->GetProtocol(name2);
        ASSERT_NOT_NULL(pptr);

        if (strcmp(value2, pptr->Value()))
        {
            char buff[256];
            snprintf(buff, sizeof(buff) - 1, "Value2 mismatch %s != %s", value2, pptr->Value());
            ASSERT_FAIL(buff);
        }

        pptr = client->GetProtocol(name3);
        ASSERT_NOT_NULL(pptr);

        if (strcmp(value3, pptr->Value()))
        {
            char buff[256];
            snprintf(buff, 255, "Value3 mismatch %s != %s", value3, pptr->Value());
            ASSERT_FAIL(buff);
        }

        pptr = client->GetProtocol(name4);
        ASSERT_NOT_NULL(pptr);

        if (strcmp(value4, pptr->Value()))
        {
            char buff[256];
            snprintf(buff, sizeof(buff) - 1, "Value4 mismatch %s != %s", value4, pptr->Value());
            ASSERT_FAIL(buff);
        }
        return true;
    }();
    return rv;
}

// A test for Parallel Sync error handling - Job085941

bool TestP4BridgeServer::TestParallelSync()
{
    P4ClientError* connectionError = nullptr;

    ASSERT_TRUE(TestP4BridgeServer::CreateClient(testClient, testCfgDir));
    ASSERT_TRUE(TestP4BridgeServer::CreateClient(testClient2, testCfgDir2));

    // create a new server
    ps = new P4BridgeServer("localhost:6666", "admin", "", testClient);

    bool rval = [&] {
        ASSERT_NOT_NULL(ps);

        // connect and see if the api returned an error. 
        if (!CheckConnection(ps, connectionError))
            return false;

        ASSERT_NOT_EQUAL(ps->unicodeServer(), 1);
        // ps->set_charset("utf8", "utf16le");

         // run p4 configure set net.parallel.max=4	
        const char* args[] = { "set", "net.parallel.max=4" };
        ASSERT_INT_TRUE(ps->run_command("configure", 0, 1, args, 2));

        // run p4 configure set dmc=3
        const char* args1[] = { "set", "dmc=3" };
        ASSERT_INT_TRUE(ps->run_command("configure", 0, 1, args1, 2));

        // Paths of interest for the parallel test.
        std::string ppath = testParallelDir1;
        std::string ppathall = ppath + "...";
        std::string ppathnone = ppathall + "#none";
        std::string targetpath = ppath + "testFile99.txt";

        std::string ppath1 = testParallelDir2;
        std::string targetpath1 = ppath1 + "testFile99.txt";

        // Create the workspace directory for the parallel test
        UnitTestSuite::mkDir(ppath.c_str());

        // write 500 files of 1K size to the workspace directory
        int count = 500;
        for (int i = 0; i < count; i++)
        {
            std::ostringstream oss;
            oss << ppath << "testFile" << i << ".txt";

            std::ofstream ofs(oss.str(), std::ios::binary | std::ios::out);
            ofs.seekp(1020);
            ofs.write("test", 4);
            ofs.close();
        }

        // add them to the server
        char const* args2[] = { ppathall.c_str() };
        ASSERT_INT_TRUE(ps->run_command("add", 0, 1, args2, 1));

        // submit them
        char const* args3[] = { "-d", "\"initial submit of test files\"", ppathall.c_str() };
        ASSERT_INT_TRUE(ps->run_command("submit", 0, 1, args3, 3));

        // Remove all Workspace Files (Sync to NONE)
        char const* args4[] = { "-f", ppathnone.c_str() };
        ASSERT_INT_TRUE(ps->run_command("sync", 0, 1, args4, 2));

        // Sync just the target file
        char const* args5[] = { "-f", targetpath.c_str() };
        ASSERT_INT_TRUE(ps->run_command("sync", 0, 1, args5, 2));

        // Change target file to Writable
#ifdef OS_NT
        _chmod(targetpath.c_str(), _S_IWRITE | _S_IREAD);
#else
        chmod(targetpath.c_str(), S_IRUSR | S_IWUSR | S_IRGRP | S_IWGRP);
#endif
        // disconnect and reconnect in order to properly pick up net.parallel.max setting
        ps->disconnect();
        delete ps;

        // reconnect, and switch to a different client
        ps = new P4BridgeServer("localhost:6666", "admin", "", testClient2);
        ASSERT_NOT_NULL(ps);

        // connect and see if the api returned an error.
        if (!CheckConnection(ps, connectionError))
            return false;

        // look at config settings
        // run p4 configure show
        const char* args9[] = { "show" };
        ASSERT_INT_TRUE(ps->run_command("configure", 0, 1, args9, 1));

        // Sync target file
        char const* args6[] = { "-f", targetpath1.c_str() };
        ASSERT_INT_TRUE(ps->run_command("sync", 0, 1, args6, 2));

        // Edit the target file
        char const* args7[] = { targetpath1.c_str() };
        ASSERT_INT_TRUE(ps->run_command("edit", 0, 1, args7, 1));

        // Submit the target file
        char const* args8[] = { "-d", "\"submit of test file\"", targetpath1.c_str() };
        ASSERT_INT_TRUE(ps->run_command("submit", 0, 1, args8, 3));

        // Back to the original workspace
        ps->set_client(testClient);

        // Do a parallel sync of the original workspace, causes overwrite error, should fail
        char const* args9a[] = { "--parallel", "threads=4,batch=8,batchsize=2000,min=9,minsize=2000",
                               ppathall.c_str() };
        int rv = ps->run_command("sync", 0, 1, args9a, 3);
        ASSERT_INT_FALSE(rv);  // Expect failure
        return true;
    }();
    return rval;
}

int STDCALL MyParallelSyncCallbackFn(int* pServer, const char* cmd, const char** pArgs, int argCount, int* dictListIter, int threads)
{
    // stub: print the args out
    printf("P4BridgeServer: 0x[0x%p]\n", pServer);
    printf("Command: %s\n", cmd);
    printf("Args:\n");
    for (int i = 0; i < argCount; i++)
    {
        printf("\t%d: %s\n", i, pArgs[i]);
    }
    printf("Dictionary:\n");
    StrDictListIterator* pDli = (StrDictListIterator*)dictListIter;
    KeyValuePair* pCur = pDli->GetNextEntry();
    while (pCur)
    {
        printf("\t%s: %s\n", pCur->key.c_str(), pCur->value.c_str());
        pCur = pDli->GetNextEntry();
    }
    printf("Threads: %d\n", threads);

    // TODO: fill in the param checking from the real callback
    //		 we could manually launch p4.exe to run the requested
    //		 operations and verify that it worked?

    // for now just return OK
    return 0;
}

// similar to above but with light callback testing
bool TestP4BridgeServer::TestParallelSyncCallback()
{
    bool rv = [] {

        ps = nullptr;

        ASSERT_TRUE(TestP4BridgeServer::CreateClient(testClient, testCfgDir));

        {
            // run p4 configure set net.parallel.max=4	
            P4BridgeServer* pServer = new P4BridgeServer("localhost:6666", "admin", "", testClient);

            const char* const args[] = { "set", "net.parallel.max=4" };
            ASSERT_INT_TRUE(pServer->run_command("configure", 0, 1, args, 2));

            // run p4 configure set dmc=3 (?)
            const char* const args1[] = { "set", "dmc=3" };
            ASSERT_INT_TRUE(pServer->run_command("configure", 0, 1, args1, 2));

#if defined(OS_LINUX) || defined(OS_MACOSX)
            // fix the digests of files (this depot was originally created in windows)
            const char* const args2[] = { "-v", "//depot/..." };
            ASSERT_INT_TRUE(pServer->run_command("verify", 0, 1, args2, 2));
#endif
            delete pServer;
        }

        // make a new connection to pick up the net.parallel.max value
        ps = new P4BridgeServer("localhost:6666", "admin", "", testClient);
        P4Connection* pCon = ps->getConnection(7);
        P4BridgeClient* ui = pCon->getUi();

        ps->SetParallelTransferCallbackFn(MyParallelSyncCallbackFn);

        // positive test here; in this phase it should
        //   - call p4 sync with --parallel set to something useful
        //   - receive a callback with some text message
        //   - the sync will actually fail (because we did not sync anything)
        const char* args[] = {
            "--parallel",
            "threads=4,batch=8,batchsize=1,min=1,minsize=1",
            "-f",
            "//..."
        };

        ASSERT_INT_TRUE(ps->run_command("sync", 7, true, (char**)args, sizeof(args) / sizeof(args[0])));
        // verify some of the files are here

        StrDictListIterator* tagged = ps->get_ui(7)->GetTaggedOutput();
        int itemCnt = 0;
        while (StrDictList* pItem = tagged->GetNextItem())
        {
            int entryCnt = 0;

            // Find each local filename from tagged output
            // and confirm that it exists
            while (KeyValuePair* pEntry = tagged->GetNextEntry())
            {
                struct stat buffer;
                if (strcmp(pEntry->key.c_str(), "clientFile") == 0)
                    ASSERT_INT_TRUE((stat(pEntry->value.c_str(), &buffer) == 0));
            }
            itemCnt++;
        }

        delete tagged;

        // negative tests
        // set a null pointer, which means...use p4.exe instead?
        ps->SetParallelTransferCallbackFn((ParallelTransferCallbackFn*)0x00);
        ASSERT_INT_TRUE(ps->run_command("sync", 7, true, (char**)args, sizeof(args) / sizeof(args[0])));

        // set an invalid pointer, which will fail but gracefully (requires SEH, so no longer tested)
        //pServer->SetParallelTransferCallbackFn((ParallelTransferCallbackFn *) 0xFFFFFFFF);
        //ASSERT_INT_FALSE(pServer->run_command("sync", 7, true, (char **) args, sizeof(args) / sizeof(args[0])));

    // now that Client's "errors" member has incremented, run sync again without parallel to make sure that 
    // we don't get an error for a successful run without disconnect
        const char* args2[] = { "-f", "//..." };

        ASSERT_INT_TRUE(ps->run_command("sync", 7, true, (char**)args2, sizeof(args2) / sizeof(args2[0])));
        return true;
    }();

    return rv;
}

bool TestP4BridgeServer::TestDefaultProgramNameAndVersion()
{
    P4ClientError* connectionError = nullptr;

    ASSERT_TRUE(TestP4BridgeServer::CreateClient(testClient, testCfgDir));

    // create a new server
    ps = new P4BridgeServer("localhost:6666", "admin", "", testClient);

    bool rv = [&] {
        ASSERT_NOT_NULL(ps);

        // connect and see if the api returned an error. 
        if (!CheckConnection(ps, connectionError))
            return false;

        const char* const params[] = { "//depot/MyCode/*" };

        //p4base::DumpMemoryState("Before first command run");

        ASSERT_INT_TRUE(ps->run_command("files", 6, 1, params, 1))

#ifdef OS_NT
            ASSERT_STRING_EQUAL(ps->pProgramName.c_str(), defaultApplication); // from .rc 
#endif
#if defined(OS_LINUX) || defined(OS_MACOSX)
        ASSERT_STRING_STARTS_WITH(ps->pProgramName.c_str(), "P4NET/");
#endif
        ASSERT_STRING_EQUAL(ps->pProgramVer.c_str(), defaultVersion);   // from .rc
//p4base::DumpMemoryState("After first command run");

// set both program name and version
        ps->pProgramName = testProgramName;
        ps->pProgramVer = testProgramVer;

        ASSERT_INT_TRUE(ps->run_command("files", 7, 1, params, 1))

            // should both be set to supplied strings
            ASSERT_STRING_EQUAL(ps->pProgramName.c_str(), testProgramName);
        ASSERT_STRING_EQUAL(ps->pProgramVer.c_str(), testProgramVer);
        //p4base::DumpMemoryState("After first command run");

        // set program name but not version
        ps->pProgramName = testProgramName;
        ps->pProgramVer.clear();

        ASSERT_INT_TRUE(ps->run_command("files", 8, 1, params, 1))

            // only name should  be set to supplied strings
            ASSERT_STRING_EQUAL(ps->pProgramName.c_str(), testProgramName);
        ASSERT_STRING_NOT_EQUAL(ps->pProgramVer.c_str(), testProgramVer);
        ASSERT_STRING_EQUAL(ps->pProgramVer.c_str(), defaultVersion);

        // set program version but not name
        ps->pProgramName.clear();
        ps->pProgramVer = testProgramVer;

        ASSERT_INT_TRUE(ps->run_command("files", 7, 1, params, 1))

            // only version should  be set to supplied strings
            ASSERT_STRING_NOT_EQUAL(ps->pProgramName.c_str(), testProgramName);
        ASSERT_STRING_EQUAL(ps->pProgramVer.c_str(), testProgramVer);
#ifdef OS_NT
        ASSERT_STRING_EQUAL(ps->pProgramName.c_str(), "BridgeUnit.exe");
#endif
        return true;
    }();
    return rv;
}

bool TestP4BridgeServer::TestGetTicketFile()
{
    bool rv = [] {
        string tktfile = P4BridgeServer::GetTicketFile();

        ASSERT_TRUE(tktfile.length() != 0);

        return true;
    }();
    return rv;
}

// Callback to provide password for "login" in TestUnicodeUserName()
void STDCALL ProvidePassword2(int cmdId, const char* message, char* response, int responseSize, int noEcho)
{
    strncpy(response, "pass1234", responseSize);
}

bool TestP4BridgeServer::TestSetTicketFile()
{
    ASSERT_TRUE(TestP4BridgeServer::CreateClient(testClient, testCfgDir));

    ps = new P4BridgeServer("localhost:6666", "admin", "", testClient);

    bool rv = [&] {
        ASSERT_NOT_NULL(ps);

        P4ClientError* connectionError = NULL;

        // connect and see if the api returned an error.
        if (!CheckConnection(ps, connectionError))
            return false;

        const char* params[] = { "set", "security=2" };
        ASSERT_INT_TRUE(ps->run_command("configure", 7, 0, params, 2));
        //string tktfile = ps->ticketFile;
        //ASSERT_TRUE(tktfile.length() != 0);
        //ASSERT_EQUAL(tktfile, testTicketFile);

        ps->SetPromptCallbackFn(ProvidePassword2);
        //char* params2[] = { "-P","pass1234" };
        ASSERT_INT_TRUE(ps->run_command("passwd", 7, 0, NULL, 0));

        delete ps;

        remove(testTicketFile);

        ps = new P4BridgeServer("localhost:6666", "admin", "pass1234", testClient);
        ps->set_ticketFile(testTicketFile);

        // connect and see if the api returned an error.
        if (!CheckConnection(ps, connectionError))
            return false;

        ps->SetPromptCallbackFn(ProvidePassword2);
        ASSERT_INT_TRUE(ps->run_command("login", 7, 0, NULL, 0));

        ASSERT_INT_TRUE(ps->run_command("depots", 7, 0, NULL, 0));

        std::filebuf fb;
        ASSERT_NOT_NULL(fb.open(testTicketFile, std::ios::in));

        return true;
    }();
    return rv;
}


bool FindInLog(std::string val)
{
    // scan through the server log looking for 'RpcRecvBuffer pizza = 333333'
    std::filebuf fb;
    fb.open(TestLog, std::ios::in);
    std::istream is(&fb);
    bool foundIt = false;
    char buff[4096];
    while (!is.eof())
    {
        is.getline(buff, sizeof(buff));
        if (val == buff)
        {
            return true;
        }
    }
    return false;
}

bool TestP4BridgeServer::TestSetProtocol()
{
    P4ClientError* connectionError = NULL;

    bool rv = [&] {
        // turn on rpc=3 on the server and reconnect
        {
            const char* args[] = { "set", "rpc=3" };
            P4BridgeServer* pServer = new P4BridgeServer("localhost:6666", "admin", "", testClient);
            ASSERT_TRUE(pServer->connected(&connectionError) == 1);
            ASSERT_INT_TRUE(pServer->run_command("configure", 0, true, args, 2));
            delete pServer;
        }

        // create a another connection
        ps = new P4BridgeServer("localhost:6666", "admin", "", testClient);
        ASSERT_NOT_NULL(ps);

        // must run SetProtocol before the connection is established
        // note that for the most part SetProtocol is already handled 
        // in our connection code, but this is for the case when
        // a parallel sync/transmit response has protocol data for
        // the new connections

        // note that the server needs to be started with rpc=3
        // set a bogus protocol
        ps->SetProtocol("pizza", "333333");
        // set an api number too
        ps->SetProtocol("api", "9090909");

        // connect and see if the api returned an error. 
        if (!CheckConnection(ps, connectionError))
            return false;

        ASSERT_INT_TRUE(ps->run_command("info", 0, true, NULL, 0));
        // pizza is only in the RpcRecvBuffer, the server will ignore it
        ASSERT_TRUE(FindInLog("RpcRecvBuffer pizza = 333333"));
        ASSERT_TRUE(FindInLog("RpcRecvBuffer api = 9090909"));

        // negative test: call SetProtocol too late (we're connected)
        ps->SetProtocol("calzone", "444444");
        ASSERT_INT_TRUE(ps->run_command("info", 0, true, NULL, 0));
        ASSERT_FALSE(FindInLog("RpcRecvBuffer calzone = 444444"));

        // positive test: reconnnect and check again
        ASSERT_INT_TRUE(ps->disconnect());
        ASSERT_TRUE(ps->connected(&connectionError) == 1);
        ASSERT_INT_TRUE(ps->run_command("info", 0, true, NULL, 0));
        ASSERT_TRUE(FindInLog("RpcRecvBuffer calzone = 444444"));

        return true;
    }();
    return rv;
}
