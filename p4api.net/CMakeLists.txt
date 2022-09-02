cmake_minimum_required(VERSION 3.17)
project(P4ApiNetLib  VERSION 21.1.0 DESCRIPTION "p4api.net managed library"  LANGUAGES CSharp)

# cmake file for managed P4API.NET

set(CMAKE_VERBOSE_MAKEFILE ON)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)
set(CMAKE_COLOR_MAKEFILE ON)

message( NOTICE "Platform: ${CMAKE_SYSTEM_NAME}" ) # 'Windows'  'Linux'  'Darwin'
message( NOTICE "Version: ${P4ApiNetLib_VERSION}")
message( NOTICE "Build Type: ${CMAKE_BUILD_TYPE}")


if (WIN32)
  set(CMAKE_SHARED_LIBRARY_PREFIX "")
endif()

# Setup project to use C# utilities
INCLUDE(CSharpUtils)


ADD_LIBRARY(P4ApiNetLib SHARED 
	BranchSpec.cs
	BridgeInterfaceClasses.cs
	Changelist.cs
	Client.cs
	ClientMetadata.cs
	Connection.cs
	Counter.cs
	Credential.cs
	Debug.cs
	Depot.cs
	DepotFileDiff.cs
	FileAnnotation.cs
	File.cs
	FileHistory.cs
	FileIntegrationRecord.cs
	FileLineMatch.cs
	FileMetaData.cs
	FileResolveRecord.cs
	FileSpec.cs
	FileType.cs
	Fix.cs
	FormBase.cs
	FormSpec.cs
	Group.cs
	Job.cs
	Label.cs
	LogFile.cs
	Options.cs
	P4Bridge.cs
	P4Command.cs
	P4CommandResult.cs
	P4Exception.cs
	P4MapApi.cs
	P4Server.cs
	P4Server.internal.cs
	PathSpec.cs
	ProtectionEntry.cs
	ProtectionTable.cs
	Repository.BranchSpec.cs
	Repository.Changelist.cs
	Repository.Client.cs
	Repository.ClientMetadata.cs
	Repository.cs
	Repository.Depot.cs
	Repository.Group.cs
	Repository.Job.cs
	Repository.Label.cs
	Repository.ServerMetaData.cs
	Repository.Stream.cs
	Repository.User.cs
	Server.cs
	SimpleList.cs
	Stream.cs
	StreamMetaData.cs
	StringEnum.cs
	StringList.cs
	SubmitResults.cs
	TaggedObject.cs
	TriggerTable.cs
	TypeMap.cs
	User.cs
	VersionSpec.cs
	ViewMap.cs
	)
