#pragma once

#include "UnitTestFrameWork.h"
#include "../p4bridge/P4BridgeServer.h"

class TestP4BridgeServer :
    public UnitTestSuite
{
public:
    TestP4BridgeServer(void);
    ~TestP4BridgeServer(void);

    DECLARE_TEST_SUITE(TestP4BridgeServer)

    bool Setup();

    bool TearDown(const char* testName);

    static bool CheckConnection(P4BridgeServer *ps, P4ClientError *connectionError );

    static bool ServerConnectionTest();
    static bool CreateClient(const char* name, const char* workspace_root);
    static bool TestUnicodeClientToNonUnicodeServer();
    static bool TestUnicodeUserName();
    static bool TestUntaggedCommand();
    static bool TestTaggedCommand();
    static bool TestTextOutCommand();
    static bool TestBinaryOutCommand();
    static bool TestErrorOutCommand();
    static bool TestGetSet();
	static bool TestEnviro();
	static bool TestConnectSetClient();
	static bool TestGetConfig();
    static bool TestSetCwd();
    static bool TestIsIgnored();
    static bool TestConnectionManager();
	static bool TestSetVars();
    static bool TestDefaultProgramNameAndVersion();
	static bool TestParallelSync();
	static bool TestParallelSyncCallback();
	static bool TestGetTicketFile();
	static bool TestSetProtocol();
	static bool TestSetTicketFile();

	static int STDCALL LogCallback(int level, const char *file, int line, const char *msg);
};

