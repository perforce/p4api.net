/*******************************************************************************

Copyright (c) 2010, Perforce Software, Inc.  All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1.  Redistributions of source code must retain the above copyright
	notice, this list of conditions and the following disclaimer.

2.  Redistributions in binary form must reproduce the above copyright
	notice, this list of conditions and the following disclaimer in the
	documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
ARE DISCLAIMED. IN NO EVENT SHALL PERFORCE SOFTWARE, INC. BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*******************************************************************************/

/*******************************************************************************
 * Name		: p4bridge-api.cc
 *
 * Author	: dbb
 *
 * Description	: A "Flat C" interface for the Perforce API. Used to provide 
 * 		  simple access for C#.NET using P/Invoke and dllimport.
 *
 ******************************************************************************/


#include <csignal>
#include <stdexcept>
#include <typeinfo>

using std::exception;

#include "stdafx.h"
#include "ticket.h"
#include "p4libs.h"
#include "signaler.h"
#include "P4BridgeServer.h"

#include "enviro.h"

#ifdef _DEBUG_VLD
#include <vld.h> 
#endif

#if defined OS_NT
# define CONSTRUCTOR
# define DESTRUCTOR
#elif defined(__GNUC__)
# define CONSTRUCTOR __attribute__ ((constructor))
# define DESTRUCTOR __attribute__ ((destructor))
#else
#error Non Gnu Compiler!
#endif

static bool dll_initialized = false;

// Initialize after bridge DLL load
CONSTRUCTOR void initializer()
{
	Error e;
	P4Libraries::Initialize(P4LIBRARIES_INIT_P4 | P4LIBRARIES_INIT_OPENSSL, &e);

	signal(SIGINT, SIG_DFL); // unset the default set by global signaler in C++ (which exits)
	signaler.Disable(); // disable the global signaler at runtime
	dll_initialized = true;
}

// Finalize before bridge DLL unload
DESTRUCTOR void destructor()
{
	Error e;
	P4Libraries::Shutdown(P4LIBRARIES_INIT_P4 | P4LIBRARIES_INIT_OPENSSL, &e);
		}

#if defined OS_NT
// attributes work ok on other OS's, but 
// windows has it's own traditional way...
BOOL APIENTRY DllMain(HMODULE hModule,
	DWORD  ul_reason_for_call,
	LPVOID lpReserved
)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
		initializer();
		break;
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
		break;
	case DLL_PROCESS_DETACH:
		destructor();
		break;
	}
	return TRUE;
}
#endif

// If there is a connection error, keep a copy so the client can fetch it later
P4ClientError * connectionError = nullptr;

extern "C"
{
    EXPORT void ClearConnectionError()
	{
        try
		{
			// free old error, if any.
			if (connectionError)
			{
				delete connectionError;
				connectionError = nullptr;
			}
		}
        catch(exception &e)
		{
            P4BridgeServer::ReportException(e,"ClearConnectionError");
		}
	}
}

/******************************************************************************
 * Helper function to (re)connect to the server and determine if it is Unicode 
 * enabled.
******************************************************************************/
int ServerConnect(P4BridgeServer* pServer)
{
	try
	{
		// free old error string, if any.
		ClearConnectionError();

		if( !pServer->connected( &connectionError ) )
		{
			// Abort if the connect did not succeed
			return 0;
		}

		return 1;
	}
	catch (exception& e)
	{
		P4BridgeServer::ReportException(e, "ServerConnect");
		return 0;
	}
}

/******************************************************************************
 * Helper function to (re)connect to the server and determine if it is Unicode 
 * enabled.
******************************************************************************/
int ServerConnectTrust(P4BridgeServer* pServer, char* trust_flag, char* fingerprint)
{
	try
	{
		ClearConnectionError();

		// Connect to the server and see if the api returns an error. 
		if( !pServer->connect_and_trust( &connectionError, trust_flag, fingerprint ) )
		{
			// Abort if the connect did not succeed
			return 0;
		}

		if ( pServer )
		{
			// Check if the server is Unicode enabled
			pServer->unicodeServer(  );
		}
		return 1;
	}
    catch (exception& e)
	{
        P4BridgeServer::ReportException(e, "ServerConnectTrust");
		return 0;
	}
}

