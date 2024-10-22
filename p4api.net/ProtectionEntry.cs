using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perforce.P4
{
	/// <summary>
	/// The protection mode or rights associated with this entry. 
	/// </summary>
	public enum ProtectionMode
	{
		List, Read, Open, Write, Admin, Owner, Super, Review, ReadRights,
		BranchRights, OpenRights, WriteRights, None,
        ReadStreamSpec, OpenStreamSpec, WriteStreamSpec,
        ReadStreamSpecRights, OpenStreamSpecRights, WriteStreamSpecRights
    }

	/// <summary>
	/// The type of protection (user or group). 
	/// </summary>
	public enum EntryType
	{ User, Group }

	/// <summary>
	/// Describes a protection entry (line) in a Perforce protection table. 
	/// </summary>
	public class ProtectionEntry
	{
		public ProtectionEntry(ProtectionMode mode, EntryType type, string name, string host,
            string path, bool unmap)
		{
			Mode = mode;
			Type = type;
			Name = name;
			Host = host;
			Path = path;
            Unmap = unmap;
		}
		public ProtectionMode Mode { get; set; }
		public EntryType Type { get; set; }
		public string Name { get; set; }
		public string Host { get; set; }
		public string Path { get; set; }
        public bool Unmap { get; set; }
	}

    public static class ProtectionEntryExtensions
    {
        /// <summary>
        /// Extension method which returns a string presentation of protection mode,
        /// which can be recognized by the server
        /// e.g. Write is translated to "write" and ReadRights to "=read".
        /// These are server compatible mode representations
        /// </summary>
        /// <example>
        ///		To get string representation of protection mode instance
        ///		<code>
        ///			string readMode = ProtectionMode.Read.ToServerCompatibleString(); // returns "read"
        ///			string writeRightMode = ProtectionMode.WriteRights.ToServerCompatibleString(); // returns "=write"
        ///		</code>
        /// </example>
        public static string ToServerCompatibleString(this ProtectionMode mode)
        {
            return mode.ToString().EndsWith("Rights") ?
                "=" + mode.ToString().ToLower().Substring(0, mode.ToString().Length - 6) :
                mode.ToString().ToLower();
        }

        /// <summary>
        /// Extension method which returns a string presentation of protection entry type,
        /// which can be recognized by the server
        /// e.g. User is translated to "user" and Group to "group".
        /// These are server compatible entry type representations
        /// </summary>
        /// <example>
        ///		To get string representation of protection entry type
        ///		<code>
        ///			string userEntry = EntryType.User.ToServerCompatibleString(); // returns "user"
        ///			string groupEntry = EntryType.Group.ToServerCompatibleString(); // returns "group"
        ///		</code>
        /// </example>
        public static string ToServerCompatibleString(this EntryType type)
        {
            return type.ToString().ToLower();
        }
    }
}
