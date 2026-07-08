/*******************************************************************************

Copyright (c) 2011, Perforce Software, Inc.  All rights reserved.

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
 * Name		: Group.cs
 *
 * Author	: dbb
 *
 * Description	: Class used to abstract a form specification in Perforce.
 *
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Perforce.P4
{
    /// <summary>
    /// A user group on a P4 Server, represented by a P4 Group spec.
    /// </summary>
    /// <remarks>
    /// <br/><b>p4 help group</b>
    /// <br/> 
    /// <br/>     group -- Change members of user group
    /// <br/> 
    /// <br/>     p4 group [-a|-A] name
    /// <br/>     p4 group -d [-a | -F] name
    /// <br/>     p4 group -o name
    /// <br/>     p4 group -i [-a|-A]
    /// <br/>
    /// <br/> 	Create a group or modify the membership of an existing group.
    /// <br/> 	A group can contain users and other groups. The group specification
    /// <br/> 	is put into a temporary file and the editor (configured by the
    /// <br/> 	environment variable $P4EDITOR) is invoked.
    /// <br/>
    /// <br/> 	A group exists when it has any users or other groups in it, and
    /// <br/> 	ceases to exist if all users and groups in it are removed.
    /// <br/>
    /// <br/> 	Each group has MaxResults, MaxScanRows, MaxLockTime, MaxOpenFiles
    /// <br/> 	and MaxMemory fields, which limit the resources committed to
    /// <br/> 	operations performed by members of the group.  See
    /// <br/> 	'p4 help maxresults' for more information on MaxResults, MaxScanRows,
    /// <br/> 	MaxLockTime, MaxOpenFiles and MaxMemory.
    /// <br/>
    /// <br/> 	Each group also has a Timeout field, which specifies how long (in
    /// <br/> 	seconds)  a 'p4 login' ticket remains valid.  A value of 'unset' or
    /// <br/> 	'unlimited' is equivalent to no timeout. An individual's timeout is
    /// <br/> 	the highest of any group with a limit to which he belongs, unlimited
    /// <br/> 	if any of his groups has 'unlimited' for the timeout value, or
    /// <br/> 	unlimited if he belongs to no group with a limit. See 'p4 help login'
    /// <br/> 	for more information.
    /// <br/>
    /// <br/> 	Each group has a PasswordTimeout field, which determines how long a
    /// <br/> 	password remains valid for members of the group.
    /// <br/>
    /// <br/> 	Each group also has an IdleTimeout field, which specifies how long
    /// <br/> 	(in seconds) after the last command a login ticket is invalidated.
    /// <br/> 	A value of 'unset' or 'unlimited' means no idle timeout. An
    /// <br/> 	individual's idle timeout is the highest of any group with a limit
    /// <br/> 	to which he belongs, unlimited if any of his groups has 'unlimited'
    /// <br/> 	for the idle timeout value, or unlimited if he belongs to no group
    /// <br/> 	with a limit. If the value is lower than dm.user.accessupdate or
    /// <br/> 	dm.user.accessforce, the highest of those is used instead.
    /// <br/>
    /// <br/> 	A group may be synchronized with an LDAP group by setting the three
    /// <br/> 	fields: LdapConfig, LdapSearchQuery and LdapUserAttribute. This takes
    /// <br/> 	the LDAP configuration (see 'p4 ldap') specified by LdapConfig and uses
    /// <br/> 	it to execute the query stored by LdapSearchQuery. The LDAP attribute
    /// <br/> 	specified by LdapUserAttribute is taken to be user's username and is
    /// <br/> 	added to the group's user list. At least one group owner must be set if
    /// <br/> 	these LDAP fields are used. If the LDAP server requires login for
    /// <br/> 	read-only queries, then the LDAP configuration must contain valid bind
    /// <br/> 	credentials in the LDAP spec's SearchBindDN and SearchPasswd fields
    /// <br/> 
    /// <br/> 	The -d flag deletes a group.
    /// <br/> 
    /// <br/> 	The -o flag writes the group specification to standard output. The
    /// <br/> 	user's editor is not invoked.
    /// <br/> 
    /// <br/> 	The -i flag reads a group specification from standard input. The
    /// <br/> 	user's editor is not invoked.  The new group specification replaces
    /// <br/> 	the previous one.
    /// <br/> 
    /// <br/> 	The -a flag enables a user without 'super' access to modify the group
    /// <br/> 	if that user is an 'owner' of that group. Group owners	are specified
    /// <br/> 	in the 'Owners' field of the group spec.
    /// <br/> 
    /// <br/> 	The -A flag enables a user with 'admin' access to add a new group.
    /// <br/> 	Existing groups may not be modified when this flag is used.
    /// <br/>
    /// <br/> 	The -F flag, used only with -d, forces the deletion of the
    /// <br/> 	specified group, and also removes the group from the protections
    /// <br/> 	table and from all groups.
    /// <br/>
    /// <br/> 	All commands that require access granted by 'p4 protect' consider a
    /// <br/> 	user's groups when calculating access levels.
    /// <br/>
    /// <br/> 	'p4 group' requires 'super' access granted by 'p4 protect' unless
    /// <br/> 	invoked with the '-a' or '-A' flag by a qualified user.
    /// <br/>
    /// <br/>
    /// </remarks>
    /// <summary>
    /// A group specification in a Perforce repository.
    /// </summary>
    public class Group
    {
        private FormBase _baseForm;

        #region properties
        /// <summary>
        /// The name of the group.
        /// </summary>
        public string Id { get; internal set; }
        /// <summary>
        /// A description about this group
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Limits the rows (unless 'unlimited' or 'unset') any one
        /// operation can return to the client.
        /// Use 0 for unset (field omitted from spec), -1 for unlimited, or a positive integer.
        /// </summary>
        public int MaxResults { get; set; }
        /// <summary>
        /// Limits the rows (unless 'unlimited' or 'unset') any one
        /// operation can scan from any one database table.
        /// Use 0 for unset (field omitted from spec), -1 for unlimited, or a positive integer.
        /// </summary>
        public int MaxScanRows { get; set; }
        /// <summary>
        /// Limits the time (in milliseconds, unless 'unlimited' or
        /// 'unset') any one operation can lock any database table when
        /// scanning data.
        /// Use 0 for unset (field omitted from spec), -1 for unlimited, or a positive integer.
        /// </summary>
        public int MaxLockTime { get; set; }
        /// <summary>
        /// The maximum number of files that a member of a group
        /// can open using a single command (unless 'unlimited' or 'unset').
        /// Use 0 for unset (field omitted from spec), -1 for unlimited, or a positive integer.
        /// </summary>
        public int MaxOpenFiles { get; set; }
        /// <summary>
        /// Maximum amount of megabytes of random-access memory
        /// that a command can use when run by any member of this group
        /// (unless 'unlimited' or 'unset').
        /// Use 0 for unset (field omitted from spec), -1 for unlimited, or a positive integer.
        /// </summary>
        public int MaxMemory { get; set; }
        /// <summary>
        /// A time (in seconds) which determines how long a 'p4 login'
        /// session ticket remains valid (default is 12 hours).
        /// Use 0 for unset (field omitted from spec), -1 for unlimited, or a positive integer for seconds.
        /// </summary>
        public int TimeOut { get; set; }

        /// <summary>
        /// A time (in seconds, unless 'unlimited' or 'unset')
        /// Specifies how long (in seconds) after the last command a login ticket is invalidated.
        /// Use 0 for unset (field omitted from spec), -1 for unlimited, or a positive integer for seconds.
        /// A user's effective timeout is the highest limit across their groups;
        /// unlimited if any group is unlimited or if none of their groups has a limit.
        /// If the value is lower than dm.user.accessupdate or dm.user.accessforce, the highest of those is used instead.
        /// </summary>
        public int IdleTimeout { get; set; }

        /// <summary>
        /// A time (in seconds) which determines how long a 'p4 password'
        /// password remains valid (default is unset).
        /// Use 0 for unset (field omitted from spec), -1 for unlimited, or a positive integer for seconds.
        /// </summary>
        public int PasswordTimeout { get; set; }
        /// <summary>
        /// Users allowed to change this group without requiring super
        /// access permission.
        /// </summary>
        public IList<string> OwnerNames { get; set; }
        /// <summary>
        /// The users in the group.
        /// </summary>
        public IList<string> UserNames { get; set; }
        /// <summary>
        /// Other groups automatically included in this group.
        /// </summary>
        public IList<string> SubGroups { get; set; }

        public FormSpec Spec { get; set; }

        #endregion

        #region creators

        internal Group()
        {
        }
        /// <summary>
        /// Create a group supplying the name.
        /// </summary>
        /// <param name="id"></param>
        public Group(string id)
        {
            Id = id;
            Description = string.Empty;
            MaxResults = -1;
            MaxScanRows = -1;
            MaxLockTime = -1;
            MaxOpenFiles = -1;
            MaxMemory = -1;
            TimeOut = 43200;
            IdleTimeout = 0;
            PasswordTimeout = 0;
            OwnerNames = null;
            UserNames = null;
            SubGroups = null;
        }
        /// <summary>
        /// Create a group providing all of the properties
        /// </summary>
        /// <param name="id"></param>
        /// <param name="maxResults"></param>
        /// <param name="maxScanRows"></param>
        /// <param name="maxLockTime"></param>
        /// <param name="maxOpenFiles"></param>
        /// <param name="timeOut"></param>
        /// <param name="passwordTimeout"></param>
        /// <param name="ownerNames"></param>
        /// <param name="userNames"></param>
        /// <param name="subGroups"></param>
        /// <param name="spec"></param>
        [ObsoleteAttribute("Use Group(string id, string description, int maxResults, int maxScanRows, int maxLockTime, int maxOpenFiles, int maxMemory, int timeOut, int passwordTimeout, IList<string> ownerNames, IList<string> userNames, IList<string> subGroups, FormSpec spec)", false)]
        public Group(string id,
                        int maxResults,
                        int maxScanRows,
                        int maxLockTime,
                        int maxOpenFiles,
                        int timeOut,
                        int passwordTimeout,
                        IList<string> ownerNames,
                        IList<string> userNames,
                        IList<string> subGroups,
                        FormSpec spec)
        {
            Id = id;
            MaxResults = maxResults;
            MaxScanRows = maxScanRows;
            MaxLockTime = maxLockTime;
            MaxOpenFiles = maxOpenFiles;
            TimeOut = timeOut;
            PasswordTimeout = passwordTimeout;
            OwnerNames = ownerNames;
            UserNames = userNames;
            SubGroups = subGroups;
            Spec = spec;
        }

		public Group(string id,
                        string description,
                        int maxResults,
                        int maxScanRows,
                        int maxLockTime,
                        int maxOpenFiles,
                        int maxMemory,
                        int timeOut,
                        int idleTimeout,
                        int passwordTimeout,
                        IList<string> ownerNames,
                        IList<string> userNames,
                        IList<string> subGroups,
                        FormSpec spec)
        {
            Id = id;
            Description = description;
            MaxResults = maxResults;
            MaxScanRows = maxScanRows;
            MaxLockTime = maxLockTime;
            MaxOpenFiles = maxOpenFiles;
            MaxMemory = maxMemory;
            TimeOut = timeOut;
            IdleTimeout = idleTimeout;
            PasswordTimeout = passwordTimeout;
            OwnerNames = ownerNames;
            UserNames = userNames;
            SubGroups = subGroups;
            Spec = spec;
        }

        /// <summary>
        /// Create a group providing all of the properties
        /// </summary>
        /// <param name="id"></param>
        /// <param name="maxResults"></param>
        /// <param name="maxScanRows"></param>
        /// <param name="maxLockTime"></param>		
        /// <param name="timeOut"></param>
        /// <param name="passwordTimeout"></param>
        /// <param name="ownerNames"></param>
        /// <param name="userNames"></param>
        /// <param name="subGroups"></param>
        /// <param name="spec"></param>
        [ObsoleteAttribute("Use Group(string id, string description, int maxResults, int maxScanRows, int maxLockTime, int maxOpenFiles, int maxMemory, int timeOut, int passwordTimeout, IList<string> ownerNames, IList<string> userNames, IList<string> subGroups, FormSpec spec)", false)]
        public Group(string id,
                        int maxResults,
                        int maxScanRows,
                        int maxLockTime,
                        int timeOut,
                        int passwordTimeout,
                        IList<string> ownerNames,
                        IList<string> userNames,
                        IList<string> subGroups,
                        FormSpec spec)
        {
            Id = id;
            MaxResults = maxResults;
            MaxScanRows = maxScanRows;
            MaxLockTime = maxLockTime;
            TimeOut = timeOut;
            PasswordTimeout = passwordTimeout;
            OwnerNames = ownerNames;
            UserNames = userNames;
            SubGroups = subGroups;
            Spec = spec;
        }
        #endregion

        #region initializers

        /// <summary>
        /// Read the fields from the tagged output of a group command
        /// </summary>
        /// <param name="objectInfo">Tagged output from the 'user' command</param>
        public void FromGroupCmdTaggedOutput(TaggedObject objectInfo)
        {
            _baseForm = new FormBase();

            _baseForm.SetValues(objectInfo);

            if (objectInfo.ContainsKey("Group"))
                Id = objectInfo["Group"];

            if (objectInfo.ContainsKey("Description"))
                Description = objectInfo["Description"];

            if (objectInfo.ContainsKey("MaxResults"))
            {
                var s = objectInfo["MaxResults"];
                if (string.Equals(s, "unlimited", StringComparison.OrdinalIgnoreCase))
                    MaxResults = -1;
                else if (string.Equals(s, "unset", StringComparison.OrdinalIgnoreCase))
                    MaxResults = 0;
                else
                    MaxResults = int.TryParse(s, out int v) ? v : 0;
            }

            if (objectInfo.ContainsKey("MaxScanRows"))
            {
                var s = objectInfo["MaxScanRows"];
                if (string.Equals(s, "unlimited", StringComparison.OrdinalIgnoreCase))
                    MaxScanRows = -1;
                else if (string.Equals(s, "unset", StringComparison.OrdinalIgnoreCase))
                    MaxScanRows = 0;
                else
                    MaxScanRows = int.TryParse(s, out int v) ? v : 0;
            }

            if (objectInfo.ContainsKey("MaxLockTime"))
            {
                var s = objectInfo["MaxLockTime"];
                if (string.Equals(s, "unlimited", StringComparison.OrdinalIgnoreCase))
                    MaxLockTime = -1;
                else if (string.Equals(s, "unset", StringComparison.OrdinalIgnoreCase))
                    MaxLockTime = 0;
                else
                    MaxLockTime = int.TryParse(s, out int v) ? v : 0;
            }

            if (objectInfo.ContainsKey("MaxOpenFiles"))
            {
                var s = objectInfo["MaxOpenFiles"];
                if (string.Equals(s, "unlimited", StringComparison.OrdinalIgnoreCase))
                    MaxOpenFiles = -1;
                else if (string.Equals(s, "unset", StringComparison.OrdinalIgnoreCase))
                    MaxOpenFiles = 0;
                else
                    MaxOpenFiles = int.TryParse(s, out int v) ? v : 0;
            }

            if (objectInfo.ContainsKey("MaxMemory"))
            {
                var s = objectInfo["MaxMemory"];
                if (string.Equals(s, "unlimited", StringComparison.OrdinalIgnoreCase))
                    MaxMemory = -1;
                else if (string.Equals(s, "unset", StringComparison.OrdinalIgnoreCase))
                    MaxMemory = 0;
                else
                    MaxMemory = int.TryParse(s, out int v) ? v : 0;
            }

            if (objectInfo.ContainsKey("Timeout"))
            {
                var s = objectInfo["Timeout"];
                if (string.Equals(s, "unlimited", StringComparison.OrdinalIgnoreCase))
                    TimeOut = -1;
                else if (string.Equals(s, "unset", StringComparison.OrdinalIgnoreCase))
                    TimeOut = 0;
                else
                    TimeOut = int.TryParse(s, out int v) ? v : 0;
            }

            if (objectInfo.ContainsKey("IdleTimeout"))
            {
                var s = objectInfo["IdleTimeout"];
                if (string.Equals(s, "unlimited", StringComparison.OrdinalIgnoreCase))
                    IdleTimeout = -1;
                else if (string.Equals(s, "unset", StringComparison.OrdinalIgnoreCase))
                    IdleTimeout = 0;
                else
                    IdleTimeout = int.TryParse(s, out int v) ? v : 0;
            }

            if (objectInfo.ContainsKey("PasswordTimeout"))
            {
                var s = objectInfo["PasswordTimeout"];
                if (string.Equals(s, "unlimited", StringComparison.OrdinalIgnoreCase))
                    PasswordTimeout = -1;
                else if (string.Equals(s, "unset", StringComparison.OrdinalIgnoreCase))
                    PasswordTimeout = 0;
                else
                    PasswordTimeout = int.TryParse(s, out int v) ? v : 0;
            }
            String key = "Users0";
            if (objectInfo.ContainsKey(key))
            {
                int idx = 1;
                UserNames = new StringList();
                while (objectInfo.ContainsKey(key))
                {
                    UserNames.Add(objectInfo[key]);
                    key = String.Format("Users{0}", idx++);
                }
            }
            key = "Owners0";
            if (objectInfo.ContainsKey(key))
            {
                int idx = 1;
                OwnerNames = new StringList();
                while (objectInfo.ContainsKey(key))
                {
                    OwnerNames.Add(objectInfo[key]);
                    key = String.Format("Owners{0}", idx++);
                }
            }
            key = "Subgroups0";
            if (objectInfo.ContainsKey(key))
            {
                int idx = 1;
                SubGroups = new StringList();
                while (objectInfo.ContainsKey(key))
                {
                    SubGroups.Add(objectInfo[key]);
                    key = String.Format("Subgroups{0}", idx++);
                }
            }
        }

        /// <summary>
        /// Parse the fields from a group specification 
        /// </summary>
        /// <param name="spec">Text of group user specification in server format</param>
        /// <returns></returns>
        /// <remarks>
        /// # A Perforce Group Specification.
        /// #
        /// #  Group:       The name of the group.
        /// #  MaxResults:  Limits the rows (unless 'unlimited' or 'unset') any one
        /// #               operation can return to the client.
        /// #               See 'p4 help maxresults'.
        /// #  MaxScanRows: Limits the rows (unless 'unlimited' or 'unset') any one
        /// #               operation can scan from any one database table.
        /// #               See 'p4 help maxresults'.
        /// #  MaxLockTime: Limits the time (in milliseconds, unless 'unlimited' or
        /// #               'unset') any one operation can lock any database table when
        /// #               scanning data. See 'p4 help maxresults'.
        /// #  Timeout:     A time (in seconds, unless 'unlimited' or 'unset')
        /// #               which determines how long a 'p4 login'
        /// #               session ticket remains valid (default is 12 hours).
        /// #  IdleTimeout: A time (in seconds, unless 'unlimited' or 'unset')
        /// #               which specifies how long after the last command a
        /// #               login ticket is invalidated (default is unset).
        /// #  PasswordTimeout:
        /// #               A time (in seconds, unless 'unlimited' or 'unset')
        /// #               which determines how long a 'p4 password'
        /// #               password remains valid (default is unset).
        /// #  Subgroups:   Other groups automatically included in this group.
        /// #  Owners:      Users allowed to change this group without requiring super
        /// #               access permission.
        /// #  Users:       The users in the group.  One per line.
        /// </remarks>
        public bool Parse(String spec)
        {
            _baseForm = new FormBase();

            _baseForm.Parse(spec); // parse the values into the underlying dictionary

            if (_baseForm.ContainsKey("Group"))
                Id = _baseForm["Group"] as string;

            if (_baseForm.ContainsKey("Description"))
                Description = _baseForm["Description"] as string;

            if (_baseForm.ContainsKey("MaxResults"))
            {
                var s = _baseForm["MaxResults"] as string;
                if (string.Equals(s, "unlimited", StringComparison.OrdinalIgnoreCase))
                    MaxResults = -1;
                else if (string.Equals(s, "unset", StringComparison.OrdinalIgnoreCase))
                    MaxResults = 0;
                else
                    MaxResults = int.TryParse(s, out int v) ? v : 0;
            }

            if (_baseForm.ContainsKey("MaxScanRows"))
            {
                var s = _baseForm["MaxScanRows"] as string;
                if (string.Equals(s, "unlimited", StringComparison.OrdinalIgnoreCase))
                    MaxScanRows = -1;
                else if (string.Equals(s, "unset", StringComparison.OrdinalIgnoreCase))
                    MaxScanRows = 0;
                else
                    MaxScanRows = int.TryParse(s, out int v) ? v : 0;
            }

            if (_baseForm.ContainsKey("MaxLockTime"))
            {
                var s = _baseForm["MaxLockTime"] as string;
                if (string.Equals(s, "unlimited", StringComparison.OrdinalIgnoreCase))
                    MaxLockTime = -1;
                else if (string.Equals(s, "unset", StringComparison.OrdinalIgnoreCase))
                    MaxLockTime = 0;
                else
                    MaxLockTime = int.TryParse(s, out int v) ? v : 0;
            }

            if (_baseForm.ContainsKey("MaxOpenFiles"))
            {
                var s = _baseForm["MaxOpenFiles"] as string;
                if (string.Equals(s, "unlimited", StringComparison.OrdinalIgnoreCase))
                    MaxOpenFiles = -1;
                else if (string.Equals(s, "unset", StringComparison.OrdinalIgnoreCase))
                    MaxOpenFiles = 0;
                else
                    MaxOpenFiles = int.TryParse(s, out int v) ? v : 0;
            }

            if (_baseForm.ContainsKey("MaxMemory"))
            {
                var s = _baseForm["MaxMemory"] as string;
                if (string.Equals(s, "unlimited", StringComparison.OrdinalIgnoreCase))
                    MaxMemory = -1;
                else if (string.Equals(s, "unset", StringComparison.OrdinalIgnoreCase))
                    MaxMemory = 0;
                else
                    MaxMemory = int.TryParse(s, out int v) ? v : 0;
            }

            if (_baseForm.ContainsKey("Timeout"))
            {
                var s = _baseForm["Timeout"] as string;
                if (string.Equals(s, "unlimited", StringComparison.OrdinalIgnoreCase))
                    TimeOut = -1;
                else if (string.Equals(s, "unset", StringComparison.OrdinalIgnoreCase))
                    TimeOut = 0;
                else
                    TimeOut = int.TryParse(s, out int v) ? v : 0;
            }

            if (_baseForm.ContainsKey("IdleTimeout"))
            {
                var s = _baseForm["IdleTimeout"] as string;
                if (string.Equals(s, "unlimited", StringComparison.OrdinalIgnoreCase))
                    IdleTimeout = -1;
                else if (string.Equals(s, "unset", StringComparison.OrdinalIgnoreCase))
                    IdleTimeout = 0;
                else
                    IdleTimeout = int.TryParse(s, out int v) ? v : 0;
            }

            if (_baseForm.ContainsKey("PasswordTimeout"))
            {
                var s = _baseForm["PasswordTimeout"] as string;
                if (string.Equals(s, "unlimited", StringComparison.OrdinalIgnoreCase))
                    PasswordTimeout = -1;
                else if (string.Equals(s, "unset", StringComparison.OrdinalIgnoreCase))
                    PasswordTimeout = 0;
                else
                    PasswordTimeout = int.TryParse(s, out int v) ? v : 0;
            }
            if (_baseForm.ContainsKey("Users"))
            {
                if (_baseForm["Users"] is IList<string>)
                {
                    UserNames = _baseForm["Users"] as IList<string>;
                }
                else if (_baseForm["Users"] is SimpleList<string>)
                {
                    UserNames = (List<string>)((SimpleList<string>)_baseForm["Users"]);
                }
            }
            if (_baseForm.ContainsKey("Owners"))
            {
                if (_baseForm["Owners"] is IList<string>)
                {
                    OwnerNames = _baseForm["Owners"] as IList<string>;
                }
                else if (_baseForm["Owners"] is SimpleList<string>)
                {
                    OwnerNames = (List<string>)((SimpleList<string>)_baseForm["Owners"]);
                }
            }
            if (_baseForm.ContainsKey("Subgroups"))
            {
                if (_baseForm["Subgroups"] is IList<string>)
                {
                    SubGroups = _baseForm["Subgroups"] as IList<string>;
                }
                else if (_baseForm["Subgroups"] is SimpleList<string>)
                {
                    SubGroups = (List<string>)((SimpleList<string>)_baseForm["Subgroups"]);
                }
            }
            return true;
        }
        #endregion

        /// <summary>
        /// Format of a user specification used to save a user to the server
        /// </summary>
        private static String GroupSpecFormat =
            $"Group:\t{{0}}{Environment.NewLine}" +
            $"{Environment.NewLine}" +
            $"MaxResults:\t{{1}}{Environment.NewLine}" +
            $"{Environment.NewLine}" +
            $"MaxScanRows:\t{{2}}{Environment.NewLine}" +
            $"{Environment.NewLine}" +
            $"MaxLockTime:\t{{3}}{Environment.NewLine}" +
            $"{Environment.NewLine}" +
            $"MaxOpenFiles:\t{{4}}{Environment.NewLine}" +
            $"{Environment.NewLine}" +
            $"Timeout:\t{{5}}{Environment.NewLine}" +
            $"{Environment.NewLine}" +
            $"{{6}}" +
            $"{{7}}" +
            $"Subgroups:{Environment.NewLine}{{8}}" +
            $"{Environment.NewLine}" +
            $"Owners:{Environment.NewLine}{{9}}" +
            $"{Environment.NewLine}" +
            $"Users:{Environment.NewLine}{{10}}";

        private static String GroupSpecFormat55 =
            $"{Environment.NewLine}" +
            $"Description:\t{{11}}{Environment.NewLine}" +
            $"{Environment.NewLine}" +
            $"MaxMemory:\t{{12}}{Environment.NewLine}";

        /// <summary>
        /// Convert to specification in server format
        /// </summary>
        /// <returns>The specification for the group</returns>
        override public String ToString()
        {
            return this.ToString(0);
        }

        /// <summary>
        /// Convert to specification in server format
        /// </summary>
        /// <param name="apiLevel">Server protocol</param>
        /// <returns>The specification for the group</returns>
        public String ToString(int apiLevel)
        {
            String subgroupsView = String.Empty;
            if (SubGroups != null && SubGroups.Count > 0)
            {
                StringBuilder sb = new StringBuilder(SubGroups.Count * 20);
                foreach (string subGroup in SubGroups)
                {
                    sb.Append('\t').Append(subGroup).Append(Environment.NewLine);
                }
                subgroupsView = sb.ToString();
            }
            String ownersView = String.Empty;
            if (OwnerNames != null && OwnerNames.Count > 0)
            {
                StringBuilder sb = new StringBuilder(OwnerNames.Count * 20);
                foreach (string owner in OwnerNames)
                {
                    sb.Append('\t').Append(owner).Append(Environment.NewLine);
                }
                ownersView = sb.ToString();
            }
            String usersView = String.Empty;
            if (UserNames != null && UserNames.Count > 0)
            {
                StringBuilder sb = new StringBuilder(UserNames.Count * 20);
                foreach (string user in UserNames)
                {
                    sb.Append('\t').Append(user).Append(Environment.NewLine);
                }
                usersView = sb.ToString();
            }

            // Server protocol for release 2022.2 is 55
            var groupSpecFormat = apiLevel >= 55 || apiLevel == 0 ?
                GroupSpecFormat + GroupSpecFormat55 :
                GroupSpecFormat;

            String value = String.Format(groupSpecFormat,
                Id,
                MaxResults == -1 ? "unlimited" : MaxResults == 0 ? string.Empty : MaxResults.ToString(),
                MaxScanRows == -1 ? "unlimited" : MaxScanRows == 0 ? string.Empty : MaxScanRows.ToString(),
                MaxLockTime == -1 ? "unlimited" : MaxLockTime == 0 ? string.Empty : MaxLockTime.ToString(),
                MaxOpenFiles == -1 ? "unlimited" : MaxOpenFiles == 0 ? string.Empty : MaxOpenFiles.ToString(),
                TimeOut == -1 ? "unlimited" : TimeOut == 0 ? "unset" : TimeOut.ToString(),
                IdleTimeout == -1 ? $"IdleTimeout:\tunlimited{Environment.NewLine}{Environment.NewLine}" :
                    IdleTimeout == 0 ? $"IdleTimeout:\tunset{Environment.NewLine}{Environment.NewLine}" :
                    $"IdleTimeout:\t{IdleTimeout}{Environment.NewLine}{Environment.NewLine}",
                PasswordTimeout == -1 ? $"PasswordTimeout:\tunlimited{Environment.NewLine}{Environment.NewLine}" :
                    PasswordTimeout == 0 ? $"PasswordTimeout:\tunset{Environment.NewLine}{Environment.NewLine}" :
                    $"PasswordTimeout:\t{PasswordTimeout}{Environment.NewLine}{Environment.NewLine}",
                subgroupsView,
                ownersView,
                usersView,
                Description,
                MaxMemory == -1 ? "unlimited" : MaxMemory == 0 ? string.Empty : MaxMemory.ToString());

            return value;
        }
    }
}