/******************************************************************************
 * 'Flat' C interface for the dll. This interface will be imported into C# 
 *    using P/Invoke 
******************************************************************************/
extern "C" 
{
	EXPORT void SetLogFunction(
		LogCallbackFn *log_fn)
	{
		P4BridgeServer::SetLogCallFn(log_fn);
	}

	/**************************************************************************
	*
	* P4BridgeServer functions
	*
	*    These are the functions that use a P4BridgeServer* to access an object 
	*      created in the dll.
	*
	**************************************************************************/

	P4BridgeServer* Connect_Int(	const char *server, 
													const char *user, 
													const char *pass,
													const char *ws_client,
													LogCallbackFn *log_fn)
	{
		LogCallbackFn *orig = P4BridgeServer::SetLogCallFn(log_fn);

		//Connect to the server
		P4BridgeServer* pServer = new P4BridgeServer(   server, 
														user, 
														pass,
														ws_client);
		P4BridgeServer::SetLogCallFn(orig);
		if (ServerConnect( pServer ) )
		{
			return pServer;
		}

		delete pServer;
		return nullptr;
	}

	/**************************************************************************
	*
	*  Connect: Connect to the a Perforce server and create a new 
	*    P4BridgeServer to access the server. 
	*
	*   Returns: Pointer to the new P4BridgeServer, NULL if there is an error.
	*
	*  NOTE: Call CloseConnection() on the returned pointer to free the object
	*
	**************************************************************************/

	EXPORT P4BridgeServer* Connect(	const char *server,
													const char *user, 
													const char *pass,
													const char *ws_client,
													LogCallbackFn *log_fn)
	{
		LOG_ENTRY();
		LOG_DEBUG1(4, "dll_initialized: %d", dll_initialized);
		try
		{
			ClearConnectionError();
			return Connect_Int(	server, user, pass, ws_client, log_fn);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"Connect");
			return nullptr;
		}
	}

	P4BridgeServer* TrustedConnect_Int(	const char *server, 
															const char *user, 
															const char *pass,
															const char *ws_client,
															char *trust_flag,
															char *fingerprint,
															LogCallbackFn *log_fn)
	{
		//Connect to the server
		P4BridgeServer* pServer = new P4BridgeServer(   server, 
														user, 
														pass,
														ws_client);
		LogCallbackFn* orig = P4BridgeServer::SetLogCallFn(log_fn); 
		bool ok = ServerConnectTrust( pServer, trust_flag, fingerprint ) != 0;
		P4BridgeServer::SetLogCallFn(orig);

		if (ok)
		{
			return pServer;
		}
		delete pServer;
		return nullptr;
	}

	/**************************************************************************
	*
	*  TrustedConnect: Connect to the a Perforce server and create a new 
	*    P4BridgeServer to access the server, and establish (or reestablish)
	*	 a trust relationship based on the servers certificate fingerprint. 
	*
	*   Returns: Pointer to the new P4BridgeServer, NULL if there is an error.
	*
	*  NOTE: Call CloseConnection() on the returned pointer to free the object
	*
	**************************************************************************/

	EXPORT P4BridgeServer* TrustedConnect(	const char *server,
															const char *user, 
															const char *pass,
															const char *ws_client,
															char *trust_flag,
															char *fingerprint,
															LogCallbackFn *log_fn)
	{
		try
		{
			ClearConnectionError();
			return TrustedConnect_Int( server, user, pass, ws_client, trust_flag, fingerprint, log_fn);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"TrustedConnect");
			return nullptr;
		}
	}

	P4BridgeServer* _connection_from_path(const char* cwd)
	{
		// create an un-connected p4bridgeserver based on a path
		// this is handy if you just want to get the connection info 
		// from a directory path using P4CONFIG or environment variables
		Enviro t_enviro;

		// Clone the BridgeEnviron to create a temporary enviro to work with.
		//Enviro t_enviro(*P4BridgeServer::GetEnviro());
		// The copy constructor crashes on windows, so a work-around follows

		char* p4config = P4BridgeServer::GetEnviro()->Get("P4CONFIG");
		if (p4config != NULL)
		{
			t_enviro.Update("P4CONFIG", p4config);
	}

		StrBuf t_cwd = cwd;
		t_enviro.Config(t_cwd);
		char *t_port = t_enviro.Get("P4PORT");		
		char* t_user = t_enviro.Get("P4USER");		
		char* t_client = t_enviro.Get("P4CLIENT");
		char* t_pass = t_enviro.Get("P4PASSWORD");

		return new P4BridgeServer(t_port, t_user, t_pass, t_client);
    }

    EXPORT P4BridgeServer* ConnectionFromPath(const char * cwd)
	{
        try
		{
			return _connection_from_path(cwd);
		}
        catch (exception& e)
		{
            P4BridgeServer::ReportException(e,"ConnectionFromPath");
            return nullptr;
		}
	}
	/**************************************************************************
	*
	*  GetConnectionError: Returns the text of a the error message 
	*   generated by the connection attempt, if any.
	*
	**************************************************************************/

	EXPORT P4ClientError * GetConnectionError( void )
		{
			return connectionError;
		}

	/**************************************************************************
	*
	*  CloseConnection: Closes the connection and deletes the P4BridgeServer 
	*		object.
	*
	*    pServer: Pointer to the P4BridgeServer 
	*
	**************************************************************************/
	EXPORT int ReleaseConnection( P4BridgeServer* pServer )
	{
		LOG_ENTRY();
		if (!pServer) 
		{
			return 1;
		}
		try
		{
			// if the handle is invalid or freeing it causes an exception, 
			// just consider it closed so return success
			if (!VALIDATE_HANDLE(pServer, tP4BridgeServer))
			{
					delete pServer;
				return 1;
			}
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"ReleaseConnection");
			return 1;
		}

		try
		{
			pServer->SetTaggedOutputCallbackFn(nullptr);
			pServer->SetErrorCallbackFn(nullptr);
			pServer->SetInfoResultsCallbackFn(nullptr);
			pServer->SetTextResultsCallbackFn(nullptr);
			pServer->SetBinaryResultsCallbackFn(nullptr);
			pServer->SetPromptCallbackFn(nullptr);
			pServer->SetParallelTransferCallbackFn(nullptr);
			pServer->SetResolveCallbackFn(nullptr);
			pServer->SetResolveACallbackFn(nullptr);

			LOG_LOC();
			int ret = pServer->close_connection();
			delete pServer;
			return ret;
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"ReleaseConnection1");
			return 0;
		}
	}

	/**************************************************************************
	*
	*  Disconnect: Disconnect from the server after running one or more 
	*	commands.
	*
	*    pServer: Pointer to the P4BridgeServer 
	*
	**************************************************************************/
	EXPORT int Disconnect( P4BridgeServer* pServer )
	{
		try
		{
			VALIDATE_HANDLE_B(pServer, tP4BridgeServer)
			return pServer->disconnect();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"Disconnect");
			return 0;
		}
	}

	/**************************************************************************
	*
	*  IsUnicode: Check if the server supports Unicode 
	*
	*  Note: Is set during connection so is valid immediately after Connect()
	*    successfully completes.
	*
	**************************************************************************/

	EXPORT int IsUnicode( P4BridgeServer* pServer )
	{
		try
		{
			VALIDATE_HANDLE_B(pServer, tP4BridgeServer)
			return pServer->unicodeServer();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"IsUnicode");
			return 0;
		}
	}

	/**************************************************************************
	*
	*  APILevel: Get the API level supported by the server 
	*
	*  Note: Is set during connection so is valid immediately after Connect()
	*    successfully completes.
	*
	**************************************************************************/

	EXPORT int APILevel( P4BridgeServer* pServer )
	{
		try
		{
			VALIDATE_HANDLE_B(pServer, tP4BridgeServer)
			return pServer->APILevel();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"APILevel");
			return 0;
		}
	}

	/**************************************************************************
	*
	*  UseLogin: Check if the server requires the Login command be used 
	*
	*  Note: Is set during connection so is valid immediately after Connect()
	*    successfully completes.
	*
	**************************************************************************/

	EXPORT int UseLogin( P4BridgeServer* pServer )
	{
		try
		{
			VALIDATE_HANDLE_B(pServer, tP4BridgeServer)
			return pServer->UseLogin();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"UseLogin");
			return 0;
		}
	}

	/**************************************************************************
	*
	*  SupportsExtSubmit: Check if the server support extended submit options 
	*   (2006.2 higher)?  
	*
	*  Note: Is set during connection so is valid immediately after Connect()
	*    successfully completes.
	*
	**************************************************************************/

	EXPORT int SupportsExtSubmit( P4BridgeServer* pServer )
	{
		try
		{
			VALIDATE_HANDLE_B(pServer, tP4BridgeServer)
			return pServer->SupportsExtSubmit();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"SupportsExtSubmit");
			return 0;
		}
	}

	/**************************************************************************
	*
	*  UrlLaunched: Check if the ClientUser::HandleUrl method has been
	*   called
	*
	*  Note: P4BridgeClient::HandleUrl is the override that sets a bool
	*   and calls ClientUser::HandleUrl.
	*
	**************************************************************************/
	
	EXPORT int UrlLaunched()
	{
		return handleUrl;
	}

	/**************************************************************************
	*
	*  IsUnicode: Check if the server supports Unicode 
	*
	*    pServer:      Pointer to the P4BridgeServer 
	*
	*    pCharSet:     String name for the character set to use for command 
	*                    data passed to/from the server.
	*
	*    pFileCharSet: String name for the character set to use for the 
	*                    contents of type Unicode file when stored in the 
	*                    a file on the client's disk.
	*
	*  Note: Needs to be called before any command which takes parameters is 
	*    called.
	*
	**************************************************************************/
	
	const char* _SetCharacterSet(P4BridgeServer* pServer,
		const char * pCharSet,
                                    const char * pFileCharSet ) {
		return Utils::AllocString(pServer->set_charset(pCharSet, pFileCharSet));
	}

    EXPORT const char * SetCharacterSet( P4BridgeServer* pServer,
													const char * pCharSet, 
													const char * pFileCharSet )
		try
	{
			VALIDATE_HANDLE_P(pServer, tP4BridgeServer)
 			return _SetCharacterSet(pServer, pCharSet, pFileCharSet);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"SetCharacterSet");
			return nullptr;
		}
	}

	/**************************************************************************
	*
	*  set_connection: Set the connection parameters.
	*
	*    pServer: Pointer to the P4BridgeServer
	*
	*    newPort:		New port
	*    newUser:		New workspace
	*    newPassword:	New password
	*    newClient:		New workspace
	*
	*  Return: None
	**************************************************************************/

	EXPORT void set_connection( P4BridgeServer* pServer,
		const char* newPort,
		const char* newUser,
		const char* newPassword,
		const char* newClient)
	{
		try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
			return pServer->set_connection(newPort, newUser, newPassword, newClient);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"set_connection");
		}
	}

	/**************************************************************************
	*
	*  set_client: Set the client workspace.
	*
	*    pServer: Pointer to the P4BridgeServer
	*
	*    pNew: New workspace
	*
	*  Return: None
	**************************************************************************/

	EXPORT void set_client( P4BridgeServer* pServer, const char* workspace )
	{
		try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
			return pServer->set_client(workspace);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"set_client");
		}
	}

	/**************************************************************************
	*
	*  get_client: Get the name of the current client workspace.
	*
	*    pServer: Pointer to the P4BridgeServer
	*
	*  Return: Pointer to access the data.
	*
	**************************************************************************/

	const char* _get_client(P4BridgeServer* pServer)
	{
		return Utils::AllocString(pServer->get_client());
	}

	EXPORT const char * get_client( P4BridgeServer* pServer )
	{
		try
		{
			VALIDATE_HANDLE_P(pServer, tP4BridgeServer);
			return _get_client(pServer);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"get_client");
			return nullptr;
		}
	}

	/**************************************************************************
	*
	*  get_user: Get the user name for the current connection.
	*
	*    pServer: Pointer to the P4BridgeServer
	*
	*  Return: Pointer access the data.
	*
	**************************************************************************/

	const char* _get_user(P4BridgeServer* pServer)
	{
		return Utils::AllocString(pServer->get_user());
	}

	EXPORT const char * get_user( P4BridgeServer* pServer )
	{
		try
		{
			VALIDATE_HANDLE_P(pServer, tP4BridgeServer);
			return Utils::AllocString(pServer->get_user());
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"get_user");
			return nullptr;
		}
	}

	/**************************************************************************
	*
	*  set_user: Set the user name for the connection.
	*
	*    pServer: Pointer to the P4BridgeServer
	*    newValue: The new value
	*
	*  Return: Pointer access the data.
	*
	**************************************************************************/

	EXPORT void set_user( P4BridgeServer* pServer, char * newValue )
	{
		try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
				pServer->set_user(newValue);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"set_user");
		}
	}

	/**************************************************************************
	*
	*  get_port: Get the port for the current connection.
	*
	*    pServer: Pointer to the P4BridgeServer
	*
	*  Return: Pointer to access the data.
	*
	**************************************************************************/

	const char* _get_port(P4BridgeServer* pServer)
	{
		return Utils::AllocString(pServer->get_port());
	}

    EXPORT const char * get_port( P4BridgeServer* pServer )
	{
		try
		{
			VALIDATE_HANDLE_P(pServer, tP4BridgeServer);
			return _get_port(pServer);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"get_port");
			return nullptr;
		}
	}

	/**************************************************************************
	*
	*  set_port: Set the port for the connection.
	*
	*    pServer: Pointer to the P4BridgeServer
	*    newValue: The new value
	*
	*  Return: Pointer to access the data.
	*
	**************************************************************************/


	EXPORT void set_port( P4BridgeServer* pServer, char * newValue )

	{
		try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
				pServer->set_port(newValue);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"set_port");
		}
	}

	/**************************************************************************
	*
	*  get_password: Get the password for the current connection.
	*
	*    pServer: Pointer to the P4BridgeServer
	*
	*  Return: Pointer to access the data.
	*
	**************************************************************************/

	const char* _get_password(P4BridgeServer* pServer)
	{
		return Utils::AllocString(pServer->get_password());
	}

	EXPORT const char * get_password( P4BridgeServer* pServer )
	{
		try
		{
			VALIDATE_HANDLE_P(pServer, tP4BridgeServer);
			return _get_password(pServer);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"get_password");
			return nullptr;
		}
	}

	/**************************************************************************
	*
	*  set_password: Set the password for the connection.
	*
	*    pServer: Pointer to the P4BridgeServer
	*    newValue: The new value
	*
	*  Return: Pointer to access the data.
	*
	**************************************************************************/

	EXPORT void set_password( P4BridgeServer* pServer, char * newValue )
	{
		try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
				pServer->set_password(newValue);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"set_password");
		}
	}

	/**************************************************************************
    *
    *  set_ticketFile: Set the ticket file.
	*
	*    pServer: Pointer to the P4BridgeServer
	*
	*    ticketFile: New ticket file
	*
	*  Return: None
	**************************************************************************/

    EXPORT void set_ticketFile(P4BridgeServer* pServer, const char* ticketFile)
	{
        try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
				return pServer->set_ticketFile(ticketFile);
		}
        catch (exception& e)
		{
            P4BridgeServer::ReportException(e,"set_ticketFile");
		}
	}

	/**************************************************************************
	*
	*  get_ticketFile: Get the name of the current ticket file.
	*
	*    pServer: Pointer to the P4BridgeServer
	*
	*  Return: Pointer to access the data.
	*
	**************************************************************************/

	const char* _get_TicketFile(P4BridgeServer* pServer)
	{
		return Utils::AllocString(pServer->get_ticketFile());
	}

    EXPORT const char * get_ticketFile(P4BridgeServer* pServer)
	{
            try
		{
			VALIDATE_HANDLE_P(pServer, tP4BridgeServer);
			return _get_TicketFile(pServer);
		}
            catch (exception& e)
		{
                P4BridgeServer::ReportException(e,"get_ticketFile");
                return nullptr;
		}
	}

	/**************************************************************************
*
*  get_ticket: Get the ticket for the current connection, if any.
*
*    pServer: Pointer to the P4BridgeServer
*
*  Return: Pointer to access the data.
*
**************************************************************************/

	const char* _get_ticket(char* path, char* port, char* user)
	{
		LOG_ENTRY();
		StrPtr pathStr = StrRef(path);
		Ticket ticket(&pathStr);
		StrPtr portStr = StrRef(port);
		StrPtr userStr = StrRef(user);

		return Utils::AllocString(ticket.GetTicket(portStr, userStr));
	}

	EXPORT const char * get_ticket(char* path, char* port, char* user)
	{
		LOG_ENTRY();
		try
		{
			return _get_ticket(path, port, user);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"get_ticketFile");
			return nullptr;
		}
	}

	/**************************************************************************
	*
	*  GetCwd: Gets the current working directory for the P4BridgeServer.
	*
	*    pServer: Pointer to the P4BridgeServer
	*
	**************************************************************************/

	const char* get_cwd_int(P4BridgeServer* pServer)
	{
		return Utils::AllocString(pServer->get_cwd());
	}

	EXPORT const char * get_cwd( P4BridgeServer* pServer )
		{
		try
		{
			VALIDATE_HANDLE_P(pServer, tP4BridgeServer)
			return get_cwd_int(pServer);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"get_cwd");
			return nullptr;
		}
	}

	/**************************************************************************
	*
	*  SetCwd: Sets the current working directory for the P4BridgeServer.
	*
	*    pServer: Pointer to the P4BridgeServer
	*
	*    new_val: Path to the new current working directory
	*
	**************************************************************************/

	EXPORT void set_cwd( P4BridgeServer* pServer,
		const char * new_val)
	{
	    try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
			pServer->set_cwd((const char *)new_val);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"set_cwd");
		}
	}

	/**************************************************************************
	*
	*  get_programName: Get the program name to use for the connection.
	*
	*    pServer: Pointer to the P4BridgeServer
	*
	*  Return: Pointer to access the data.
	*
	**************************************************************************/

	const char* _get_programName(P4BridgeServer* pServer)
	{
		return Utils::AllocString(pServer->get_programName());
	}

	EXPORT const char * get_programName( P4BridgeServer* pServer )
	{
		try
		{
			VALIDATE_HANDLE_P(pServer, tP4BridgeServer)
			return _get_programName(pServer);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"get_programName");
			return nullptr;
		}
	}

	/**************************************************************************
	*
	*  set_programName: Set the program name to use for the connection.
	*
	*    pServer: Pointer to the P4BridgeServer
	*    newValue: The new value
	*
	*  Return: Pointer to access the data.
	*
	**************************************************************************/

	EXPORT void set_programName( P4BridgeServer* pServer, char * newValue )
	{
		try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
				pServer->set_programName(newValue);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"set_programName");
		}
	}

	/**************************************************************************
	*
	*  get_programVer: Get the program version to use for the connection.
	*
	*    pServer: Pointer to the P4BridgeServer
	*
	*  Return: Pointer to access the data.
	*
	**************************************************************************/

	const char * _get_programVer(P4BridgeServer* pServer)
	{
		return Utils::AllocString(pServer->get_programVer());
	}

    EXPORT const char * get_programVer( P4BridgeServer* pServer )
	{
		try
		{
			VALIDATE_HANDLE_P(pServer, tP4BridgeServer)
			return _get_programVer(pServer);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"get_programVer");
			return nullptr;
		}
	}

	/**************************************************************************
	*
	*  set_programVer: Set the program version to use for the connection.
	*
	*    pServer: Pointer to the P4BridgeServer 
	*    newValue: The new value
	*    
	*  Return: Pointer to access the data.
	*
	**************************************************************************/

	EXPORT void set_programVer( P4BridgeServer* pServer, char * newValue )
	{
		try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
			pServer->set_programVer(newValue);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"set_programVer");
		}
	}

	/**************************************************************************
	*
	*  set_debugLevel: Set debug options like "rpc=3"
	*
	*    level: The level value as a null terminated string.
	*	 debug information goes to stdout
	*
	**************************************************************************/

	EXPORT void set_debugLevel(const char* level)
	{
		P4BridgeServer::SetDebugLevel(level);
	}

	/**************************************************************************
	*
	*  set_debugLevelFile: Set debug options like "rpc=3" with output to file
	*
	*    level: The level value as a null terminated string.
	*	 filename: debug information is output to this file
	*
	**************************************************************************/

	EXPORT void set_debugLevelFile(const char* level, const char *filename)
	{
		P4BridgeServer::SetDebugLevel(level, filename);
	}

	/**************************************************************************
	*
	*  get_charset: Get the character to use for the connection.
	*
	*    pServer: Pointer to the P4BridgeServer 
	*    
	*  Return: Pointer to access the data.
	*
	**************************************************************************/

	const char * _get_charset(P4BridgeServer* pServer)
	{
		return Utils::AllocString(pServer->get_charset());
	}

    EXPORT const char * get_charset( P4BridgeServer* pServer )
	{
		try
		{
			VALIDATE_HANDLE_P(pServer, tP4BridgeServer)
			return _get_charset(pServer);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"get_charset");
			return nullptr;
		}
	}

	/**************************************************************************
	*
	*  get_config: Get the config file for the current connection, if any.
	*
	*    pServer: Pointer to the P4BridgeServer 
	*    
	*  Return: Pointer to access the data.
	*
	**************************************************************************/
	const char * _get_config(P4BridgeServer* pServer)
	{
		return Utils::AllocString(pServer->get_config());
	}

	EXPORT const char * get_config( P4BridgeServer* pServer )
	{
		try
		{
			VALIDATE_HANDLE_P(pServer, tP4BridgeServer)
			return _get_config(pServer);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"get_config");
			return nullptr;
		}
	}

	const char* _get_config_cwd(char* cwd)
	{
		LOG_ENTRY();
		return Utils::AllocString(P4BridgeServer::get_config(cwd));
	}

    EXPORT const char * get_config_cwd( char* cwd )
	{
		try
		{
			return _get_config_cwd(cwd);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"get_config_cwd");
			return nullptr;
		}
	}

	const char* _Get(const char *var)
	{
		return Utils::AllocString(P4BridgeServer::Get(var));
	}

    EXPORT const char * Get( const char *var )
	{
		try
		{
			return _Get( var );
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"Get");
			return nullptr;
		}
	}

	EXPORT void Set( const char *var, const char *val )
	{
		try
		{
			return P4BridgeServer::Set( var, val );
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"Set");
		}
	}

	EXPORT void Update( const char *var, const char *val )
	{
		try
		{
			return P4BridgeServer::Update( var, val );
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"Update");
		}
	}

	EXPORT void ReloadEnviro()
	{
		try
		{
			return P4BridgeServer::Reload();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"Reload");
		}
	}

	EXPORT void ListEnviro()
	{
		try
		{
			return P4BridgeServer::ListEnviro();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e, "ListEnviro");
		}
	}


	/**************************************************************************
	*
	*  GetTicketFile: Get the path to the file where user tickets are stored.
	*
	*  Return: Path to the ticket file, NULL if not known or error
	**************************************************************************/
	const char* _GetTicketFile()
	{
		return Utils::AllocString(P4BridgeServer::GetTicketFile());
	}

    EXPORT const char * GetTicketFile(  )
	{
		try
		{
			return _GetTicketFile();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"GetTicketFile");
			return nullptr;
		}
	}

	/**************************************************************************
	*
	*  GetExistingTicket: Get the existing ticket for a connection, if any.
	*
	*  Return: The ticket, NULL if no ticket in file or error
	**************************************************************************/
	const char* _GetTicket(char* port, char* user)
	{
		return Utils::AllocString(P4BridgeServer::GetTicket(port, user));
	}

	EXPORT const char * GetTicket( char* port, char* user )
	{
		try
		{
			return _GetTicket(port, user);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"GetTicket");
			return nullptr;
		}
	}

	/*
    *     Raw SetProtocol - must be called on a disconnected pServer to be effective, or on a pServer that you reconnect on
	*/
    EXPORT void SetProtocol(P4BridgeServer* pServer, const char* key, const char* val)
	{
        try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer);
			pServer->SetProtocol(key, val);
		}
        catch (exception& e)
		{
            P4BridgeServer::ReportException(e,"SetProtocol");
		}
	}

	/**************************************************************************
	*
	*  RunCommand: Run a command using the P4BridgeServer.
	*
	*    pServer: Pointer to the P4BridgeServer 
	*
	*    cmd: Command name, i.e 'fstst'. These are always in ASCII, regardless
	*           of whether or not the server is Unicode enabled.
	*
	*    tagged: If non zero, run the command using tagged protocol 
	*
	*    args: list of arguments. For non Unicode servers, these are ASCII
	*            encode strings. For Unicode servers they should be encoded in
	*            using the encoding specified in a previous call to 
	*            SetCharacterSet().
	*
	*    argc: count of arguments
	*
	*  Return: Zero if there was an error running the command
	**************************************************************************/

	EXPORT int RunCommand( P4BridgeServer* pServer,
										  const char *cmd, 
										  int cmdId,
										  int tagged, 
										  char * const *args,
										  int argc )
	{
		try
		{
			VALIDATE_HANDLE_I(pServer, tP4BridgeServer)
			// make sure we're connected to the server
			if (0 == ServerConnect( pServer ))
			{
				return 0;
			}
			return pServer->run_command(cmd, cmdId, tagged, args, argc);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"RunCommand");
			return 0;
		}
	}

	/**************************************************************************
	*
	*  CancelCommand: Cancel a running command
	*
	*  Return: None
	**************************************************************************/

	EXPORT void CancelCommand( P4BridgeServer* pServer, int cmdId )
	{
		try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
			pServer->cancel_command(cmdId);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"CancelCommand");
		}
	}

	EXPORT int IsConnected(P4BridgeServer* pServer)
	{
		try
		{
			VALIDATE_HANDLE_I(pServer, tP4BridgeServer)
			return pServer->IsConnected();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"IsConnected");
			return 0;
		}
	}

	/**************************************************************************
	*
	*  SetTaggedOutputCallbackFn: Set the tagged output callback fn.
	*
	*    pServer: Pointer to the P4BridgeServer 
	*
	*    pNew: New function pointer 
	*    
	*  Return: None
	**************************************************************************/

	EXPORT void SetTaggedOutputCallbackFn( P4BridgeServer* pServer, IntTextTextCallbackFn* pNew )
	{
		try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
			pServer->SetTaggedOutputCallbackFn(pNew);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"SetTaggedOutputCallbackFn");
		}
	}

	/**************************************************************************
	*
	*  GetTaggedOutputCount: Get a count of the number of entries in the tagged 
	*							output.
	*
	*    pServer: Pointer to the P4BridgeServer 
	*    
	*  Return: count of the number of entries in the tagged output..
	*
	*  NOTE: Call Release() on the returned pointer to free the object
	*
	**************************************************************************/

	EXPORT int GetTaggedOutputCount( P4BridgeServer* pServer, int cmdId )
	{
		try
		{
			VALIDATE_HANDLE_I(pServer, tP4BridgeServer);
			P4BridgeClient* pUi = pServer->find_ui(cmdId);
			if (!pUi)
				return  -1;
			return pUi->GetTaggedOutputCount();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"GetTaggedOutputCount");
			return -1;
		}
	}

	/**************************************************************************
	*
	*  GetTaggedOutput: Get a StrDictListIterator to iterate through
	*                            the tagged output.
	*
	*    pServer: Pointer to the P4BridgeServer 
	*    
	*  Return: Pointer to a new StrDictListIterator to access the data.
	*
	*  NOTE: Call Release() on the returned pointer to free the object
	*
	**************************************************************************/

	EXPORT StrDictListIterator * GetTaggedOutput( P4BridgeServer* pServer, int cmdId )
	{
		try
		{
			VALIDATE_HANDLE_P(pServer, tP4BridgeServer)
			P4BridgeClient* pUi = pServer->find_ui(cmdId);
			if (!pUi)
				return  nullptr;
			return pUi->GetTaggedOutput();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"StrDictListIterator");
			return(nullptr);
		}
	}

	/**************************************************************************
	*
	*  SetErrorCallbackFn: Set the error output callback fn.
	*
	*    pServer: Pointer to the P4BridgeServer 
	*
	*    pNew: New function pointer 
	*    
	*  Return: None
	**************************************************************************/

	EXPORT void SetErrorCallbackFn( P4BridgeServer* pServer, IntIntIntTextCallbackFn* pNew )
	{
		try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
			pServer->SetErrorCallbackFn(pNew);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"SetErrorCallback");
		}
	}

	/**************************************************************************
	*
	*  GetErrorResults: Get the error output.
	*
	*    pServer: Pointer to the P4BridgeServer 
	*    
	*  Return: Pointer to the data.
	*
	**************************************************************************/

	EXPORT P4ClientError * GetErrorResults( P4BridgeServer * pServer, int cmdId)
	{
		try
		{
			VALIDATE_HANDLE_P(pServer, tP4BridgeServer)
			P4BridgeClient* pUi = pServer->find_ui(cmdId);
			if (!pUi)
				return  nullptr;
			return pUi->GetErrorResults();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"GetErrorResults");
			return nullptr;
		}
	}

	/**************************************************************************
	*
	*  SetInfoResultsCallbackFn: Set the info output callback fn.
	*
	*    pServer: Pointer to the P4BridgeServer 
	*
	*    pNew: New function pointer 
	*    
	*  Return: None
	**************************************************************************/

	EXPORT void SetInfoResultsCallbackFn( P4BridgeServer* pServer, IntIntIntTextCallbackFn* pNew )
	{
		try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
			pServer->SetInfoResultsCallbackFn(pNew);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"SetInfoResultsCallbackFn");
		}
	}

	/**************************************************************************
	*
	*  GetInfoResultsCount: Get the count of the number of the info output.
	*
	*    pServer: Pointer to the P4BridgeServer 
	*    
	*  Return: Count of number of entries in the info out.
	*
	**************************************************************************/

	EXPORT int GetInfoResultsCount( P4BridgeServer* pServer, int cmdId)
	{
		try
		{
			VALIDATE_HANDLE_B(pServer, tP4BridgeServer)
			P4BridgeClient* pUi = pServer->find_ui(cmdId);
			if (!pUi)
				return  -1;
			return pUi->GetInfoResultsCount();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"GetInfoResultsCount");
			return -1;
		}
	}

	/**************************************************************************
	*
	*  GetInfoResults: Get the info output.
	*
	*    pServer: Pointer to the P4BridgeServer 
	*    
	*  Return: Pointer to access the data.
	*
	**************************************************************************/

	EXPORT P4ClientInfoMsg * GetInfoResults( P4BridgeServer* pServer, int cmdId)
	{
		try
		{
			VALIDATE_HANDLE_P(pServer, tP4BridgeServer)
			P4BridgeClient* pUi = pServer->find_ui(cmdId);
			if (!pUi)
				return  nullptr;
			if (!pUi->GetInfoResults())
				return  nullptr;
			return pUi->GetInfoResults();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"GetInfoResults");
			return nullptr;
		}
	}

	/**************************************************************************
	*
	*  SetTextResultsCallbackFn: Set the text output callback fn.
	*
	*    pServer: Pointer to the P4BridgeServer 
	*
	*    pNew: New function pointer 
	*    
	*  Return: None
	**************************************************************************/

	EXPORT void SetTextResultsCallbackFn( P4BridgeServer* pServer, TextCallbackFn* pNew )
	{
		try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
			pServer->SetTextResultsCallbackFn(pNew);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"SetTextResultsCallbackFn");
		}
	}

	/**************************************************************************
	*
	*  GetTextResults: Get the text output.
	*
	*    pServer: Pointer to the P4BridgeServer 
	*    
	*  Return: Pointer to the data.
	*
	**************************************************************************/

	EXPORT const char * GetTextResults( P4BridgeServer* pServer, int cmdId )
	{
		try
		{
			VALIDATE_HANDLE_P(pServer, tP4BridgeServer)
			P4BridgeClient* pUi = pServer->find_ui(cmdId);
			if (!pUi)
				return  nullptr;
			if (!pUi->GetTextResults())
				return  nullptr;
			return pUi->GetTextResults();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"GetTextResults");
			return nullptr;
		}
	}

	/**************************************************************************
	*
	*  SetBinaryResultsCallbackFn: Set the callback for binary output.
	*
	*    pServer: Pointer to the P4BridgeServer 
	*
	*    pNew: The new callback function pointer
	*    
	*  Return: None
	**************************************************************************/

	EXPORT void SetBinaryResultsCallbackFn( P4BridgeServer* pServer, BinaryCallbackFn* pNew )
	{
		try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
			pServer->SetBinaryResultsCallbackFn(pNew);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"SetBinaryResultsCallbackFn");
		}
	}

	/**************************************************************************
	*
	*  GetBinaryResultsCount: Get the size in bytes of the binary output.
	*
	*    pServer: Pointer to the P4BridgeServer 
	*    
	*  Return: Byte count for the data.
	*
	**************************************************************************/

	EXPORT size_t GetBinaryResultsCount(  P4BridgeServer* pServer, int cmdId)
	{ 
		try
		{
			VALIDATE_HANDLE_I(pServer, tP4BridgeServer)
			P4BridgeClient* pUi = pServer->find_ui(cmdId);
			if (!pUi)
				return  0;
			return pUi->GetBinaryResultsCount( );
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"GetBinaryResultsCount");
			return 0;
		}
	}

	/**************************************************************************
	*
	*  GetBinaryResults: Get the binary output.
	*
	*    pServer: Pointer to the P4BridgeServer 
	*    
	*  Return: Pointer to the data.
	*
	**************************************************************************/

	EXPORT const unsigned char * GetBinaryResults( P4BridgeServer* pServer, int cmdId )
	{
		try
		{
			VALIDATE_HANDLE_P(pServer, tP4BridgeServer)
			P4BridgeClient* pUi = pServer->find_ui(cmdId);
			if (!pUi)
				return  NULL;
			return pUi->GetBinaryResults( );
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"GetBinaryResults");
			return nullptr;
		}
	}

	/**************************************************************************
	*
	*  SetDataSet: Set the Data Set in the UI (P4Client).
	*
	*    pServer: Pointer to the P4BridgeServer 
	*
	*    data: String Pointer to the data
	*    
	*  Return: Pointer to char * for the data.
	*
	**************************************************************************/

	EXPORT void SetDataSet( P4BridgeServer* pServer, int cmdId,
										   const char * data )
	{
		try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
			P4BridgeClient* pUi = pServer->get_ui(cmdId);
			if (!pUi)
				return;
			return pUi->SetDataSet(data);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"SetDataSet");
		}
	}

	/**************************************************************************
	*
	*  GetDataSet: Get the Data Set in the UI (P4Client).
	*
	*    pServer: Pointer to the P4BridgeServer 
	*    
	*  Return: Pointer to a new char * to access the data.
	*
	**************************************************************************/

	EXPORT  char* GetDataSet(P4BridgeServer* pServer, int cmdId)
	{
		try
		{
			VALIDATE_HANDLE_P(pServer, tP4BridgeServer)
			P4BridgeClient* pUi = pServer->find_ui(cmdId);
			if (!pUi)
				return  nullptr;
			return pUi->GetDataSet()->Text();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e, "GetDataSet");
			return nullptr;
		}
	}

	/**************************************************************************
	*
	*  SetPromptCallbackFn: Set the callback for replying to a server prompt.
	*
	*    pServer: Pointer to the P4BridgeServer 
	*
	*    pNew: The new callback function pointer
	*    
	*  Return: None
	**************************************************************************/

	EXPORT void SetPromptCallbackFn( P4BridgeServer* pServer,
													PromptCallbackFn* pNew )
	{
		try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
			pServer->SetPromptCallbackFn(pNew);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"SetPromptCallbackFn");
		}
	}
	/**************************************************************************
	*
	*  SetParallelTransferCallbackFn: Set the callback for replying to a server prompt.
	*
	*    pServer: Pointer to the P4BridgeServer
	*
	*    pNew: The new callback function pointer
	*
	*  Return: None
	**************************************************************************/

	EXPORT void SetParallelTransferCallbackFn(P4BridgeServer* pServer,
		ParallelTransferCallbackFn* pNew)
	{
		try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
			pServer->SetParallelTransferCallbackFn(pNew);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"SetParallelTransferCallbackFn");
		}
	}
	/**************************************************************************
	*
	*  IsIgnored: Test to see if a particular file is ignored.
	*
	*    pServer: Pointer to the P4BridgeServer 
	*
	*    pNew: local path of the file
	*    
	*  Return: non zero if file is ignored
	**************************************************************************/

	EXPORT int IsIgnored( const char *pPath )
	{
		try
		{
			StrPtr Str = StrRef(pPath);
			return P4BridgeServer::IsIgnored(Str);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"IsIgnored");
		}
		return 0;
	}

	/**************************************************************************
	*   class StrDictListIterator
	* 
	* A StrDictList is a list of items defined by StrDictionaries. Each
	*   StrDictionary can be considered a collection of entries, key:value 
	*   pairs of string data. StrDictListIterator allows you to walk this
	*   list of lists.
	*
	* itemList---->item1----->item2-....->itemN
	*              ->entry1   ->entry1    ->entry1
	*              ->entry2   ->entry2    ->entry2
	*                ...        ...         ...              
	*              ->entryX   ->entryY    ->entryZ
	*
	* Basic Usage:
	*   StrDictListIterator * pItem;
	*   while (pItem = pIterator-GetNextItem()
	*   {
	*       KeyValuePair * = pEntry;
	*       while (pEntry = pIterator-GetNextEntry()
	*          // do something with the key:value pair, pEntry
	*   }
	*
	*  NOTE: The iterate as currently implemented, can only iterate through the
	*    data once, as there is no method to reset it.
	*
	**************************************************************************/

	/**************************************************************************
	*
	*  GetNextEntry: Get the next Item in the list. Returns the first item
	*      on the first call.
	*
	*    pServer: Pointer to the P4BridgeServer 
	*    
	*  Return: Pointer to access the data.
	*
	**************************************************************************/

	EXPORT StrDictList* GetNextItem( StrDictListIterator* pObj )
	{
		try
		{
			VALIDATE_HANDLE_P(pObj, tStrDictListIterator)
			return pObj->GetNextItem();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"GetNextItem");
			return nullptr;
		}
	}

	/**************************************************************************
	*
	*  GetNextEntry: Get the next Entry for the current item. Returns the first 
	*      entry for the item on the first call to.
	*
	*    pObj: Pointer to the iterator. 
	*    
	*  Return: Pointer to access the data.
	*
	**************************************************************************/

	EXPORT KeyValuePair * GetNextEntry( StrDictListIterator* pObj )
	{
		try
		{
			VALIDATE_HANDLE_P(pObj, tStrDictListIterator)
			return pObj->GetNextEntry();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"GetNextEntry");
			return nullptr;
		}
	}

	/**************************************************************************
	*
	*  Release: Delete an object allocated in the bridge's heap.
	*
	*    pObj: Pointer to the iterator. 
	*    
	*  Return: None.
	*
	**************************************************************************/

	EXPORT void Release( void* pObj )
	{
		try
		{
			// make sure to cast to a p4base object first, otherwise
			// the destructor will not be called and 'bad things will happen'
			p4base* pBase = static_cast<p4base*>(pObj);
			delete pBase;
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"Release");
		}
	}

	/**************************************************************************
	*
	*  Release: Delete an array allocated in the bridge's heap.
	*
	*    pObj: Pointer to the iterator. 
	*    
	*  Return: None.
	*
	**************************************************************************/

	EXPORT void ReleaseString( void* pObj )
	{
		try
		{
			Utils::ReleaseString(pObj);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"ReleaseString");
		}
	}

	/**************************************************************************
	* class KeyValuePair
	**************************************************************************/
	
	/**************************************************************************
	*
	*  GetKey: Get the key.
	*
	*    pObj: Pointer to the KeyValuePair. 
	*    
	*  Return: Pointer to access the data.
	*
	**************************************************************************/

	EXPORT const char * GetKey( KeyValuePair* pObj )
	{
		try
		{
			VALIDATE_HANDLE_P(pObj, tKeyValuePair)
			return pObj->key.c_str();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"GetKey");
			return nullptr;
		}
	}
	
	/**************************************************************************
	*
	*  GetValue: Get the value.
	*
	*    pObj: Pointer to the KeyValuePair. 
	*    
	*  Return: Pointer to access the data.
	*
	**************************************************************************/

	EXPORT const char *  GetValue( KeyValuePair* pObj )
	{
	    try
		{
			VALIDATE_HANDLE_P(pObj, tKeyValuePair)
			return pObj->value.c_str();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"GetValue");
			return nullptr;
		}
	}

	/**************************************************************************
	 *  P4ClientError
	 *************************************************************************/

	/**************************************************************************
	*
	*  GetSeverity: Get the severity.
	*
	*    pObj: Pointer to the P4ClientError. 
	*    
	*  Return: Severity of the Error.
	*
	**************************************************************************/

	EXPORT const int Severity( P4ClientError* pObj )
	{
		try
		{
			VALIDATE_HANDLE_I(pObj, tP4ClientError)
			return pObj->Severity;
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"Severity");
			return 0;
		}
	}

	/**************************************************************************
	*
	*  ErrorCode: Get the Error Code.
	*
	*    pObj: Pointer to the P4ClientError. 
	*    
	*  Return: Unique ErrorCode of the Error.
	*
	**************************************************************************/

	EXPORT const int ErrorCode( P4ClientError* pObj )
	{
		try
		{
			VALIDATE_HANDLE_I(pObj, tP4ClientError)
			return pObj->ErrorCode;
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"ErrorCode");
			return 0;
		}
	}

	/**************************************************************************
	*
	*  Message: Get the error message.
	*
	*    pObj: Pointer to the P4ClientError. 
	*    
	*  Return: Error Message.
	*
	**************************************************************************/

	EXPORT const char * Message( P4ClientError* pObj )
	{
		try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientError)
			return pObj->Message.c_str();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"Message");
			return nullptr;
		}
	}
	
	/**************************************************************************
	*
	*  GetNext: Get the next error message.
	*
	*    pObj: Pointer to the P4ClientError. 
	*    
	*  Return: Pointer to the next error message.
	*
	**************************************************************************/

	EXPORT P4ClientError * Next( P4ClientError * pObj )
	{
		try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientError)
			return pObj->Next;
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"Next");
			return nullptr;
		}
	}


	/**************************************************************************
	 *  P4ClientInfoMsg
	 *************************************************************************/

	/**************************************************************************
	*
	*  GetLevel: Get the message level.
	*
	*    pObj: Pointer to the P4ClientInfoMsg. 
	*    
	*  Return: Message level char from 0->9.
	*
	**************************************************************************/

	EXPORT const char MessageLevel( P4ClientInfoMsg* pObj )
	{
		try
		{
			VALIDATE_HANDLE_C(pObj, tP4ClientInfoMsg)
			return pObj->Level;
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"MessageLevel");
			return 0;
		}
	}

	/**************************************************************************
	*
	*  ErrorCode: Get the Message Code.
	*
	*    pObj: Pointer to the P4ClientInfoMsg. 
	*    
	*  Return: Unique Code of the Message.
	*
	**************************************************************************/

	EXPORT const int InfoMsgCode( P4ClientInfoMsg* pObj )
	{
		try
		{
			VALIDATE_HANDLE_I(pObj, tP4ClientInfoMsg)
			return pObj->MsgCode;
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"InfoMsgCode");
			return 0;
		}
	}

	/**************************************************************************
	*
	*  GetMessage: Get the info message.
	*
	*    pObj: Pointer to the P4ClientInfoMsg. 
	*    
	*  Return: Error Message.
	*
	**************************************************************************/

	EXPORT const char * InfoMessage( P4ClientInfoMsg* pObj )
	{
		try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientInfoMsg)
			return pObj->Message.c_str();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"InfoMessage");
			return nullptr;
		}
	}
	
	/**************************************************************************
	*
	*  GetNext: Get the next message.
	*
	*    pObj: Pointer to the P4ClientInfoMsg. 
	*    
	*  Return: Pointer to the next message.
	*
	**************************************************************************/

	EXPORT P4ClientInfoMsg * NextInfoMsg( P4ClientInfoMsg * pObj )
	{
		try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientInfoMsg)
			return pObj->Next;
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"NextInfoMsg");
			return nullptr;
		}
	}

	/**************************************************************************
	 *
	 *  P4ClientMerge
	 *
	 *  This simple class is a ClientMerge object.
	 *
	 *************************************************************************/

	EXPORT int CM_AutoResolve( P4ClientMerge* pObj, MergeForce forceMerge )
	{
		try
		{
			VALIDATE_HANDLE_I(pObj, tP4ClientMerge);
			return (int) pObj->AutoResolve(forceMerge);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"CM_AutoResolve");
			return -1;
		}
	}

	EXPORT int CM_Resolve( P4ClientMerge* pObj )
	{
		try
		{
			VALIDATE_HANDLE_I(pObj, tP4ClientMerge);
			return (int) pObj->Resolve();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"CM_Resolve");
			return -1;
		}
	}

	EXPORT int CM_DetectResolve( P4ClientMerge* pObj )
	{
		try
		{
			VALIDATE_HANDLE_I(pObj, tP4ClientMerge);
			return (int) pObj->DetectResolve();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"CM_DetectResolve");
			return -1;
		}
	}

	EXPORT int CM_IsAcceptable( P4ClientMerge* pObj )
	{
		try
		{
			VALIDATE_HANDLE_I(pObj, tP4ClientMerge);
			return pObj->IsAcceptable();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"CM_IsAcceptable");
			return -1;
		}
	}

	EXPORT char *CM_GetBaseFile( P4ClientMerge* pObj )
	{
		try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientMerge)
			if (!pObj->GetBaseFile())
				return nullptr;
			return pObj->GetBaseFile()->Text();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"CM_GetBaseFile");
			return nullptr;
		}
	}

	EXPORT char *CM_GetYourFile( P4ClientMerge* pObj )
	{
		try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientMerge)
			if (!pObj->GetYourFile())
				return nullptr;
			return pObj->GetYourFile()->Text();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"CM_GetYourFile");
			return nullptr;
		}
	}

	EXPORT char *CM_GetTheirFile( P4ClientMerge* pObj )
	{
		try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientMerge)
			if (!pObj->GetTheirFile())
				return nullptr;
			return pObj->GetTheirFile()->Text();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"CM_GetTheirFile");
			return nullptr;
		}
	}

	EXPORT char *CM_GetResultFile( P4ClientMerge* pObj )
	{
		try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientMerge)
			if (!pObj->GetResultFile())
				return nullptr;
			return pObj->GetResultFile()->Text();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"CM_GetResultFile");
			return nullptr;
		}
	}


	EXPORT int	CM_GetYourChunks( P4ClientMerge* pObj )
	{
		try
		{
			VALIDATE_HANDLE_I(pObj, tP4ClientMerge);
			return pObj->GetYourChunks();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"CM_GetYourChunks");
			return -1;
		}
	}

	EXPORT int	CM_GetTheirChunks( P4ClientMerge* pObj )
	{
		try
		{
			VALIDATE_HANDLE_I(pObj, tP4ClientMerge);
			return pObj->GetTheirChunks();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"CM_GetTheirChunks");
			return -1;
		}
	}

	EXPORT int	CM_GetBothChunks( P4ClientMerge* pObj )
	{
		try
		{
			VALIDATE_HANDLE_I(pObj, tP4ClientMerge);
			return pObj->GetBothChunks();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"CM_GetBothChunks");
			return -1;
		}
	}

	EXPORT int	CM_GetConflictChunks(P4ClientMerge* pObj)
	{
		try
		{
			VALIDATE_HANDLE_I(pObj, tP4ClientMerge);
			return pObj->GetConflictChunks();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e, "CM_GetConflictChunks");
			return -1;
		}
	}

	EXPORT char *CM_GetMergeDigest( P4ClientMerge* pObj )
	{
		try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientMerge)
			if (!pObj->GetBaseFile())
				return nullptr;
			return pObj->GetMergeDigest()->Text();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"CM_GetMergeDigest");
			return nullptr;
		}
	}

	EXPORT char *CM_GetYourDigest( P4ClientMerge* pObj )
	{
		try
		{
			if (!pObj->GetBaseFile())
				return nullptr;
			return pObj->GetYourDigest()->Text();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"CM_GetYourDigest");
			return nullptr;
		}	
	}

	EXPORT char *CM_GetTheirDigest( P4ClientMerge* pObj )
	{
		try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientMerge)
			if (!pObj->GetBaseFile())
				return nullptr;
			return pObj->GetTheirDigest()->Text();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"CM_GetTheirDigest");
			return nullptr;
		}
	}

	EXPORT P4ClientError *CM_GetLastClientMergeError(P4ClientMerge* pObj)
	{
		try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientMerge)
			return pObj->GetLastError();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"CM_GetLastClientMergeError");
			return nullptr;
		}
	}

