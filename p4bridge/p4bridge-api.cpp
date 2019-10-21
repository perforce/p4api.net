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

#include "stdafx.h"
#include "ticket.h"
#include "P4BridgeServer.h"
#ifdef _DEBUG_VLD
#include <vld.h> 
#endif

// If there is a connection error, keep it so the client can fetch it later
P4ClientError * connectionError = NULL;

// #define STACKTRACE  // I don't know if this will work on a 32 bit build

#ifdef STACKTRACE
#include <WinBase.h>
#include <DbgHelp.h>

#define STACK_SIZE 20

void printStackTrace()
{
    char *result = "";
    unsigned int   i;
    void          *stack[STACK_SIZE];
    unsigned short frames;
    SYMBOL_INFO   *symbol;
    HANDLE         process;
    process = GetCurrentProcess();
    SymInitialize( process, NULL, TRUE );
    frames               = CaptureStackBackTrace( 0, STACK_SIZE, stack, NULL );
    symbol               = ( SYMBOL_INFO * )calloc( sizeof( SYMBOL_INFO ) + 256 * sizeof( char ), 1 );
    symbol->MaxNameLen   = 255;
    symbol->SizeOfStruct = sizeof( SYMBOL_INFO );
    for( i = 0; i < frames; i++ )
    {
        SymFromAddr( process, ( DWORD64 )( stack[ i ] ), 0, symbol );
        char * symbol_name = "unknown symbol";
		if (strlen(symbol->Name) > 0)
		{
			symbol_name = symbol->Name;
		}

        P4BridgeServer::LogMessage(0, __FILE__,__LINE__,"%3d %s %I64x\n", frames - i -1, symbol_name, symbol->Address);
    }
    free( symbol );
}

#endif

#define HANDLE_EXCEPTION() HandleException(__FILE__, __LINE__, __FUNCTION__, GetExceptionCode(), GetExceptionInformation())

/*******************************************************************************
*
*  HandleException
*
*  Handle any platform exceptions. The Microsoft Structured Exception Handler
*      allows software to catch platform exceptions such as array overrun. The
*      exception is logged, but the application will continue to run.
*
******************************************************************************/

int HandleException(const char* fname, unsigned int line, const char* func, unsigned int c, struct _EXCEPTION_POINTERS *e)
{
	unsigned int code = c;
	struct _EXCEPTION_POINTERS *ep = e;

	// Log the exception
	char * exType = "Unknown";

	switch (code)
	{
	case EXCEPTION_ACCESS_VIOLATION:
		exType = "EXCEPTION_ACCESS_VIOLATION\r\n";
		break;
	case EXCEPTION_DATATYPE_MISALIGNMENT:
		exType = "EXCEPTION_DATATYPE_MISALIGNMENT\r\n";
		break;
	case EXCEPTION_BREAKPOINT:
		exType = "EXCEPTION_BREAKPOINT\r\n";
		break;
	case EXCEPTION_SINGLE_STEP:
		exType = "EXCEPTION_SINGLE_STEP\r\n";
		break;
	case EXCEPTION_ARRAY_BOUNDS_EXCEEDED:
		exType = "EXCEPTION_ARRAY_BOUNDS_EXCEEDED\r\n";
		break;
	case EXCEPTION_FLT_DENORMAL_OPERAND:
		exType = "EXCEPTION_FLT_DENORMAL_OPERAND\r\n";
		break;
	case EXCEPTION_FLT_DIVIDE_BY_ZERO:
		exType = "EXCEPTION_FLT_DIVIDE_BY_ZERO\r\n";
		break;
	case EXCEPTION_FLT_INEXACT_RESULT:
		exType = "EXCEPTION_FLT_INEXACT_RESULT\r\n";
		break;
	case EXCEPTION_FLT_INVALID_OPERATION:
		exType = "EXCEPTION_FLT_INVALID_OPERATION\r\n";
		break;
	case EXCEPTION_FLT_OVERFLOW:
		exType = "EXCEPTION_FLT_OVERFLOW\r\n";
		break;
	case EXCEPTION_FLT_STACK_CHECK:
		exType = "EXCEPTION_FLT_STACK_CHECK\r\n";
		break;
	case EXCEPTION_FLT_UNDERFLOW:
		exType = "EXCEPTION_FLT_UNDERFLOW\r\n";
		break;
	case EXCEPTION_INT_DIVIDE_BY_ZERO:
		exType = "EXCEPTION_INT_DIVIDE_BY_ZERO\r\n";
		break;
	case EXCEPTION_INT_OVERFLOW:
		exType = "EXCEPTION_INT_OVERFLOW\r\n";
		break;
	case EXCEPTION_PRIV_INSTRUCTION:
		exType = "EXCEPTION_PRIV_INSTRUCTION\r\n";
		break;
	case EXCEPTION_IN_PAGE_ERROR:
		exType = "EXCEPTION_IN_PAGE_ERROR\r\n";
		break;
	case EXCEPTION_ILLEGAL_INSTRUCTION:
		exType = "EXCEPTION_ILLEGAL_INSTRUCTION\r\n";
		break;
	case EXCEPTION_NONCONTINUABLE_EXCEPTION:
		exType = "EXCEPTION_NONCONTINUABLE_EXCEPTION\r\n";
		break;
	case EXCEPTION_STACK_OVERFLOW:
		exType = "EXCEPTION_STACK_OVERFLOW\r\n";
		break;
	case EXCEPTION_INVALID_DISPOSITION:
		exType = "EXCEPTION_INVALID_DISPOSITION\r\n";
		break;
	case EXCEPTION_GUARD_PAGE:
		exType = "EXCEPTION_GUARD_PAGE\r\n";
		break;
	case EXCEPTION_INVALID_HANDLE:
		exType = "EXCEPTION_INVALID_HANDLE\r\n";
		break;
	//case EXCEPTION_POSSIBLE_DEADLOCK:
	//    exType = "EXCEPTION_POSSIBLE_DEADLOCK\r\n");
	//    break;
	default:
		exType = "UNKNOWN EXCEPTION\r\n";
		break;
	}

	P4BridgeServer::LogMessage(0, fname, line, "Exception Detected (0x%08x) in bridge function %s: %s", code, func, exType);

#ifdef STACKTRACE
	printStackTrace();
#endif

	return EXCEPTION_EXECUTE_HANDLER;
}

