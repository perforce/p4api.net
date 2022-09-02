/*******************************************************************************

Copyright (c) 2017, Perforce Software, Inc.  All rights reserved.

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
 * Name		: P4Server.cs
 *
 * Author	: dscheirer
 *
 * Description	: A multithreaded manager for P4Server instances
 *
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perforce.P4
{
    /// <summary>
    /// A multithreaded manager for P4Server.  A P4Server instance should not be shared between threads;
    /// instead, either create a P4Server in each thread where you need a connection, or a create a global P4ServerMT and call P4ServerMT.getThread()
    /// from the thread context to get a thread-safe connection to a Perforce server.
    /// </summary>
    public class P4ServerMT : IDisposable
    {
        private String server;
        private String user;
        private String pass;
        private String ws_client;

        private Dictionary<int, P4Server> mapTIDtoServer = new Dictionary<int, P4Server>();
        private Object dictLock = new object();
        private string cwd;
        private string trustFlag;
        private string fingerprint;

        /// <summary>
        /// Create a handler for later creation of P4Server using the provided parameters
        /// </summary>
        /// <param name="server">host:port for the P4 server</param>
        /// <param name="user">User name for the login. Can be null/blank if only running commands that do not require a login.</param>
        /// <param name="pass">Password for the login. Can be null/blank if only running commands that do not require a login.</param>
        /// <param name="ws_client">Workspace (client) to be used by the connection. Can be null/blank if only running commands that do not require a login.</param>
        /// <returns></returns>
        public P4ServerMT(String server,
                         String user,
                         String pass,
                         String ws_client)
        {
            this.server = server;
            this.user = user;
            this.pass = pass;
            this.ws_client = ws_client;
            _programName = "";
            _programVersion = "";
            _characterSet = "";
            _runCmdTimeout = TimeSpan.Zero;
        }

        /// <summary>
        /// Create a handler for later creation of P4Server using the provided parameters
        /// </summary>
        /// <param name="server">Host:port for the P4 server.</param>
        /// <param name="user">User name for the login. 
        ///     Can be null/blank if only running commands that do not require 
        ///     a login.</param>
        /// <param name="pass">Password for  the login. Can be null/blank if 
        ///     only running commands that do not require a login.</param>
        /// <param name="ws_client">Workspace (client) to be used by the 
        ///     connection. Can be null/blank if only running commands that do 
        ///     not require a login.</param>
        /// <param name="cwd">Current working directory</param>
        /// <param name="trust_flag">Trust or not</param>
        /// <param name="fingerprint">Fingerprint to trust</param>
        public P4ServerMT(string server, string user, string pass, string ws_client, string cwd, string trust_flag, string fingerprint) 
            : this(server, user, pass, ws_client)
        {
            this.cwd = cwd;
            this.trustFlag = trust_flag;
            this.fingerprint = fingerprint;
        }

        /// <summary>
        /// Create a handler for later creation of P4Server using the provided parameters
        /// </summary>
        /// <param name="cwd">Current working Directory. Can be null/blank if 
        ///		not connecting to the Perforce server using a P4Config file.</param>
        public P4ServerMT(string cwd)
        {
            this.cwd = cwd;
            _programName = "";
            _programVersion = "";
            _characterSet = "";
            _runCmdTimeout = TimeSpan.Zero;
        }

        public void Dispose()
        {
            // dispose of the P4Servers
            foreach (KeyValuePair<int, P4Server> entry in mapTIDtoServer)
            {
                entry.Value.Dispose();
            }
        }

        /// <summary>
        /// Create/retrieve a P4Server for use in this thread's context
        /// </summary>
        /// <returns>P4Server connection</returns>
        public P4Server getServer()
        {
            return getServerForThread(System.Threading.Thread.CurrentThread.ManagedThreadId);
        }

        // persist some properties  in the P4ServerMT
        // where they will be propagated to threads as they are created.
        private string _programName;
        private string _programVersion;
        private string _characterSet;
        private TimeSpan _runCmdTimeout;

        public string ProgramName
        {
            get => _programName;
            set
            {
                // update each thread associated with this P4ServerMT
                _programName = value;
                lock (dictLock)
                {
                    foreach (KeyValuePair<int, P4Server> entry in mapTIDtoServer)
                    {
                        entry.Value.ProgramName = _programName;
                    }
                }
            }
        }
        
        public string ProgramVersion
        {
            get => _programVersion;
            set
            {
                // update each thread associated with this P4ServerMT
                _programVersion = value;
                lock (dictLock)
                {
                    foreach (KeyValuePair<int, P4Server> entry in mapTIDtoServer)
                    {
                        entry.Value.ProgramVersion = _programVersion;
                    }
                }
            }
        }
        
        public string CharacterSet
        {
            get => _characterSet;
            set 
            {
                // update each thread associated with this P4ServerMT
                _characterSet = value;
                lock (dictLock)
                {
                    foreach (KeyValuePair<int, P4Server> entry in mapTIDtoServer)
                    {
                        entry.Value.CharacterSet = _characterSet;
                    }
                }
            } 
        }
        
        public TimeSpan RunCmdTimeout
        {
            get => _runCmdTimeout;
            set 
            {
                // update each thread associated with this P4ServerMT
                _runCmdTimeout = value;
                lock (dictLock)
                {
                    foreach (KeyValuePair<int, P4Server> entry in mapTIDtoServer)
                    {
                        entry.Value.RunCmdTimeout = _runCmdTimeout;
                    }
                }
            } 
        }


        /// <summary>
        /// Create/retrieve a P4Server for a different thread
        /// </summary>
        /// <param name="threadId">Managed thread ID of the other thread</param>
        /// <returns>P4Server connection</returns>
        public P4Server getServerForThread(int threadId)
        {
            lock (dictLock)
            {
                if (mapTIDtoServer.ContainsKey(threadId))
                {
                    return mapTIDtoServer[threadId];
                }
                // only call the fingerprint constructor if we got configured with a fingerprint
                // otherwise it will not throw the correct "you need to trust this" exception
                // (which seems wrong, both methods should operate similarly))
                P4Server p4server = !string.IsNullOrEmpty(fingerprint) || 
                                    !string.IsNullOrEmpty(trustFlag) ?
                    new P4Server(server, user, pass, ws_client, cwd, trustFlag, fingerprint) :
                    new P4Server(server, user, pass, ws_client, cwd);
                p4server.SetThreadOwner(threadId);
                mapTIDtoServer[threadId] = p4server;
                
                // initialize the new server with current cross-thread shared properties
                if (ProgramName.Length > 0) p4server.ProgramName = ProgramName;
                if (ProgramVersion.Length > 0) p4server.ProgramVersion = ProgramVersion;
                if (CharacterSet.Length > 0) p4server.CharacterSet = CharacterSet;
                if (RunCmdTimeout != TimeSpan.Zero) p4server.RunCmdTimeout = RunCmdTimeout;
                return p4server;
            }
        }
    }
}
