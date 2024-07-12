using System.Collections.Generic;
using System.Linq;

namespace Perforce.P4
{
    public partial class Repository
    {
        /// <summary>
        /// Retrieves Server License related information from the repository.
        /// </summary>
        /// <param name="options">The '-o','-u' and '-L' flags can be used to get Server License information.</param>
        /// <returns>The ServerLicense object with license information.</returns>
        /// <remarks>
        /// <br/><b>p4 help license</b>
        /// <br/> 
        /// <br/>     license -- Update or display the license file
        /// <br/> 
        /// <br/>         p4 license -o
        /// <br/>         p4 license -i
        /// <br/>         p4 license -u
        /// <br/>         p4 license -L
        /// <br/> 
        /// <br/> 	Info lists information about the current client (user name,
        /// <br/> 	Update the Perforce license file.This command requires a valid
        /// <br/> 	license file in the Perforce root directory.Typically this command
        /// <br/> 	lets an administrator add extra licensed users to the Perforce server
        /// <br/> 	without having to shut the server down and copy the license file to
        /// <br/> 	the server root.
        /// <br/> 	
        /// <br/> 	Most new license files obtained from Perforce can be installed with
        /// <br/> 	this command, unless the server's IP address or port has changed.
        /// <br/> 	In that case, stop the server, copy the new license file to the root,
        /// <br/> 	and restart the server.
        /// <br/> 	
        /// <br/> 	The -o flag writes the license file to the standard output.
        /// <br/> 	
        /// <br/> 	The -i flag reads a license file from the standard input.
        /// <br/> 	
        /// <br/> 	The -u flag reports the license limits and how many entities are in
        /// <br/> 	use towards the limits.
        /// <br/> 	
        /// <br/> 	The -L flag lists valid server IP and MAC addresses to be used when
        /// <br/> 	requesting a valid license from Perforce Support.
        /// <br/> 	
        /// <br/> 	This command requires 'super' access (or 'admin' for '-u'),
        /// <br/> 	which is granted by 'p4 protect'.
        /// <br/> 	
        /// <br/> 	**********************************************************************
        /// <br/> 	When using the free version of the server(no license file) the server
        /// <br/> 	is limited to 5 users and 20 workspaces, or unlimited users and
        /// <br/> 	workspaces when the repository has less than 1,000 files
        /// <br/> 	**********************************************************************
        /// </remarks>
        /// <example>
        ///		If you have license installed for your server and want to output information:
        ///		Pre-requisites: You are connected to the server repository.
        ///		<code>
        ///		
        ///		    LicenseCmdOptions options = new LicenseCmdOptions(LicenseCmdFlags.Output);
        ///		    ServerLicense serverLicense = rep.GetServerLicenseInformation(options);
        ///		    
        ///		</code>
        /// </example>
        /// <seealso cref="LicenseCmdFlags"/> 
        public ServerLicense GetServerLicenseInformation(Options options)
        {

            using (P4Command cmd = new P4Command(this, "license", true))
            {
                P4CommandResult results = cmd.Run(options);
                if (results.Success)
                {
                    if ((results.TaggedOutput == null) || (results.TaggedOutput.Count <= 0))
                    {
                        return null;
                    }

                    ServerLicense value = new ServerLicense();

                    List<ServerIPMACaddress> serverIPMACaddresses = new List<ServerIPMACaddress>();

                    foreach (TaggedObject obj in results.TaggedOutput)
                    {
                        if (obj.Count > 0)
                        {
                            if (obj.ContainsKey("interface"))
                            {
                                ServerIPMACaddress serverIPMACaddress = new ServerIPMACaddress();
                                serverIPMACaddress.PopulateInterfaceDetailsFromTaggedOutput(obj);
                                serverIPMACaddresses.Add(serverIPMACaddress);
                            }
                            else
                            {
                                value.FromServerLicenseCmdTaggedOutput(obj);
                            }
                        }
                    }

                    if(serverIPMACaddresses?.Count > 0)
                    {
                        value.ServerIPMACAddresses = serverIPMACaddresses; ;
                    }

                    return value;
                }
                else
                {
                    P4Exception.Throw(results.ErrorList);
                }

                return null;
            }
        }

        /// <summary>Adds/updates license information for Perforce Repository.</summary>
        /// <param name="serverLicense">ServerLicense object along with required license specification properties.</param>
        /// <param name="options">The '-i' flags needs to be used to add/update Server License information.</param>
        /// <returns>Message retrieved from server for add/update operation.</returns>
        /// <remarks>
        /// <br/><b>p4 help license</b>
        /// <br/> 
        /// <br/>     license -- Update or display the license file
        /// <br/> 
        /// <br/>         p4 license -o
        /// <br/>         p4 license -i
        /// <br/>         p4 license -u
        /// <br/>         p4 license -L
        /// <br/> 
        /// <br/> 	Info lists information about the current client (user name,
        /// <br/> 	Update the Perforce license file.This command requires a valid
        /// <br/> 	license file in the Perforce root directory.Typically this command
        /// <br/> 	lets an administrator add extra licensed users to the Perforce server
        /// <br/> 	without having to shut the server down and copy the license file to
        /// <br/> 	the server root.
        /// <br/> 	
        /// <br/> 	Most new license files obtained from Perforce can be installed with
        /// <br/> 	this command, unless the server's IP address or port has changed.
        /// <br/> 	In that case, stop the server, copy the new license file to the root,
        /// <br/> 	and restart the server.
        /// <br/> 	
        /// <br/> 	The -o flag writes the license file to the standard output.
        /// <br/> 	
        /// <br/> 	The -i flag reads a license file from the standard input.
        /// <br/> 	
        /// <br/> 	The -u flag reports the license limits and how many entities are in
        /// <br/> 	use towards the limits.
        /// <br/> 	
        /// <br/> 	The -L flag lists valid server IP and MAC addresses to be used when
        /// <br/> 	requesting a valid license from Perforce Support.
        /// <br/> 	
        /// <br/> 	This command requires 'super' access (or 'admin' for '-u'),
        /// <br/> 	which is granted by 'p4 protect'.
        /// <br/> 	
        /// <br/> 	**********************************************************************
        /// <br/> 	When using the free version of the server(no license file) the server
        /// <br/> 	is limited to 5 users and 20 workspaces, or unlimited users and
        /// <br/> 	workspaces when the repository has less than 1,000 files
        /// <br/> 	**********************************************************************
        /// </remarks>
        /// <example>
        ///		To add license: 
        ///		Pre-requisites: You have license file received from Perforce support.
        ///		                You are connected to the server repository.
        ///		<code>
        ///		    
        ///		    // Read license file content into string and parse the same into serverlicense object.
        ///         LicenseCmdOptions option = new LicenseCmdOptions(LicenseCmdFlags.Input);
        ///         ServerLicense serverLicense = new ServerLicense();
        ///         serverLicense.Parse(serverLicenseSpecification);
        ///         string result = rep.AddOrUpdateServerLicense(serverLicense, options);
        ///		    
        ///		</code>
        /// </example>
        /// <seealso cref="LicenseCmdFlags"/> 
        public string AddOrUpdateServerLicense(ServerLicense serverLicense, Options options)
        {
            using (P4Command cmd = new P4Command(this, "license", true))
            {
                cmd.DataSet = serverLicense.ToString();

                P4CommandResult results = cmd.Run(options);

                if (!results.Success)
                {
                    P4Exception.Throw(results.ErrorList);
                }
                return results?.InfoOutput?.FirstOrDefault()?.Message;
            }
        }
    }
}
