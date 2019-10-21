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
 * Name		: Stream.cs
 *
 * Author	: wjb
 *
 * Description	: Class used to abstract a stream in Perforce.
 *
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perforce.P4
{
	/// <summary>
	/// A stream specification in a Perforce repository. 
	/// </summary>
	public class Stream
	{
		public Stream() { _type = "development"; }

		public Stream(
			string id,
			DateTime updated,
			DateTime accessed,
			string ownername,
			string name,
			PathSpec parent,
			PathSpec baseparent,
			StreamType type,
			string description,
			StreamOption options,
			string firmerthanparent,
			string changeflowstoparent,
			string changeflowsfromparent,
			ViewMap paths,
			ViewMap remapped,
			ViewMap ignored,
            ViewMap view,
			FormSpec spec)
		{
			Id = id;
			Updated = updated;
			Accessed = accessed;
			OwnerName = ownername;
			Name = name;
			Parent = parent;
			BaseParent = baseparent;
			Type = type;
			Description = description;
			Options = options;
			FirmerThanParent = firmerthanparent;
			ChangeFlowsToParent = changeflowstoparent;
			ChangeFlowsFromParent = changeflowsfromparent;
			Paths = paths;
			Remapped = remapped;
			Ignored = ignored;
            View = view;
			Spec = spec;
		}

		private FormBase _baseForm;

		#region properties

		public string Id { get; set; }
		public DateTime Updated { get; set; }
		public DateTime Accessed { get; set; }
		public string OwnerName { get; set; }
		public string Name { get; set; }
		public PathSpec Parent { get; set; }
		public PathSpec BaseParent { get; set; }

		private StringEnum<StreamType> _type;
		public StreamType Type
		{
			get { return _type; }
			set { _type = value; }
		}

		public string Description { get; set; }
		public string FirmerThanParent { get; set; }
		public string ChangeFlowsToParent { get; set; }
		public string ChangeFlowsFromParent { get; set; }

		private StreamOptionEnum _options;
		public StreamOption Options
		{
			get { return _options; }
			set { _options = (StreamOptionEnum)value; }
		}

 		public ViewMap Paths { get; set; }
		public ViewMap Remapped { get; set; }
		public ViewMap Ignored { get; set; }
        public ViewMap View { get; private set; }
		public FormSpec Spec { get; set; }


        #endregion

        /// <summary>
        /// Read the fields from the tagged output of a 'streams' command
        /// </summary>
        /// <param name="objectInfo">Tagged output from the 'streams' command</param>
		/// <param name="offset">Date processing</param>
        /// <param name="dst_mismatch">DST for date</param>
        public void FromStreamsCmdTaggedOutput(TaggedObject objectInfo, string offset, bool dst_mismatch)
		{
			_baseForm = new FormBase();

			_baseForm.SetValues(objectInfo);

			if (objectInfo.ContainsKey("Stream"))
			{
				Id = objectInfo["Stream"];
			}

            if (objectInfo.ContainsKey("Update"))
            {
                DateTime UTC = FormBase.ConvertUnixTime(objectInfo["Update"]);
                DateTime GMT = new DateTime(UTC.Year, UTC.Month, UTC.Day, UTC.Hour, UTC.Minute, UTC.Second,
                    DateTimeKind.Unspecified);
                Updated = FormBase.ConvertFromUTC(GMT, offset, dst_mismatch);
            }

			if (objectInfo.ContainsKey("Access"))
			{
                DateTime UTC = FormBase.ConvertUnixTime(objectInfo["Access"]);
                DateTime GMT = new DateTime(UTC.Year, UTC.Month, UTC.Day, UTC.Hour, UTC.Minute, UTC.Second,
                    DateTimeKind.Unspecified);
                Accessed = FormBase.ConvertFromUTC(GMT, offset, dst_mismatch);
			}

			if (objectInfo.ContainsKey("Owner"))
			{
				OwnerName = objectInfo["Owner"];
			}

			if (objectInfo.ContainsKey("Name"))
			{
				Name = objectInfo["Name"];
			}

			if (objectInfo.ContainsKey("Parent"))
			{
				Parent = new DepotPath(objectInfo["Parent"]);
			}

			if (objectInfo.ContainsKey("baseParent"))
			{
				BaseParent = new DepotPath(objectInfo["baseParent"]);
			}

            if (objectInfo.ContainsKey("Type"))
            {
                _type = (objectInfo["Type"]);
            }

			if (objectInfo.ContainsKey("desc"))
			{
                Description = objectInfo["desc"];
			}

			if (objectInfo.ContainsKey("Options"))
			{
				String optionsStr = objectInfo["Options"];
				_options = optionsStr;
			}

			if (objectInfo.ContainsKey("firmerThanParent"))
			{
				FirmerThanParent = objectInfo["firmerThanParent"];
			}

			if (objectInfo.ContainsKey("changeFlowsToParent"))
			{
				ChangeFlowsToParent = objectInfo["changeFlowsToParent"];
			}

			if (objectInfo.ContainsKey("changeFlowsFromParent"))
			{
				ChangeFlowsFromParent = objectInfo["changeFlowsFromParent"];
			}

			int idx = 0;
			string key = String.Format("Paths{0}", idx);
			if (objectInfo.ContainsKey(key))
			{
				ViewMap Paths = new ViewMap();
				StringEnum<MapType> map = null;
				PathSpec LeftPath = null;
				PathSpec RightPath = null;
				MapEntry Path = new MapEntry(map, LeftPath, RightPath);

				while (objectInfo.ContainsKey(key))
				{
					string l = (objectInfo[key]);
					string[] p = l.Split(' ');
					map = p[0];
					LeftPath = new DepotPath(p[1]);
					RightPath = new DepotPath(p[2]);
					Path = new MapEntry(map, LeftPath, RightPath);
					Paths.Add(Path);
					idx++;
					key = String.Format("Paths{0}", idx);
				}
			}

			idx = 0;
			key = String.Format("Remapped{0}", idx);
			if (objectInfo.ContainsKey(key))
			{
				ViewMap Remapped = new ViewMap();
				PathSpec LeftPath = null;
				PathSpec RightPath = null;
				MapEntry Remap = new MapEntry(MapType.Include, LeftPath, RightPath);

				while (objectInfo.ContainsKey(key))
				{
					string l = (objectInfo[key]);
					string[] p = l.Split(' ');
					LeftPath = new DepotPath(p[0]);
					RightPath = new DepotPath(p[1]);
					Remap = new MapEntry(MapType.Include, LeftPath, RightPath);
					Remapped.Add(Remap);
					idx++;
					key = String.Format("Remapped{0}", idx);
				}
			}

			idx = 0;
			key = String.Format("Ignored{0}", idx);
			if (objectInfo.ContainsKey(key))
			{
				List<FileSpec> Ignored = new List<FileSpec>();
				FileSpec ignore = new FileSpec(new DepotPath(string.Empty), null);

				while (objectInfo.ContainsKey(key))
				{
					string i = (objectInfo[key]);
					ignore = new FileSpec(new DepotPath(i), null);
					Ignored.Add(ignore);
					idx++;
					key = String.Format("Remapped{0}", idx);
				}
			}
			else
			{
				Ignored = null;
			}
		}

		/// <summary>
		/// Read the fields from the tagged output of a 'stream' command
		/// </summary>
		/// <param name="objectInfo">Tagged output from the 'stream' command</param>
		public void FromStreamCmdTaggedOutput(TaggedObject objectInfo)
		{
			_baseForm = new FormBase();

			_baseForm.SetValues(objectInfo);

			if (objectInfo.ContainsKey("Stream"))
			{
				Id = objectInfo["Stream"];
			}

			if (objectInfo.ContainsKey("Update"))
			{
				DateTime v = DateTime.MinValue;
				DateTime.TryParse(objectInfo["Update"], out v);
				Updated = v;
			}

			if (objectInfo.ContainsKey("Access"))
			{
				DateTime v = DateTime.MinValue;
				DateTime.TryParse(objectInfo["Access"], out v);
				Accessed = v;
			}

			if (objectInfo.ContainsKey("Owner"))
			{
				OwnerName = objectInfo["Owner"];
			}

			if (objectInfo.ContainsKey("Name"))
			{
				Name = objectInfo["Name"];
			}

			if (objectInfo.ContainsKey("Parent"))
			{
				Parent = new DepotPath(objectInfo["Parent"]);
			}

			if (objectInfo.ContainsKey("baseParent"))
			{
				BaseParent = new DepotPath(objectInfo["baseParent"]);
			}

			if (objectInfo.ContainsKey("Type"))
			{
				_type = (objectInfo["Type"]);
			}

			if (objectInfo.ContainsKey("Description"))
			{
                Description = objectInfo["Description"];
			}

			if (objectInfo.ContainsKey("Options"))
			{
                String optionsStr = objectInfo["Options"];
                _options = optionsStr;
			}

			if (objectInfo.ContainsKey("firmerThanParent"))
			{
				FirmerThanParent = objectInfo["firmerThanParent"];
			}

			if (objectInfo.ContainsKey("changeFlowsToParent"))
			{
				ChangeFlowsToParent = objectInfo["changeFlowsToParent"];
			}

			if (objectInfo.ContainsKey("changeFlowsFromParent"))
			{
				ChangeFlowsFromParent = objectInfo["changeFlowsFromParent"];
			}

			int idx = 0;
			string key = String.Format("Paths{0}", idx);
			if (objectInfo.ContainsKey(key))
			{
				Paths = new ViewMap();
				while (objectInfo.ContainsKey(key))
				{
					Paths.Add(objectInfo[key]);
					idx++;
					key = String.Format("Paths{0}", idx);
				}
			}
			else
			{
				Paths = null;
			}

			idx = 0;
			key = String.Format("Remapped{0}", idx);
			if (objectInfo.ContainsKey(key))
			{
				Remapped = new ViewMap();
				PathSpec LeftPath = new ClientPath(string.Empty);
				PathSpec RightPath = new ClientPath(string.Empty);
				MapEntry Remap = new MapEntry(MapType.Include, LeftPath, RightPath);

				while (objectInfo.ContainsKey(key))
				{
					string l = (objectInfo[key]);
					string[] p = l.Split(' ');
					LeftPath = new ClientPath(p[0]);
					RightPath = new ClientPath(p[1]);
					Remap = new MapEntry(MapType.Include, LeftPath, RightPath);
					Remapped.Add(Remap);
					idx++;
					key = String.Format("Remapped{0}", idx);
				}
			}
			else
			{
				Remapped = null;
			}

			idx = 0;
			key = String.Format("Ignored{0}", idx);
			if (objectInfo.ContainsKey(key))
			{
				Ignored = new ViewMap();
				PathSpec LeftPath = new ClientPath(string.Empty);
				PathSpec RightPath = new ClientPath(string.Empty);
				MapEntry Ignore = new MapEntry(MapType.Include, LeftPath, RightPath);

				while (objectInfo.ContainsKey(key))
				{
					string l = (objectInfo[key]);
					string[] p = l.Split(' ');
					LeftPath = new ClientPath(p[0]);
					if (p.Length > 1)
					{
						RightPath = new ClientPath(p[1]);
					}
					Ignore = new MapEntry(MapType.Include, LeftPath, RightPath);
					Ignored.Add(Ignore);
					idx++;
					key = String.Format("Ignored{0}", idx);
				}
			}
			else
			{
				Ignored = null;
			}

            idx = 0;
            key = String.Format("View{0}", idx);
            if (objectInfo.ContainsKey(key))
            {
                View = new ViewMap();
                while (objectInfo.ContainsKey(key))
                {
                    View.Add(objectInfo[key]);
                    idx++;
                    key = String.Format("View{0}", idx);
                }
            }
            else
            {
                View = null;
            }
		}

		/// <summary>
		/// Parse the fields from a stream specification 
		/// </summary>
		/// <param name="spec">Text of the stream specification in server format</param>
		/// <returns></returns>
		public bool Parse(String spec)
		{
			_baseForm = new FormBase();
			_baseForm.Parse(spec); // parse the values into the underlying dictionary

			if (_baseForm.ContainsKey("Stream"))
			{
				Id = _baseForm["Stream"] as string;
			}

			if (_baseForm.ContainsKey("Update"))
			{
				DateTime d;
				if (DateTime.TryParse(_baseForm["Update"] as string, out d))
				{
					Updated = d;
				}
			}

			if (_baseForm.ContainsKey("Access"))
			{
				DateTime d;
				if (DateTime.TryParse(_baseForm["Access"] as string, out d))
				{
					Accessed = d;
				}
			}

			if (_baseForm.ContainsKey("Owner"))
			{
				OwnerName = _baseForm["Owner"] as string;
			}

            if (_baseForm.ContainsKey("Name"))
            {
                Name = _baseForm["Name"] as string;
            }

			if (_baseForm.ContainsKey("Parent"))
			{
				string line = _baseForm["Parent"] as string;
				Parent = new DepotPath(line);
			}

			if (_baseForm.ContainsKey("Type"))
			{
				_type = _baseForm["Type"] as string;
			}

			if (_baseForm.ContainsKey("Description"))
			{
				Description = _baseForm["Description"] as string;
			}

			if (_baseForm.ContainsKey("Options"))
			{
				_options = _baseForm["Options"] as string;
			}

			return true;
		}

		/// <summary>
		/// Format of a stream specification used to save a stream to the server
		/// </summary>
		private static String StreamSpecFormat =
													"Stream:\t{0}\r\n" +
													"\r\n" +
													"Update:\t{1}\r\n" +
													"\r\n" +
													"Access:\t{2}\r\n" +
													"\r\n" +
													"Owner:\t{3}\r\n" +
													"\r\n" +
													"Name:\t{4}\r\n" +
													"\r\n" +
													"Parent:\t{5}\r\n" +
													"\r\n" +
													"Type:\t{6}\r\n" +
													"\r\n" +
													"Description:\r\n\t{7}\r\n" +
													"\r\n" +
													"Options:\t{8}\r\n" +
													"\r\n" +
													"Paths:\r\n\t{9}\r\n" +
													"\r\n" +
													"Remapped:\r\n\t{10}\r\n" +
													"\r\n" +
													"Ignored:\r\n\t{11}\r\n";




		/// <summary>
		/// Convert to specification in server format
		/// </summary>
		/// <returns></returns>
		override public String ToString()
		{
            String descStr = String.Empty;
            if (Description != null)
                descStr = Description.Replace("\n", "\n\t");

			String Type = _type.ToString(StringEnumCase.Lower);

            String ParentPath = String.Empty;
            if (Parent!=null)
            {
               ParentPath = Parent.Path.ToString();
                if (ParentPath=="None")
                {
                    ParentPath = ParentPath.ToLower();
                }
            }

			String pathsView = String.Empty;
			if (Paths != null)
				pathsView = Paths.ToString().Replace("\r\n", "\r\n\t").Trim();

			String remappedView = String.Empty;
			if (Remapped != null)
				remappedView = Remapped.ToString().Replace("\r\n", "\r\n\t").Trim();
			
			String ignoredView = String.Empty;
			if (Ignored != null)
				ignoredView = Ignored.ToString().Replace("\r\n", "\r\n\t").Trim();

			String value = String.Format(StreamSpecFormat, Id,
				FormBase.FormatDateTime(Updated), FormBase.FormatDateTime(Accessed),
				OwnerName, Name, ParentPath, Type, descStr, _options.ToString(),
				pathsView, remappedView, ignoredView);
			return value;
		}
	}


	/// <summary>
	/// Defines the expected flow of change between a stream and its parent.	
	/// </summary>
	[Flags]
		public enum StreamType
		{
		/// <summary>
		/// Development: Default. Direction of flow is
		/// to parent stream with copy and from parent
		/// stream with merge.
		/// </summary>
		Development = 0x0000,
		/// <summary>
		/// Mainline: May not have a parent. 
		/// </summary>
		Mainline = 0x0001,
		/// <summary>
		/// Release: Direction of flow is to parent
		/// with merge and from parent with copy.
		/// </summary>
		Release = 0x0002,
		/// <summary>
		/// Virtual: Not a stream but an alternative 
		/// view of its parent stream.
		/// </summary>
		Virtual = 0x0004,
        /// <summary>
        /// Task: A lightweight short-lived stream that
        ///  only promotes modified content to the
        ///  repository, branched data is stored in
        ///  shadow tables that are removed when the task
        ///  stream is deleted or unloaded.
        /// </summary>
        Task = 0x0008
		}	

	/// <summary>
	/// Flags to configure stream behavior.
	/// </summary>
		[Flags]
		public enum StreamOption
		{
			/// <summary>
			/// No flags.
			/// </summary>
			None = 0x0000,
			/// <summary>
			/// Indicates whether all users or only the
			/// of the stream may submit changes to the
			/// stream path.
			/// </summary>
			OwnerSubmit = 0x0001,
			/// <summary>
			/// Indicates whether the stream spec is locked
			/// against modifications. If locked, the spec
			/// may not be deleted, and only its owner may
			/// modify it.
			/// </summary>
			Locked = 0x0002,
			/// <summary>
			/// Indicates whether integration from the
			/// stream to its parent is expected to occur.
			/// </summary>
			NoToParent = 0x0004,
			/// <summary>
			/// Indicates whether integration to the stream
			/// from its parent is expected to occur.
			/// </summary>
			NoFromParent = 0x0008
		}

		internal class StreamOptionEnum : StringEnum<StreamOption>
		{

			public StreamOptionEnum(StreamOption v)
				: base(v)
			{
			}

			public StreamOptionEnum(string spec)
				: base(StreamOption.None)
			{
				Parse(spec);
			}

			public static implicit operator StreamOptionEnum(StreamOption v)
			{
				return new StreamOptionEnum(v);
			}

			public static implicit operator StreamOptionEnum(string s)
			{
				return new StreamOptionEnum(s);
			}

			public static implicit operator string(StreamOptionEnum v)
			{
				return v.ToString();
			}

			public override bool Equals(object obj)
			{
				if (obj.GetType() == typeof(StreamOption))
				{
					return value.Equals((StreamOption)obj);
				}
				if (obj.GetType() == typeof(StreamOptionEnum))
				{
					return value.Equals(((StreamOptionEnum)obj).value);
				}
				return false;
			}
            public override int GetHashCode() { return base.GetHashCode(); }

            public static bool operator ==(StreamOptionEnum t1, StreamOptionEnum t2) { return t1.value.Equals(t2.value); }
			public static bool operator !=(StreamOptionEnum t1, StreamOptionEnum t2) { return !t1.value.Equals(t2.value); }

			public static bool operator ==(StreamOption t1, StreamOptionEnum t2) { return t1.Equals(t2.value); }
			public static bool operator !=(StreamOption t1, StreamOptionEnum t2) { return !t1.Equals(t2.value); } 

			public static bool operator ==(StreamOptionEnum t1, StreamOption t2) { return t1.value.Equals(t2); }
			public static bool operator !=(StreamOptionEnum t1, StreamOption t2) { return !t1.value.Equals(t2); }

			/// <summary>
			/// Convert to a stream spec formatted string
			/// </summary>
			/// <returns></returns>
			public override string ToString()
			{
				return String.Format("{0} {1} {2} {3}",
					((value & StreamOption.OwnerSubmit) != 0) ? "ownersubmit" : "allsubmit",
					((value & StreamOption.Locked) != 0) ? "locked" : "unlocked",
					((value & StreamOption.NoToParent) != 0) ? "notoparent" : "toparent",
					((value & StreamOption.NoFromParent) != 0) ? "nofromparent" : "fromparent"
					);
			}
			/// <summary>
			/// Parse a client spec formatted string
			/// </summary>
			/// <param name="spec"></param>
			public void Parse(String spec)
			{
				value = StreamOption.None;

				if (!spec.Contains("allsubmit"))
					value |= StreamOption.OwnerSubmit;

				if (!spec.Contains("unlocked"))
					value |= StreamOption.Locked;

				if (spec.Contains("notoparent"))
					value |= StreamOption.NoToParent;

				if (spec.Contains("nofromparent"))
					value |= StreamOption.NoFromParent;
			}
		}

		


	
}

		