/*******************************************************************************
 *
 *  P4ClientResolve
 *
 *  This simple class is a wrapper for ClientResolve object.
 *
 ******************************************************************************/

	EXPORT int CR_AutoResolve( P4ClientResolve* pObj, MergeForce force )
	{
		try
		{
			VALIDATE_HANDLE_I(pObj, tP4ClientResolve);
			return (int) pObj->AutoResolve(force);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"CR_AutoResolve");
			return 0;
		}
	}

	EXPORT int CR_Resolve( P4ClientResolve* pObj, int preview, Error *err )
	{
		try
		{
			VALIDATE_HANDLE_I(pObj, tP4ClientResolve);
			return (int) pObj->Resolve(preview);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"CR_Resolve");
			return 0;
		}
	}

	EXPORT char *CR_GetType(P4ClientResolve* pObj)
	{
		try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientResolve);
			return pObj->GetType().Text();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"CR_GetType");
			return nullptr;
		}
	}

	EXPORT char *CR_GetMergeAction(P4ClientResolve* pObj)
	{
		try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientResolve)
			return pObj->GetMergeAction().Text();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"CR_GetMergeAction");
			return nullptr;
		}
	}

	EXPORT char *CR_GetYoursAction(P4ClientResolve* pObj)
	{
		try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientResolve)
			return pObj->GetYoursAction().Text();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"CR_GetYoursAction");
			return nullptr;
		}
	}

	EXPORT char *CR_GetTheirAction(P4ClientResolve* pObj)
	{
		try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientResolve)
			return pObj->GetTheirAction().Text();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"CR_GetTheirAction");
			return nullptr;
		}
	}

	// For the CLI interface, probably not of interest to others

	EXPORT char *CR_GetMergePrompt(P4ClientResolve* pObj)
	{
		try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientResolve)
			return pObj->GetMergePrompt().Text();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"CR_GetMergePrompt");
			return nullptr;
		}
	}

	EXPORT char *CR_GetYoursPrompt(P4ClientResolve* pObj)
	{
		try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientResolve)
			return pObj->GetYoursPrompt().Text();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"CR_GetYoursPrompt");
			return nullptr;
		}
	}

	EXPORT char *CR_GetTheirPrompt(P4ClientResolve* pObj)
	{
		try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientResolve)
			return pObj->GetTheirPrompt().Text();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"CR_GetTheirPrompt");
			return nullptr;
		}
	}

	EXPORT char *CR_GetMergeOpt(P4ClientResolve* pObj)
	{
		try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientResolve)
			return pObj->GetMergeOpt().Text();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"CR_GetMergeOpt");
			return nullptr;
		}
	}

	EXPORT char *CR_GetYoursOpt(P4ClientResolve* pObj)
	{
		try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientResolve)
			return pObj->GetYoursOpt().Text();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"CR_GetYoursOpt");
			return nullptr;
		}
	}

	EXPORT char *CR_GetTheirOpt(P4ClientResolve* pObj)
	{
		try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientResolve)
			return pObj->GetTheirOpt().Text();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"CR_GetTheirOpt");
			return nullptr;
		}
	}

	EXPORT char *CR_GetSkipOpt(P4ClientResolve* pObj)
	{
		try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientResolve)
			return pObj->GetSkipOpt().Text();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"CR_GetSkipOpt");
			return nullptr;
		}
	}

	EXPORT char *CR_GetHelpOpt(P4ClientResolve* pObj)
	{
		try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientResolve)
			return pObj->GetHelpOpt().Text();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"CR_GetHelpOpt");
			return nullptr;
		}
	}

	EXPORT char *CR_GetAutoOpt(P4ClientResolve* pObj)
	{
		try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientResolve)
			return pObj->GetAutoOpt().Text();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"CR_GetAutoOpt");
			return nullptr;
		}
	}

	EXPORT char *CR_GetPrompt(P4ClientResolve* pObj)
	{
		try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientResolve)
			return pObj->GetPrompt().Text();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"CR_GetPrompt");
			return nullptr;
		}
	}

	EXPORT char *CR_GetTypePrompt(P4ClientResolve* pObj)
	{
		try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientResolve)
			return pObj->GetTypePrompt().Text();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"CR_GetTypePrompt");
			return nullptr;
		}
	}

	EXPORT char *CR_GetUsageError(P4ClientResolve* pObj)
	{
		try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientResolve)
			return pObj->GetUsageError().Text();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"CR_GetUsageError");
			return nullptr;
		}
	}

	EXPORT char *CR_GetHelp(P4ClientResolve* pObj)
	{
		try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientResolve)
			return pObj->GetHelp().Text();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"CR_GetHelp");
			return nullptr;
		}
	}
	
	EXPORT P4ClientError *CR_GetLastError(P4ClientResolve* pObj)
	{
		try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientResolve)
			return pObj->GetLastError();
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"CR_GetLastError");
			return nullptr;
		}
	}

	/**************************************************************************
	*
	*  SetResolveCallbackFn: Set the callback for replying to a resolve 
	*		callback.
	*
	*    pServer: Pointer to the P4BridgeServer 
	*
	*    pNew: The new callback function pointer
	*    
	*  Return: None
	**************************************************************************/

	EXPORT void SetResolveCallbackFn(	P4BridgeServer* pServer,
														ResolveCallbackFn* pNew )
	{
		try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
			pServer->SetResolveCallbackFn(pNew);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"SetResolveCallbackFn");
		}
	}

	/**************************************************************************
	*
	*  SetResolveACallbackFn: Set the callback for replying to a resolve 
	*		callback.
	*
	*    pServer: Pointer to the P4BridgeServer 
	*
	*    pNew: The new callback function pointer
	*    
	*  Return: None
	**************************************************************************/

	EXPORT void SetResolveACallbackFn(	P4BridgeServer* pServer,
														ResolveACallbackFn* pNew )
	{
		try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
			pServer->SetResolveACallbackFn(pNew);
		}
		catch (exception& e)
		{
			P4BridgeServer::ReportException(e,"SetResolveACallbackFn");
		}
	}

#if defined(_DEBUG)
        EXPORT int GetAllocObjCount()            { return p4typesCount;  }
        EXPORT int GetAllocObj(int type)         { return p4base::GetItemCount(type); }
        EXPORT const char* GetAllocObjName(int type) {   return p4base::GetTypeStr(type);        }
        EXPORT long GetStringAllocs()            { return Utils::AllocCount(); }
        EXPORT long GetStringReleases()          { return Utils::FreeCount(); }
#else
        EXPORT int GetAllocObjCount()            { return 0; }
        EXPORT int GetAllocObj(int type)         { return 0; }
        EXPORT const char* GetAllocObjName(int type) { return "only available in _DEBUG builds"; }
        EXPORT long GetStringAllocs()            { return 0; }
        EXPORT long GetStringReleases()          { return 0; }
#endif

