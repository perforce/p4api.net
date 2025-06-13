using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Perforce.P4
{
    /// <summary>
    /// The address of the Perforce server.
    /// </summary>
    public class ServerAddress
	{
        /// <summary>
        /// Construct ServerAddress from string
        /// </summary>
        /// <param name="uri">string uri  host:port</param>
		public ServerAddress(string uri) { Uri = uri; }

        /// <summary>
        /// Property for the string URI
        /// </summary>
		public string Uri { get; private set; }

        /// <summary>
        /// Compare ServerAddresses
        /// </summary>
        /// <param name="obj">ServerAddress to compare to</param>
        /// <returns>true if both ServerAddresses are equal</returns>
		public override bool Equals(object obj)
		{
			if ((obj is ServerAddress) == false)
			{
				return false;
			}
			ServerAddress o = obj as ServerAddress;

			if (o.Uri != null)
			{
				if (o.Uri.Equals(this.Uri) == false)
				{ return false; }
			}
			else
			{
				if (this.Uri != null)
				{ return false; }
			}
			return true;
		}

        /// <summary>
        /// Get Hashcode
        /// </summary>
        /// <returns>hash code for ServerAddress</returns>
        public override int GetHashCode() { return Uri.GetHashCode(); }
        
        /// <summary>
        /// Convert ServerAddress to String
        /// </summary>
        /// <returns>string URI for ServerAddress</returns>
        public override string ToString()
		{
			return Uri;
		}
	}

	/// <summary>
	/// The Perforce server's version information. 
	/// </summary>
	public class ServerVersion
	{
        /// <summary>
        /// Default Constructor
        /// </summary>
		public ServerVersion()
		{

		}

        /// <summary>
        /// Parameterized Constructor
        /// </summary>
        /// <param name="product">product name</param>
        /// <param name="platform">build platform</param>
        /// <param name="major">major version</param>
        /// <param name="minor">minor version</param>
        /// <param name="date">datetime of release</param>
		public ServerVersion(string product, string platform, string major,
			string minor, DateTime date)
		{
			Product = product;
			Platform = platform;
			Major = major;
			Minor = minor;
			Date = date;
		}

        /// <summary>
        /// Product name
        /// </summary>
		public string Product { get; private set; }

        /// <summary>
        /// Platform name
        /// </summary>
		public string Platform { get; private set; }

        /// <summary>
        /// Major Version
        /// </summary>
		public string Major { get; private set; }

        /// <summary>
        /// Minor Version
        /// </summary>
		public string Minor { get; private set; }

        /// <summary>
        /// Release date
        /// </summary>
		public DateTime Date { get; private set; }
	}

 
    /// <summary>
    /// The Perforce server's license information.
    /// </summary>
    public class ServerLicense 
    {
        public ServerLicense()
        {
        }

        public ServerLicense(int users, DateTime expires)
		{
			Users = users;
			Expires = expires;
		}

        #region License Specification Properties
        /// <summary>
        /// The license key.
        /// </summary>
        public string License {  get; private set; }

        /// <summary>
        /// Date at which the license expires.
        /// </summary>
        public long LicenseExpires { get; private set; }

        /// <summary>
        /// Date at which support expires.
        /// </summary>
        public long SupportExpires { get; private set; }

        /// <summary>
        /// Customer to whom this license is granted.
        /// </summary>
        public string Customer { get; private set; }

        /// <summary>
        /// Application that can use this license.
        /// </summary>
        public string Application { get; private set; }


        /// <summary>
        /// IP/Port address for license.
        /// </summary>
        public string IPaddress { get; private set; }

        /// <summary>
        /// Platform for which license is generated.
        /// </summary>
        public string Platform { get; private set; }

        /// <summary>
        ///  Number of supported clients.
        /// </summary>
        public string Clients { get; private set; }

        /// <summary>
        /// ExtraCapabilities of license.
        /// </summary>
        public IList<string> ExtraCapabilities {  get; private set; }

        private String ExtraCapabilitiesStr
        {
            get
            {
                String value = String.Empty;
                if ((ExtraCapabilities != null) && (ExtraCapabilities.Count > 0))
                {
                    for (int idx = 0; idx < ExtraCapabilities.Count; idx++)
                    {
                        value += ExtraCapabilities[idx] + $"{Environment.NewLine}";
                    }
                }
                return value;
            }
        }

        #endregion License Specification Properties

        public int Users { get; private set; }
        public DateTime Expires { get; private set; }

        #region License -u properties
        /// <summary>
        /// Boolean indicating if Server is licensed or not.
        /// </summary>
        public bool IsLicensed { get; private set; }

        /// <summary>
        /// Number of active clients.
        /// </summary>
        public string ClientCount { get; private set; }

        /// <summary>
        /// Maximum number of clients those can use the license.
        /// </summary>
        public string ClientLimit { get; private set; }

        /// <summary>
        /// Number of files.
        /// </summary>
        public string FileCount { get; private set; }

        /// <summary>
        /// Maximum number of files those can be added under current license.
        /// </summary>
        public string FileLimit {  get; private set; }

        /// <summary>
        /// Active number of users using the license.
        /// </summary>
        public string UserCount {  get; private set; }

        /// <summary>
        /// Maximum number of users those can use the license.
        /// </summary>
        public string UserLimit {  get; private set; }

        /// <summary>
        /// Active number of Repos using the license.
        /// </summary>
        public string RepoCount { get; private set; }

        /// <summary>
        /// Maximum number of Repos allowed under current license.
        /// </summary>
        public string RepoLimit { get; private set; }

        /// <summary>
        /// Remaining time for license expiration.
        /// </summary>
        public long LicenseTimeRemaining {  get; private set; }

        #endregion License -u properties

        #region License -L properties

        public List<ServerIPMACaddress> ServerIPMACAddresses { get;  set; }

        #endregion License -L properties

        // Raw output from command in case fields are added in the future
        public TaggedObject RawData { get; private set; }
       

        private FormBase _baseForm;

        public void FromServerLicenseCmdTaggedOutput(TaggedObject objectInfo)
        {
            RawData = objectInfo;

            #region Populate values from p4 license -o command

            if (objectInfo.ContainsKey("License"))
            {
                License = objectInfo["License"];
            }

            if (objectInfo.ContainsKey("License-Expires"))
            {
                long parsedDate;
                long.TryParse(objectInfo["License-Expires"], out parsedDate);
                LicenseExpires = parsedDate;
            }

            if (objectInfo.ContainsKey("Support-Expires"))
            {
                long parsedDate;
                long.TryParse(objectInfo["Support-Expires"], out parsedDate);
                SupportExpires = parsedDate;
            }

            if (objectInfo.ContainsKey("Customer"))
            {
                Customer = objectInfo["Customer"];
            }

            if (objectInfo.ContainsKey("Application"))
            {
                Customer = objectInfo["Application"];
            }

            if (objectInfo.ContainsKey("IPaddress"))
            {
                IPaddress= objectInfo["IPaddress"];
            }

            if (objectInfo.ContainsKey("Clients"))
            {
                Clients = objectInfo["Clients"];
            }

            if (objectInfo.ContainsKey("Users"))
            {
                int users;
                int.TryParse(objectInfo["Users"], out users);
                Users = users;
            }

            int idx = 0;
            String key = String.Format("ExtraCapabilities{0}", idx);
            if (objectInfo.ContainsKey(key))
            {
                ExtraCapabilities = new List<String>();
                while (objectInfo.ContainsKey(key))
                {
                    ExtraCapabilities.Add(objectInfo[key]);
                    idx++;
                    key = String.Format("ExtraCapabilities{0}", idx);
                }
            }

            #endregion

            #region Populate values from p4 license -u

            if (objectInfo.ContainsKey("isLicensed"))
            {
                IsLicensed = objectInfo["isLicensed"].ToString().Equals("yes",StringComparison.OrdinalIgnoreCase) ? true : false;
            }

            if (objectInfo.ContainsKey("userCount"))
            {
                UserCount = objectInfo["userCount"];
            }

            if (objectInfo.ContainsKey("userLimit"))
            {
                UserLimit = objectInfo["userLimit"];
            }

            if (objectInfo.ContainsKey("clientCount"))
            {
                ClientCount = objectInfo["clientCount"];
            }

            if (objectInfo.ContainsKey("clientLimit"))
            {
                ClientLimit = objectInfo["clientLimit"];
            }


            if (objectInfo.ContainsKey("fileCount"))
            {
                FileCount = objectInfo["fileCount"];
            }

            if (objectInfo.ContainsKey("fileLimit"))
            {
                FileLimit = objectInfo["fileLimit"];
            }

            if (objectInfo.ContainsKey("repoCount"))
            {
                RepoCount = objectInfo["repoCount"];
            }

            if (objectInfo.ContainsKey("repoLimit"))
            {
                RepoLimit = objectInfo["repoLimit"];
            }

            if ( objectInfo.ContainsKey("licenseExpires"))
            {
                long parsedDate;
                long.TryParse(objectInfo["licenseExpires"], out parsedDate);
                LicenseExpires = parsedDate;
            }

            if (objectInfo.ContainsKey("licenseTimeRemaining"))
            {
                long parsedDate;
                long.TryParse(objectInfo["licenseTimeRemaining"], out parsedDate);
                LicenseTimeRemaining = parsedDate;
            }

            if (objectInfo.ContainsKey("supportExpires"))
            {
                long parsedDate;
                long.TryParse(objectInfo["supportExpires"], out parsedDate);
                SupportExpires = parsedDate;
            }


            #endregion

        }

        private static String ServerLicenseSpecFormat =
                                                    "License:\t{0}\n" +
                                                    "\n" +
                                                    "License-Expires:\t{1}\t\n" +
                                                    "\n" +
                                                    "Support-Expires:\t{2}\t\n" +
                                                    "\n" +
                                                    "Customer:\t{3}\n" +
                                                    "\n" +
                                                    "Application:\t{4}\n" +
                                                    "\n" +
                                                    "IPAddress:\t{5}\n" +
                                                    "\n" +
                                                    "Platform:\t{6}\n" +
                                                    "\n" +
                                                    "Clients:\t{7}\n" +
                                                    "\n" +
                                                    "Users:\t{8}\n" +
                                                    "\n" +
                                                    "ExtraCapabilities:\n" +
                                                    "\t{9}\n";



        /// <summary>
        /// Parse a license spec
        /// </summary>
        /// <param name="spec"></param>
        /// <returns>true if parse successful</returns>
        public bool Parse(String spec)
        {
            _baseForm = new FormBase();
            _baseForm.Parse(spec); // parse the values into the underlying dictionary

            if (_baseForm.ContainsKey("License"))
            {
                License = _baseForm["License"] as string;
            }

            if (_baseForm.ContainsKey("License-Expires"))
            {
                string dateTimeString = _baseForm["License-Expires"] as string;
                string[] dateTimeArray = dateTimeString.Split(' ');
                long parsedDate;
                long.TryParse(dateTimeArray[0], out parsedDate);
                LicenseExpires = parsedDate;
            }

            if (_baseForm.ContainsKey("Support-Expires"))
            {
                string dateTimeString = _baseForm["Support-Expires"] as string;
                string[] dateTimeArray = dateTimeString.Split(' ');
                long parsedDate;
                long.TryParse(dateTimeArray[0], out parsedDate);
                SupportExpires = parsedDate;
            }

            if (_baseForm.ContainsKey("Customer"))
            {
                Customer = _baseForm["Customer"] as string;
            }

            if (_baseForm.ContainsKey("Application"))
            {
                Customer = _baseForm["Application"] as string;
            }

            if (_baseForm.ContainsKey("IPaddress"))
            {
                IPaddress = _baseForm["IPaddress"] as string;
            }

            if (_baseForm.ContainsKey("Clients"))
            {
                Clients = _baseForm["Clients"] as string;
            }

            if (_baseForm.ContainsKey("Users"))
            {
                int users;
                int.TryParse(_baseForm["Users"] as string , out users);
                Users = users;
            }

            if (_baseForm.ContainsKey("ExtraCapabilities"))
            {
                if (_baseForm["ExtraCapabilities"] is SimpleList<string>)
                {
                    ExtraCapabilities = (List<string>)((SimpleList<string>)_baseForm["ExtraCapabilities"]);
                }
            }

            return true;
        }

        /// <summary>
        /// Format as a license spec
        /// </summary>
        /// <returns>String description of license file contents</returns>
        override public String ToString()
        {
            String tmpAltRootsStr = String.Empty;
            if (!String.IsNullOrEmpty(ExtraCapabilitiesStr))
            {
                tmpAltRootsStr = FormBase.FormatMultilineField(ExtraCapabilitiesStr.ToString());
            }
            String value = String.Format(ServerLicenseSpecFormat, License,
                LicenseExpires, SupportExpires,
                Customer, Application, IPaddress, Platform,Clients,Users, tmpAltRootsStr);
            return value;
        }
    }

    /// <summary>
    /// The interface information for server license.
    /// </summary>
    public class ServerIPMACaddress
    {
        public string Interface { get; set; }
        public string IPV4Address { get; set; }
        public string IPV6Address { get; set; }
        public string MACAddress { get; set; }
    }

    /// <summary>
    /// Extenstion class for ServerIPMACadress to populate interface information.
    /// </summary>
    static class ServerIPMACadressExtensions

    {
        public static void PopulateInterfaceDetailsFromTaggedOutput(this ServerIPMACaddress serverIPMACadress, TaggedObject objectInfo)
        {
            if (objectInfo != null)
            {
                if (objectInfo.ContainsKey("interface"))
                {
                    serverIPMACadress.Interface = objectInfo["interface"];
                }

                if (objectInfo.ContainsKey("ipv4Address"))
                {
                    serverIPMACadress.IPV4Address = objectInfo["ipv4Address"];
                }

                if (objectInfo.ContainsKey("ipv6Address"))
                {
                    serverIPMACadress.IPV6Address = objectInfo["ipv6Address"];
                }

                if (objectInfo.ContainsKey("macAddress"))
                {
                    serverIPMACadress.MACAddress = objectInfo["macAddress"];
                }

            }

        }
    }

    /// <summary>
    /// Defines useful metadata about a Perforce server.
    /// </summary>
    public class ServerMetaData
	{
		public ServerMetaData()
		{

		}
		public ServerMetaData(	string name,
								ServerAddress address,
								string root,
								DateTime date,
                                string dateTimeOffset,
								int uptime,
								ServerVersion version,
								ServerLicense license,
								string licenseIp,
								bool caseSensitive,
								bool unicodeEnabled,
								bool moveEnabled
							)
		{
			Name = name;
			Address=address;
			Root=root;
			Date=date;
		    DateTimeOffset = dateTimeOffset;
			Uptime=uptime;
			Version=version;
			License=license;
			LicenseIp=licenseIp;
			CaseSensitive=caseSensitive;
			UnicodeEnabled = unicodeEnabled;
			MoveEnabled = moveEnabled;
		}
		public ServerMetaData(ServerAddress address)
		{
			Name = null;
			Address = address;
			Root = null;
			Date = DateTime.Now;
		    DateTimeOffset = null;
			Uptime = -1;
			Version = null;
			License = null;
			LicenseIp = null;
			CaseSensitive = true;
			UnicodeEnabled = false;
			MoveEnabled = true;
		}
		public string Name { get; private set; }
		public ServerAddress Address { get; internal set; }
		public string Root { get; private set; }
		public DateTime Date { get; private set; }
        public string DateTimeOffset { get; private set; }
		public int Uptime { get; private set; }
		public ServerVersion Version { get; private set; }
		public ServerLicense License { get; private set; }
		public string LicenseIp { get; private set; }
		public bool CaseSensitive { get; private set; }
		public bool UnicodeEnabled { get; private set; }
		public bool MoveEnabled { get; private set; }


		// Raw output from command in case fields are added in the future
		public TaggedObject RawData { get; private set; }

		#region fromTaggedOutput
		/// <summary>
		/// Read the fields from the tagged output of an info command
		/// </summary>
		/// <param name="objectInfo">Tagged output from the 'info' command</param>
		public void FromGetServerMetaDataCmdTaggedOutput(TaggedObject objectInfo)
		{
			RawData = objectInfo;

			if (objectInfo.ContainsKey("serverName"))
				Name = objectInfo["serverName"];

			if (objectInfo.ContainsKey("serverAddress"))
				Address = new ServerAddress(objectInfo["serverAddress"]);

			if (objectInfo.ContainsKey("serverRoot"))
				Root = objectInfo["serverRoot"];

            if (objectInfo.ContainsKey("serverDate"))
            {
                string dateTimeString = objectInfo["serverDate"];
                string[] dateTimeArray = dateTimeString.Split(' ');
                DateTime v;
                DateTime.TryParse(dateTimeArray[0] + " " + dateTimeArray[1], out v);
                Date = v;
                for (int idx = 2; idx < dateTimeArray.Count();idx++)
                {
                  DateTimeOffset += dateTimeArray[idx] + " ";
                }
                DateTimeOffset=DateTimeOffset.Trim();
            }

		    if (objectInfo.ContainsKey("serverUptime"))
			{
				int v;
				int.TryParse(objectInfo["serverUptime"], out v);
				Uptime = v;
			}


			if (objectInfo.ContainsKey("serverVersion"))
			{
				string serverVersion = objectInfo["serverVersion"];
				string[] info = serverVersion.Split('/', ' ');
				string product = info[0];
				string platform = info[1];
				string major = info[2];
				string minor = info[3];
                char[] trimChars = { '(', ')' };
                string dateString = info[4] +"-"+ info[5] +"-"+ info[6];
                dateString = dateString.Trim(trimChars);
				DateTime date = new DateTime(1,1,1);
				DateTime.TryParse(dateString, out date);
				Version = new ServerVersion(product,platform, major,minor,date);
			}

			if (objectInfo.ContainsKey("serverLicense"))
			{
				string lic = objectInfo["serverLicense"];
                //Populate license information only if it's available.
                if (!string.IsNullOrEmpty(lic) && !lic.Equals("none"))
                {
                    int users=0;
                    DateTime expires=DateTime.MinValue;

                    // Match number of users and license expiry date using regex since position of these values is not fixed in serverLicense property.
                    Regex usersPattern = new Regex(@"\d+ users", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                    Regex expiryDatePattern = new Regex(@"\(expires (\d{4}/\d{2}/\d{2})\)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

                    Match match = usersPattern.Match(lic);

                    if (match.Success)
                    {
                        int.TryParse(match.Value.Replace(" users", "").Trim(), out users);
                    }

                    Match matchExpiryDate = expiryDatePattern.Match(lic);

                    if (matchExpiryDate.Success)
                    {
                        DateTime.TryParse(matchExpiryDate.Groups[1].Value, out expires);
                    }

                    License = new ServerLicense(users, expires);
                }
            }

			if (objectInfo.ContainsKey("serverLicense-ip"))
				LicenseIp = objectInfo["serverLicense-ip"];

			if (objectInfo.ContainsKey("caseHandling"))
			{
				if (objectInfo["caseHandling"] == "sensitive")
					CaseSensitive = true;
			}


			if (objectInfo.ContainsKey("unicode"))
			{
				if (objectInfo["unicode"] == "enabled")
					UnicodeEnabled = true;
			}

			if (objectInfo.ContainsKey("move"))
			{
				if (objectInfo["move"] == "disabled")
					MoveEnabled = false;
			}
			else
			{
				MoveEnabled = true; ;
			}

			
		}
		#endregion

		/// <summary>
		/// Defines the UTC offset for the server.
		/// </summary>
		public class ServerTimeZone
		{
			public static int UtcOffset()
			{
				return 100;
			}
		}


	}

	/// <summary>
	/// The current state of a specific server.
	/// </summary>
	[Flags]
	public enum ServerState 
	{ 
		/// <summary>
		/// The server is offline.
		/// </summary>
		Offline = 0x000,
		/// <summary>
		/// The server is online.
		/// </summary>
		Online = 0x0001,
		/// <summary>
		/// The state of the server is unknown.
		/// </summary>
		Unknown = 0x0002
	}
	
	/// <summary>
	/// Represents a specific Perforce server. 
	/// </summary>
	public class Server
	{
		public Server(ServerAddress address)
		{
			State = ServerState.Unknown;
			Address = address;
		}
		internal void SetMetadata(ServerMetaData metadata) { Metadata = metadata; }
		internal void SetState(ServerState state) { State = state; }

		/// <summary>
		/// The host:port used to connect to a Perforce server.
		/// </summary>
		/// <remarks>
		/// Note: this can be different than the value returned by the info
		/// command if a proxy or broker is used to make the connection.
		/// </remarks>
		public ServerAddress Address { get; internal set; }

		public ServerState State { get; set; }
		public ServerMetaData Metadata { get; private set; }
	}
}