extern "C"
{
	__declspec(dllexport) void ClearConnectionError()
	{
		__try
		{
			// free old error string, if any.
			if (connectionError)
			{
				delete connectionError;
				connectionError = NULL;
			}
		}
		__except (HANDLE_EXCEPTION())
		{
		}
		return;
	}
}

/******************************************************************************
 * Helper function to (re)connect to the server and determine if it is Unicode 
 * enabled.
******************************************************************************/
int ServerConnect(P4BridgeServer* pServer)
{
	__try
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
	__except (HANDLE_EXCEPTION())
	{
		return 0;
	}
}

/******************************************************************************
 * Helper function to (re)connect to the server and determine if it is Unicode 
 * enabled.
******************************************************************************/
int ServerConnectTrust(P4BridgeServer* pServer, char* trust_flag, char* fingerprint)
{
	__try
	{
		// dereference old error string, if any. It's not 'our' string, so we can't
		//  free it.
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
	__except (HANDLE_EXCEPTION())
	{
		return 0;
	}
}

/******************************************************************************
 * 'Flat' C interface for the dll. This interface will be imported into C# 
 *    using P/Invoke 
******************************************************************************/
extern "C" 
{
	__declspec(dllexport) void SetLogFunction(
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
		return NULL;
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

	__declspec(dllexport) P4BridgeServer* Connect(	const char *server, 
													const char *user, 
													const char *pass,
													const char *ws_client,
													LogCallbackFn *log_fn)
	{
		LOG_ENTRY();
		__try
		{
			connectionError = NULL;
			return Connect_Int(	server, user, pass, ws_client, log_fn);
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
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
		// delete pServer?
		delete pServer;
		return NULL;
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

	__declspec(dllexport) P4BridgeServer* TrustedConnect(	const char *server, 
															const char *user, 
															const char *pass,
															const char *ws_client,
															char *trust_flag,
															char *fingerprint,
															LogCallbackFn *log_fn)
	{
		__try
		{
			connectionError = NULL;
			return TrustedConnect_Int( server, user, pass, ws_client, trust_flag, fingerprint, log_fn);
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
		}
	}

	P4BridgeServer* _connection_from_path(const char* cwd)
	{
		// create an un-connected p4bridgeserver based on a path
		// this is handy if you just want to get the connection info 
		// from a directory path
		ClientApi tempApi;
		tempApi.SetCwd(cwd);

		return new P4BridgeServer(
			tempApi.GetPort().Text(),
			tempApi.GetUser().Text(),
			tempApi.GetPassword().Text(),
			tempApi.GetClient().Text());
	}
	__declspec(dllexport) P4BridgeServer* ConnectionFromPath(const char* cwd)
	{
		__try
		{
			return _connection_from_path(cwd);
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
		}
	}
	/**************************************************************************
	*
	*  GetConnectionError: Returns the text of a the error message 
	*   generated by the connection attempt, if any.
	*
	**************************************************************************/

	__declspec(dllexport) P4ClientError * GetConnectionError( void )
	{
		__try
		{
			return connectionError;
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
		}
	}

	/**************************************************************************
	*
	*  CloseConnection: Closes the connection and deletes the P4BridgeServer 
	*		object.
	*
	*    pServer: Pointer to the P4BridgeServer 
	*
	**************************************************************************/
	__declspec(dllexport) int ReleaseConnection( P4BridgeServer* pServer )
	{
		LOG_ENTRY();
		if (!pServer) 
		{
			return 1;
		}
		__try
		{
			// if the handle is invalid or freeing it causes an exception, 
			// just consider it closed so return success
			if (!VALIDATE_HANDLE(pServer, tP4BridgeServer))
			{
				if (pServer) 
				{
					delete pServer;
				}
				return 1;
			}
		}
		__except (HANDLE_EXCEPTION())
		{
			return 1;
		}

		__try
		{
			pServer->SetTaggedOutputCallbackFn(NULL);
			pServer->SetErrorCallbackFn(NULL);
			pServer->SetInfoResultsCallbackFn(NULL);
			pServer->SetTextResultsCallbackFn(NULL);
			pServer->SetBinaryResultsCallbackFn(NULL);
			pServer->SetPromptCallbackFn(NULL);
			pServer->SetParallelTransferCallbackFn(NULL);
			pServer->SetResolveCallbackFn(NULL);
			pServer->SetResolveACallbackFn(NULL);

			LOG_LOC();
			int ret = pServer->close_connection();
			delete pServer;
			return ret;
		}
		__except (HANDLE_EXCEPTION())
		{
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
	__declspec(dllexport) int Disconnect( P4BridgeServer* pServer )
	{
		__try
		{
			VALIDATE_HANDLE_B(pServer, tP4BridgeServer)
			return pServer->disconnect();
		}
		__except (HANDLE_EXCEPTION())
		{
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

	__declspec(dllexport) int IsUnicode( P4BridgeServer* pServer )
	{
		__try
		{
			VALIDATE_HANDLE_B(pServer, tP4BridgeServer)
			return pServer->unicodeServer();
		}
		__except (HANDLE_EXCEPTION())
		{
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

	__declspec(dllexport) int APILevel( P4BridgeServer* pServer )
	{
		__try
		{
			VALIDATE_HANDLE_B(pServer, tP4BridgeServer)
			return pServer->APILevel();
		}
		__except (HANDLE_EXCEPTION())
		{
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

	__declspec(dllexport) int UseLogin( P4BridgeServer* pServer )
	{
		__try
		{
			VALIDATE_HANDLE_B(pServer, tP4BridgeServer)
			return pServer->UseLogin();
		}
		__except (HANDLE_EXCEPTION())
		{
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

	__declspec(dllexport) int SupportsExtSubmit( P4BridgeServer* pServer )
	{
		__try
		{
			VALIDATE_HANDLE_B(pServer, tP4BridgeServer)
			return pServer->SupportsExtSubmit();
		}
		__except (HANDLE_EXCEPTION())
		{
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
	
	__declspec(dllexport) int UrlLaunched()
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
		const char * pFileCharSet)
	{
		return Utils::AllocString(pServer->set_charset(pCharSet, pFileCharSet));
	}

	__declspec(dllexport) const char * SetCharacterSet(   P4BridgeServer* pServer, 
													const char * pCharSet, 
													const char * pFileCharSet )
	{
		__try
		{
			VALIDATE_HANDLE_P(pServer, tP4BridgeServer);
 			return _SetCharacterSet(pServer, pCharSet, pFileCharSet);
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
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

	__declspec(dllexport) void set_connection(P4BridgeServer* pServer,
		const char* newPort,
		const char* newUser,
		const char* newPassword,
		const char* newClient)
	{
		__try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
			return pServer->set_connection(newPort, newUser, newPassword, newClient);
		}
		__except (HANDLE_EXCEPTION())
		{
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

	__declspec(dllexport) void set_client(P4BridgeServer* pServer, const char* workspace)
	{
		__try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
			return pServer->set_client(workspace);
		}
		__except (HANDLE_EXCEPTION())
		{
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

	__declspec(dllexport) const char * get_client(P4BridgeServer* pServer)
	{
		__try
		{
			VALIDATE_HANDLE_P(pServer, tP4BridgeServer);
			return _get_client(pServer);
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
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

	__declspec(dllexport) const char * get_user(P4BridgeServer* pServer)
	{
		__try
		{
			VALIDATE_HANDLE_P(pServer, tP4BridgeServer);
			return _get_user(pServer);
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
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

	__declspec(dllexport) void set_user(P4BridgeServer* pServer, char * newValue)
	{
		__try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
				pServer->set_user(newValue);
		}
		__except (HANDLE_EXCEPTION())
		{
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

	__declspec(dllexport) const char * get_port(P4BridgeServer* pServer)
	{
		__try
		{
			VALIDATE_HANDLE_P(pServer, tP4BridgeServer);
			return _get_port(pServer);
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
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

	__declspec(dllexport) void set_port(P4BridgeServer* pServer, char * newValue)

	{
		__try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
				pServer->set_port(newValue);
		}
		__except (HANDLE_EXCEPTION())
		{
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

	__declspec(dllexport) const char * get_password(P4BridgeServer* pServer)
	{
		__try
		{
			VALIDATE_HANDLE_P(pServer, tP4BridgeServer);
			return _get_password(pServer);
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
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

	__declspec(dllexport) void set_password(P4BridgeServer* pServer, char * newValue)
	{
		__try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
				pServer->set_password(newValue);
		}
		__except (HANDLE_EXCEPTION())
		{
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

	__declspec(dllexport) void set_ticketFile(P4BridgeServer* pServer, const char* ticketFile)
	{
		__try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
				return pServer->set_ticketFile(ticketFile);
		}
		__except (HANDLE_EXCEPTION())
		{
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

	__declspec(dllexport) const char * get_ticketFile(P4BridgeServer* pServer)
	{
		__try
		{
			VALIDATE_HANDLE_P(pServer, tP4BridgeServer);
			return _get_TicketFile(pServer);
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
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

	__declspec(dllexport) const char * get_ticket(char* path, char* port, char* user)
	{
		LOG_ENTRY();
		__try
		{
			return _get_ticket(path, port, user);
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
		}
	}

	const char* get_cwd_int(P4BridgeServer* pServer)
	{
		return Utils::AllocString(pServer->get_cwd());
	}

	/**************************************************************************
	*
	*  GetCwd: Gets the current working directory for the P4BridgeServer.
	*
	*    pServer: Pointer to the P4BridgeServer
	*
	**************************************************************************/
	__declspec(dllexport) const char * get_cwd(P4BridgeServer* pServer)
	{
		__try
		{
			VALIDATE_HANDLE_P(pServer, tP4BridgeServer);
			return get_cwd_int(pServer);
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
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

	__declspec(dllexport) void set_cwd(P4BridgeServer* pServer,
		const char * new_val)
	{
		__try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
			pServer->set_cwd((const char *)new_val);
		}
		__except (HANDLE_EXCEPTION())
		{
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

	__declspec(dllexport) const char * get_programName(P4BridgeServer* pServer)
	{
		__try
		{
			VALIDATE_HANDLE_P(pServer, tP4BridgeServer);
			return _get_programName(pServer);
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
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

	__declspec(dllexport) void set_programName(P4BridgeServer* pServer, char * newValue)
	{
		__try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
				pServer->set_programName(newValue);
		}
		__except (HANDLE_EXCEPTION())
		{
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

	__declspec(dllexport) const char * get_programVer( P4BridgeServer* pServer )
	{
		__try
		{
			VALIDATE_HANDLE_P(pServer, tP4BridgeServer);
			return _get_programVer(pServer);
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
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

	__declspec(dllexport) void set_programVer( P4BridgeServer* pServer, char * newValue )
	{
		__try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
			pServer->set_programVer(newValue);
		}
		__except (HANDLE_EXCEPTION())
		{
		}
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

	__declspec(dllexport) const char * get_charset( P4BridgeServer* pServer )
	{
		__try
		{
			VALIDATE_HANDLE_P(pServer, tP4BridgeServer);
			return _get_charset(pServer);
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
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

	__declspec(dllexport) const char * get_config( P4BridgeServer* pServer )
	{
		__try
		{
			VALIDATE_HANDLE_P(pServer, tP4BridgeServer);
			return _get_config(pServer);
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
		}
	}

	const char* _get_config_cwd(char* cwd)
	{
		LOG_ENTRY();
		return Utils::AllocString(P4BridgeServer::get_config(cwd));
	}

	__declspec(dllexport) const char * get_config_cwd( char* cwd )
	{
		LOG_ENTRY();
		__try
		{
			return _get_config_cwd(cwd);
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
		}
	}

	const char* _Get(const char *var)
	{
		return Utils::AllocString(P4BridgeServer::Get(var));
	}

	__declspec(dllexport) const char* Get( const char *var )
	{
		__try
		{
			return _Get( var );
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
		}
	}

	__declspec(dllexport) void Set( const char *var, const char *val )
	{
		__try
		{
			return P4BridgeServer::Set( var, val );
		}
		__except (HANDLE_EXCEPTION())
		{
		}
	}

	__declspec(dllexport) void Update( const char *var, const char *val )
	{
		__try
		{
			return P4BridgeServer::Update( var, val );
		}
		__except (HANDLE_EXCEPTION())
		{
		}
	}

	__declspec(dllexport) void ReloadEnviro()
	{
		__try
		{
			return P4BridgeServer::Reload();
		}
		__except (HANDLE_EXCEPTION())
		{
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

	__declspec(dllexport) const char* GetTicketFile(  )
	{
		__try
		{
			return _GetTicketFile();
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
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

	__declspec(dllexport) const char* GetTicket( char* port, char* user )
	{
		__try
		{
			return _GetTicket(port, user);
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
		}
	}

	/*
		Raw SetProtocol - must be called on a disconnected pServer to be effective, or on a pServer that you reconnect on
	*/
	__declspec(dllexport) void SetProtocol(P4BridgeServer* pServer, const char* key, const char* val)
	{
		__try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer);
			pServer->SetProtocol(key, val);
		}
		__except (HANDLE_EXCEPTION())
		{
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

	__declspec(dllexport) int RunCommand( P4BridgeServer* pServer, 
										  const char *cmd, 
										  int cmdId,
										  int tagged, 
										  char **args, 
										  int argc )
	{
		__try
		{
			VALIDATE_HANDLE_I(pServer, tP4BridgeServer)
			// make sure we're connected to the server
			if (0 == ServerConnect( pServer ))
			{
				return 0;
			}
			LOG_DEBUG2(4, "Running command [%d] %s", cmdId, cmd);
			return pServer->run_command(cmd, cmdId, tagged, args, argc);
		}
		__except (HANDLE_EXCEPTION())
		{
			return 0;
		}
	}

	/**************************************************************************
	*
	*  CancelCommand: Cancel a running command
	*
	*  Return: None
	**************************************************************************/

	__declspec(dllexport) void CancelCommand( P4BridgeServer* pServer, int cmdId ) 
	{
		__try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
			pServer->cancel_command(cmdId);
		}
		__except (HANDLE_EXCEPTION())
		{
		}
	}

	__declspec(dllexport) int IsConnected(P4BridgeServer* pServer)
	{
		__try
		{
			VALIDATE_HANDLE_I(pServer, tP4BridgeServer)
			return pServer->IsConnected();
		}
		__except (HANDLE_EXCEPTION())
		{
			return -1;
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

	__declspec(dllexport) void SetTaggedOutputCallbackFn( P4BridgeServer* pServer, IntTextTextCallbackFn* pNew )
	{
		__try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
			pServer->SetTaggedOutputCallbackFn(pNew);
		}
		__except (HANDLE_EXCEPTION())
		{
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

	__declspec(dllexport) int GetTaggedOutputCount( P4BridgeServer* pServer, int cmdId )
	{
		__try
		{
			VALIDATE_HANDLE_P(pServer, tP4BridgeServer);
			P4BridgeClient* pUi = pServer->find_ui(cmdId);
			if (!pUi)
				return  -1;
			return pUi->GetTaggedOutputCount();
		}
		__except (HANDLE_EXCEPTION())
		{
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

	__declspec(dllexport) StrDictListIterator * GetTaggedOutput( P4BridgeServer* pServer, int cmdId )
	{
		__try
		{
			VALIDATE_HANDLE_P(pServer, tP4BridgeServer);
			P4BridgeClient* pUi = pServer->find_ui(cmdId);
			if (!pUi)
				return  NULL;
			return pUi->GetTaggedOutput();
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
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

	__declspec(dllexport) void SetErrorCallbackFn( P4BridgeServer* pServer, IntIntIntTextCallbackFn* pNew )
	{
		__try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
			pServer->SetErrorCallbackFn(pNew);
		}
		__except (HANDLE_EXCEPTION())
		{
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

	__declspec(dllexport) P4ClientError * GetErrorResults( P4BridgeServer * pServer, int cmdId)
	{
		__try
		{
			VALIDATE_HANDLE_P(pServer, tP4BridgeServer);
			P4BridgeClient* pUi = pServer->find_ui(cmdId);
			if (!pUi)
				return  NULL;
			return pUi->GetErrorResults();
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
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

	__declspec(dllexport) void SetInfoResultsCallbackFn( P4BridgeServer* pServer, IntIntIntTextCallbackFn* pNew )
	{
		__try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
			pServer->SetInfoResultsCallbackFn(pNew);
		}
		__except (HANDLE_EXCEPTION())
		{
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

	__declspec(dllexport) int GetInfoResultsCount( P4BridgeServer* pServer, int cmdId)
	{
		__try
		{
			VALIDATE_HANDLE_B(pServer, tP4BridgeServer)
			P4BridgeClient* pUi = pServer->find_ui(cmdId);
			if (!pUi)
				return  -1;
			return pUi->GetInfoResultsCount();
		}
		__except (HANDLE_EXCEPTION())
		{
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

	__declspec(dllexport) P4ClientInfoMsg * GetInfoResults( P4BridgeServer* pServer, int cmdId)
	{
		__try
		{
			VALIDATE_HANDLE_B(pServer, tP4BridgeServer)
			P4BridgeClient* pUi = pServer->find_ui(cmdId);
			if (!pUi)
				return  NULL;
			if (!pUi->GetInfoResults())
				return  NULL;
			return pUi->GetInfoResults();
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
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

	__declspec(dllexport) void SetTextResultsCallbackFn( P4BridgeServer* pServer, TextCallbackFn* pNew )
	{
		__try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
			pServer->SetTextResultsCallbackFn(pNew);
		}
		__except (HANDLE_EXCEPTION())
		{
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

	__declspec(dllexport) const char * GetTextResults( P4BridgeServer* pServer, int cmdId )
	{
		__try
		{
			VALIDATE_HANDLE_B(pServer, tP4BridgeServer)
			P4BridgeClient* pUi = pServer->find_ui(cmdId);
			if (!pUi)
				return  NULL;
			if (!pUi->GetTextResults())
				return  NULL;
			return pUi->GetTextResults();
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
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

	__declspec(dllexport) void SetBinaryResultsCallbackFn( P4BridgeServer* pServer, BinaryCallbackFn* pNew )
	{
		__try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
			pServer->SetBinaryResultsCallbackFn(pNew);
		}
		__except (HANDLE_EXCEPTION())
		{
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

	__declspec(dllexport) size_t GetBinaryResultsCount(  P4BridgeServer* pServer, int cmdId) 
	{ 
		__try
		{
			VALIDATE_HANDLE_I(pServer, tP4BridgeServer)
			P4BridgeClient* pUi = pServer->find_ui(cmdId);
			if (!pUi)
				return  0;
			return pUi->GetBinaryResultsCount( );
		}
		__except (HANDLE_EXCEPTION())
		{
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

	__declspec(dllexport) const unsigned char* GetBinaryResults( P4BridgeServer* pServer, int cmdId )
	{
		__try
		{
			VALIDATE_HANDLE_P(pServer, tP4BridgeServer);
			P4BridgeClient* pUi = pServer->find_ui(cmdId);
			if (!pUi)
				return  NULL;
			return pUi->GetBinaryResults( );
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
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

	__declspec(dllexport) void SetDataSet( P4BridgeServer* pServer, int cmdId,
										   const char * data )
	{
		__try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
			P4BridgeClient* pUi = pServer->get_ui(cmdId);
			if (!pUi)
				return;
			return pUi->SetDataSet(data);
		}
		__except (HANDLE_EXCEPTION())
		{
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

	__declspec(dllexport)  char * GetDataSet( P4BridgeServer* pServer, int cmdId )
	{
		__try
		{
			VALIDATE_HANDLE_P(pServer, tP4BridgeServer);
			P4BridgeClient* pUi = pServer->find_ui(cmdId);
			if (!pUi)
				return  NULL;
			return pUi->GetDataSet()->Text();
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
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

	__declspec(dllexport) void SetPromptCallbackFn( P4BridgeServer* pServer, 
													PromptCallbackFn* pNew )
	{
		__try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
			pServer->SetPromptCallbackFn(pNew);
		}
		__except (HANDLE_EXCEPTION())
		{
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

	__declspec(dllexport) void SetParallelTransferCallbackFn(P4BridgeServer* pServer,
		ParallelTransferCallbackFn* pNew)
	{
		__try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
			pServer->SetParallelTransferCallbackFn(pNew);
		}
		__except (HANDLE_EXCEPTION())
		{
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

	__declspec(dllexport) int IsIgnored( const char *pPath )
	{
		__try
		{
			StrPtr Str = StrRef(pPath);
			return P4BridgeServer::IsIgnored(Str);
		}
		__except (HANDLE_EXCEPTION())
		{
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
	*    data once, as there is no method to rest it.
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

	__declspec(dllexport) StrDictList* GetNextItem( StrDictListIterator* pObj )
	{
		__try
		{
			VALIDATE_HANDLE_P(pObj, tStrDictListIterator)
			return pObj->GetNextItem();
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
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

	__declspec(dllexport) KeyValuePair * GetNextEntry( StrDictListIterator* pObj )
	{
		__try
		{
			VALIDATE_HANDLE_P(pObj, tStrDictListIterator)
			return pObj->GetNextEntry();
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
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

	__declspec(dllexport) void Release( void* pObj )
	{
		__try
		{
			// make sure to cast to a p4base object first, otherwise
			// the destructor will not be called and 'bad things will happen'
			p4base* pBase = static_cast<p4base*>(pObj);
			delete pBase;
		}
		__except (HANDLE_EXCEPTION())
		{
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

	__declspec(dllexport) void ReleaseString( void* pObj )
	{
		__try
		{
			Utils::ReleaseString(pObj);
		}
		__except (HANDLE_EXCEPTION())
		{
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

	__declspec(dllexport) const char * GetKey( KeyValuePair* pObj )
	{
		__try
		{
			VALIDATE_HANDLE_P(pObj, tKeyValuePair)
			return pObj->key.c_str();
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
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

	__declspec(dllexport) const char *  GetValue( KeyValuePair* pObj )
	{
		__try
		{
			VALIDATE_HANDLE_P(pObj, tKeyValuePair)
			return pObj->value.c_str();
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
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

	__declspec(dllexport) const int Severity( P4ClientError* pObj )
	{
		__try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientError)
			return pObj->Severity;
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
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

	__declspec(dllexport) const int ErrorCode( P4ClientError* pObj )
	{
		__try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientError)
			return pObj->ErrorCode;
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
		}
	}

	/**************************************************************************
	*
	*  GetMessage: Get the error message.
	*
	*    pObj: Pointer to the P4ClientError. 
	*    
	*  Return: Error Message.
	*
	**************************************************************************/

	__declspec(dllexport) const char * Message( P4ClientError* pObj )
	{
		__try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientError)
			return pObj->Message.c_str();
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
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

	__declspec(dllexport) P4ClientError * Next( P4ClientError * pObj )
	{
		__try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientError)
			return pObj->Next;
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
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

	__declspec(dllexport) const char MessageLevel( P4ClientInfoMsg* pObj )
	{
		__try
		{
			VALIDATE_HANDLE_C(pObj, tP4ClientInfoMsg)
			return pObj->Level;
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
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

	__declspec(dllexport) const int InfoMsgCode( P4ClientInfoMsg* pObj )
	{
		__try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientInfoMsg)
			return pObj->MsgCode;
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
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

	__declspec(dllexport) const char * InfoMessage( P4ClientInfoMsg* pObj )
	{
		__try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientInfoMsg)
			return pObj->Message.c_str();
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
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

	__declspec(dllexport) P4ClientInfoMsg * NextInfoMsg( P4ClientInfoMsg * pObj )
	{
		__try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientInfoMsg)
			return pObj->Next;
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
		}
	}

	/**************************************************************************
	 *
	 *  P4ClientMerge
	 *
	 *  This simple class is a ClientMerge object.
	 *
	 *************************************************************************/

	__declspec(dllexport) int CM_AutoResolve( P4ClientMerge* pObj, MergeForce forceMerge )
	{
		__try
		{
			VALIDATE_HANDLE_I(pObj, tP4ClientMerge);
			return (int) pObj->AutoResolve(forceMerge);
		}
		__except (HANDLE_EXCEPTION())
		{
			return -1;
		}
	}

	__declspec(dllexport) int CM_Resolve( P4ClientMerge* pObj )
	{
		__try
		{
			VALIDATE_HANDLE_I(pObj, tP4ClientMerge);
			return (int) pObj->Resolve();
		}
		__except (HANDLE_EXCEPTION())
		{
			return -1;
		}
	}

	__declspec(dllexport) int CM_DetectResolve( P4ClientMerge* pObj )
	{
		__try
		{
			VALIDATE_HANDLE_I(pObj, tP4ClientMerge);
			return (int) pObj->DetectResolve();
		}
		__except (HANDLE_EXCEPTION())
		{
			return -1;
		}
	}

	__declspec(dllexport) int CM_IsAcceptable( P4ClientMerge* pObj )
	{
		__try
		{
			VALIDATE_HANDLE_I(pObj, tP4ClientMerge);
			return pObj->IsAcceptable();
		}
		__except (HANDLE_EXCEPTION())
		{
			return -1;
		}
	}

	__declspec(dllexport) char *CM_GetBaseFile( P4ClientMerge* pObj )
	{
		__try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientMerge)
			if (!pObj->GetBaseFile())
				return NULL;
			return pObj->GetBaseFile()->Text();
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
		}
	}

	__declspec(dllexport) char *CM_GetYourFile( P4ClientMerge* pObj )
	{
		__try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientMerge)
			if (!pObj->GetYourFile())
				return NULL;
			return pObj->GetYourFile()->Text();
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
		}
	}

	__declspec(dllexport) char *CM_GetTheirFile( P4ClientMerge* pObj )
	{
		__try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientMerge)
			if (!pObj->GetTheirFile())
				return NULL;
			return pObj->GetTheirFile()->Text();
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
		}
	}

	__declspec(dllexport) char *CM_GetResultFile( P4ClientMerge* pObj )
	{
		__try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientMerge)
			if (!pObj->GetResultFile())
				return NULL;
			return pObj->GetResultFile()->Text();
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
		}
	}


	__declspec(dllexport) int	CM_GetYourChunks( P4ClientMerge* pObj )
	{
		__try
		{
			VALIDATE_HANDLE_I(pObj, tP4ClientMerge);
			return pObj->GetYourChunks();
		}
		__except (HANDLE_EXCEPTION())
		{
			return -1;
		}
	}

	__declspec(dllexport) int	CM_GetTheirChunks( P4ClientMerge* pObj )
	{
		__try
		{
			VALIDATE_HANDLE_I(pObj, tP4ClientMerge);
			return pObj->GetTheirChunks();
		}
		__except (HANDLE_EXCEPTION())
		{
			return -1;
		}
	}

	__declspec(dllexport) int	CM_GetBothChunks( P4ClientMerge* pObj )
	{
		__try
		{
			VALIDATE_HANDLE_I(pObj, tP4ClientMerge);
			return pObj->GetBothChunks();
		}
		__except (HANDLE_EXCEPTION())
		{
			return -1;
		}
	}

	__declspec(dllexport) int	CM_GetConflictChunks( P4ClientMerge* pObj )
	{
		__try
		{
			VALIDATE_HANDLE_I(pObj, tP4ClientMerge);
			return pObj->GetConflictChunks();
		}
		__except (HANDLE_EXCEPTION())
		{
			return -1;
		}
	}

	__declspec(dllexport) char *CM_GetMergeDigest( P4ClientMerge* pObj )
	{
		__try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientMerge)
			if (!pObj->GetBaseFile())
				return NULL;
			return pObj->GetMergeDigest()->Text();
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
		}
	}

	__declspec(dllexport) char *CM_GetYourDigest( P4ClientMerge* pObj )
	{
		__try
		{
			if (!pObj->GetBaseFile())
				return NULL;
			return pObj->GetYourDigest()->Text();
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
		}	
	}

	__declspec(dllexport) char *CM_GetTheirDigest( P4ClientMerge* pObj )
	{
		__try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientMerge)
			if (!pObj->GetBaseFile())
				return NULL;
			return pObj->GetTheirDigest()->Text();
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
		}
	}

	__declspec(dllexport) P4ClientError *CM_GetLastClientMergeError(P4ClientMerge* pObj)
	{
		__try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientMerge)
			return pObj->GetLastError();
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
		}
	}

/*******************************************************************************
 *
 *  P4ClientResolve
 *
 *  This simple class is a wrapper for ClientResolve object.
 *
 ******************************************************************************/

	__declspec(dllexport) int CR_AutoResolve( P4ClientResolve* pObj, MergeForce force )
	{
		__try
		{
			VALIDATE_HANDLE_I(pObj, tP4ClientResolve);
			return (int) pObj->AutoResolve(force);
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
		}
	}

	__declspec(dllexport) int CR_Resolve( P4ClientResolve* pObj, int preview, Error *e )
	{
		__try
		{
			VALIDATE_HANDLE_I(pObj, tP4ClientResolve);
			return (int) pObj->Resolve(preview);
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
		}
	}

	__declspec(dllexport) char *CR_GetType(P4ClientResolve* pObj)
	{
		__try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientResolve);
			return pObj->GetType().Text();
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
		}
	}

	__declspec(dllexport) char *CR_GetMergeAction(P4ClientResolve* pObj)
	{
		__try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientResolve)
			return pObj->GetMergeAction().Text();
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
		}
	}

	__declspec(dllexport) char *CR_GetYoursAction(P4ClientResolve* pObj)
	{
		__try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientResolve)
			return pObj->GetYoursAction().Text();
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
		}
	}

	__declspec(dllexport) char *CR_GetTheirAction(P4ClientResolve* pObj)
	{
		__try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientResolve)
			return pObj->GetTheirAction().Text();
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
		}
	}

	// For the CLI interface, probably not of interest to others

	__declspec(dllexport) char *CR_GetMergePrompt(P4ClientResolve* pObj)
	{
		__try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientResolve)
			return pObj->GetMergePrompt().Text();
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
		}
	}

	__declspec(dllexport) char *CR_GetYoursPrompt(P4ClientResolve* pObj)
	{
		__try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientResolve)
			return pObj->GetYoursPrompt().Text();
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
		}
	}

	__declspec(dllexport) char *CR_GetTheirPrompt(P4ClientResolve* pObj)
	{
		__try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientResolve)
			return pObj->GetTheirPrompt().Text();
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
		}
	}

	__declspec(dllexport) char *CR_GetMergeOpt(P4ClientResolve* pObj)
	{
		__try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientResolve)
			return pObj->GetMergeOpt().Text();
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
		}
	}

	__declspec(dllexport) char *CR_GetYoursOpt(P4ClientResolve* pObj)
	{
		__try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientResolve)
			return pObj->GetYoursOpt().Text();
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
		}
	}

	__declspec(dllexport) char *CR_GetTheirOpt(P4ClientResolve* pObj)
	{
		__try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientResolve)
			return pObj->GetTheirOpt().Text();
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
		}
	}

	__declspec(dllexport) char *CR_GetSkipOpt(P4ClientResolve* pObj) 
	{
		__try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientResolve)
			return pObj->GetSkipOpt().Text();
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
		}
	}

	__declspec(dllexport) char *CR_GetHelpOpt(P4ClientResolve* pObj) 
	{
		__try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientResolve)
			return pObj->GetHelpOpt().Text();
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
		}
	}

	__declspec(dllexport) char *CR_GetAutoOpt(P4ClientResolve* pObj) 
	{
		__try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientResolve)
			return pObj->GetAutoOpt().Text();
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
		}
	}

	__declspec(dllexport) char *CR_GetPrompt(P4ClientResolve* pObj)
	{
		__try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientResolve)
			return pObj->GetPrompt().Text();
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
		}
	}

	__declspec(dllexport) char *CR_GetTypePrompt(P4ClientResolve* pObj)
	{
		__try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientResolve)
			return pObj->GetTypePrompt().Text();
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
		}
	}

	__declspec(dllexport) char *CR_GetUsageError(P4ClientResolve* pObj)
	{
		__try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientResolve)
			return pObj->GetUsageError().Text();
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
		}
	}

	__declspec(dllexport) char *CR_GetHelp(P4ClientResolve* pObj)
	{
		__try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientResolve)
			return pObj->GetHelp().Text();
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
		}
	}
	
	__declspec(dllexport) P4ClientError *CR_GetLastError(P4ClientResolve* pObj)
	{
		__try
		{
			VALIDATE_HANDLE_P(pObj, tP4ClientResolve)
			return pObj->GetLastError();
		}
		__except (HANDLE_EXCEPTION())
		{
			return NULL;
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

	__declspec(dllexport) void SetResolveCallbackFn(	P4BridgeServer* pServer, 
														ResolveCallbackFn* pNew )
	{
		__try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
			pServer->SetResolveCallbackFn(pNew);
		}
		__except (HANDLE_EXCEPTION())
		{
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

	__declspec(dllexport) void SetResolveACallbackFn(	P4BridgeServer* pServer, 
														ResolveACallbackFn* pNew )
	{
		__try
		{
			VALIDATE_HANDLE_V(pServer, tP4BridgeServer)
			pServer->SetResolveACallbackFn(pNew);
		}
		__except (HANDLE_EXCEPTION())
		{
		}
	}

#if defined(_DEBUG)
	__declspec(dllexport) int GetAllocObjCount()		{ return p4typesCount;  }
	__declspec(dllexport) int GetAllocObj(int type)		{ return p4base::GetItemCount(type); }
	__declspec(dllexport) const char* GetAllocObjName(int type) {	return p4base::GetTypeStr(type);	}
	__declspec(dllexport) long GetStringAllocs()		{ return Utils::AllocCount(); }
	__declspec(dllexport) long GetStringReleases()		{ return Utils::FreeCount(); }
#else
	__declspec(dllexport) int GetAllocObjCount()		{ return 0; }
	__declspec(dllexport) int GetAllocObj(int type)		{ return 0; }
	__declspec(dllexport) const char* GetAllocObjName(int type) { return "only available in _DEBUG builds"; }
	__declspec(dllexport) long GetStringAllocs()		{ return 0; }
	__declspec(dllexport) long GetStringReleases()		{ return 0; }
#endif
}
