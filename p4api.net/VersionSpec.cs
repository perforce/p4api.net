using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Perforce.P4
{
	/// <summary>
	/// Identifies a specific revision or revision range of a Perforce managed SCM resource.
	/// </summary>
	public  abstract class VersionSpec
	{
        public static string DateTimeFormat = "yyy/MM/dd:HH:mm:ss";
		public abstract override string ToString();

		public static HeadRevision Head = new HeadRevision();
		public static HaveRevision Have = new HaveRevision();
		public static NoneRevision None = new NoneRevision();

        // Return a concrete class derived from VersionSpec which matches the revspec string passed
        public static VersionSpec CreateVersionInstance(string specifier)
        {
            if (specifier.Equals("#head"))
            {
                return Head;
            }

            if (specifier.Equals("#have"))
            {
                return Have;
            }

            if (specifier.Equals("#none"))
            {
                return None;
            }

            if (specifier.Contains("#"))    // specific revision of a file
            {
                return new Revision(specifier);
            }

            if (specifier.Contains("@"))
            {
                try
                {
                    DateTime tm = DateTime.ParseExact(specifier.Substring(1), DateTimeFormat,
                        System.Globalization.CultureInfo.InvariantCulture);
                    return new DateTimeVersion(tm);
                }
                catch
                {
                    // not a valid date format, so skip it
                }
            }

            if (specifier.Contains("@="))
            {
                // Shelve specification
                return new ShelvedInChangelistIdVersion(specifier);
            }

            if (specifier.Contains("@"))
            {
                if (int.TryParse(specifier.Substring(1), out var changeId))
                {
                    return new ChangelistIdVersion(changeId);
                }
                else
                {
                    // it's not a number, so it could be a label or a client name, 
                    // I can't tell the difference, so assume label 
                    return new LabelNameVersion(specifier);
                }
            }

            return null;  // doesn't match any pattern for a version string
	}
    }


	/// <summary>
	/// A revision range specifier consisting of a lower and upper revision.
	/// </summary>
	public class VersionRange : VersionSpec
	{
		/// <summary>
		/// A revision range specifier consisting of a lower and upper revision.
		/// </summary>
		///<param name="lower">version spec to get lower revision</param>
		///<param name="upper">version spec to get upper revision</param>
		public VersionRange(VersionSpec lower, VersionSpec upper)
		{
			if (lower == null)
			{
				throw new ArgumentNullException("lower");
			}
			if (upper == null)
			{
				throw new ArgumentNullException("upper");
			}
			Lower = lower;
			Upper = upper;
		}
		/// <summary>
		/// A revision range specifier consisting of a lower and upper revision.
		/// </summary>
		///<param name="lower">int to get lower revision</param>
		///<param name="upper">int to get upper revision</param>
		public VersionRange(int lower, int upper)
		{
			Lower = new Revision(lower);
			Upper = new Revision(upper);
		}

        public VersionRange(string revspec)
        {
            TryParse(revspec, out var lrev, out var urev);

            Lower = new Revision(lrev);
            Upper = new Revision(urev);
        }

        private static bool TryParse(string input, out int lrevo, out int urevo)
        {
            lrevo = urevo = 0;
            try
            {
                string[] parts = input.Split(',');
                if (parts.Length == 2)
                {
                    int.TryParse(parts[0].Substring(1), out var lrev);
                    int.TryParse(parts[1].Substring(1), out var urev);

                    if (lrev > urev)  // swap values
                    {
                        int t = urev;
                        urev = lrev;
                        lrev = t;
                    }

                    lrevo = lrev;
                    urevo = urev;
                    return true;
                }
            }
            catch
            {
            }
            return false;
        }

		/// <summary>
		/// Lower version revision.
		/// </summary>
		public VersionSpec Lower { get; set; }
		/// <summary>
		/// Upper version revision.
		/// </summary>
		public VersionSpec Upper { get; set; }

		///<summary>ToString method for for VersionRange</summary>
		/// <returns>String version range</returns>
		public override string ToString()
		{
			return String.Format( "{0},{1}", Lower, Upper);
		}
		///<summary>Equals method for for VersionRange</summary>
		///<param name="obj">object to get version range</param>
		/// <returns>True/False</returns>
		public override bool Equals(object obj)
		{
			if (obj == null)
			{ return false; }
			if (obj.GetType() != this.GetType())
			{
				return false;
			}
			VersionRange o = obj as VersionRange;

			if (o.Lower != null)
			{
				if (o.Lower.Equals(this.Lower) == false)
				{ return false; }
			}
			else
			{
				if (this.Lower != null)
				{ return false; }
			}
			if (o.Upper != null)
			{
				if (o.Upper.Equals(this.Upper) == false)
				{ return false; }
			}
			else
			{
				if (this.Upper != null)
				{ return false; }
			}

			return true;
		}
        public override int GetHashCode() { return base.GetHashCode(); }
    }

    /// <summary>
    /// A revision specifier #head.
    /// </summary>
    public class HeadRevision : VersionSpec
	{
        public HeadRevision()
        {
        }

#pragma warning disable 0169
        public HeadRevision(string revspec)
        {
            /// revspec must always be "#head"
        }
#pragma warning restore 0169

		///<summary>ToString method for for HeadRevision</summary>
		/// <returns>#head</returns>
		public override string ToString()
		{
			return "#head";
		}
		///<summary>Equals method for for HeadRevision</summary>
		///<param name="obj">object to get head revision</param>
		/// <returns>True/False</returns>
		public override bool Equals(object obj)
		{
			if (obj == null)
			{ return false; }
			if (obj.GetType() != this.GetType())
			{
				return false;
			}
			return true;
		}
        public override int GetHashCode() { return base.GetHashCode(); }
    }

    /// <summary>
    /// A revision specifier #have.
    /// </summary>
    public class HaveRevision : VersionSpec
	{
        public HaveRevision()
        {
        }

#pragma warning disable 0169
        public HaveRevision(string revspec)
        {
            // revspec will always be "#have"
        }
#pragma warning restore 0169

		///<summary>ToString method for for HaveRevision</summary>
		/// <returns>#have</returns>
		public override string ToString()
		{
			return "#have";
		}
		///<summary>Equals method for for HaveRevision</summary>
		///<param name="obj">object to get have revision</param>
		/// <returns>True/False</returns>
		public override bool Equals(object obj)
		{
			if (obj == null)
			{ return false; }
			if (obj.GetType() != this.GetType())
			{
				return false;
			}
			return true;
		}
        public override int GetHashCode() { return base.GetHashCode(); }
    }

    /// <summary>
    /// A revision specifier #none.
    /// </summary>
    public class NoneRevision : VersionSpec
	{
        public NoneRevision()
        {
        }

#pragma warning disable 0169
        public NoneRevision(string revspec)
        {
            // revspec will always be "#none"
        }
#pragma warning restore 0169

		///<summary>ToString method for for NoneRevision</summary>
		/// <returns>#none</returns>
		public override string ToString()
		{
			return "#none";
		}
		///<summary>Equals method for for NoneRevision</summary>
		///<param name="obj">object to get none revision</param>
		/// <returns>True/False</returns>
		public override bool Equals(object obj)
		{
			if (obj == null)
			{ return false; }
			if (obj.GetType() != this.GetType())
			{
				return false;
			}
			return true;
		}
        public override int GetHashCode() { return base.GetHashCode(); }
    }
    /// <summary>
    /// A revision specifier that is a single revision.
    /// </summary>
    public class Revision : VersionSpec
	{
		/// <summary>
		/// A revision specifier that is a single revision.
		/// </summary>
		public Revision(int rev) { Rev = rev; }

        /// <summary>
        ///  construct Revision from a string "#42" for instance
        /// </summary>
        /// <param name="revspec"></param>
        public Revision(string revspec) : this(int.Parse(revspec.Substring(1))) { }

		public int Rev { get; private set; }
		///<summary>ToString method for for Revision</summary>
		/// <returns>String client revision version</returns>
		public override string ToString()
		{
			if (Rev >= 0)
			{
				return String.Format("#{0}", Rev);
			}
			return string.Empty;
		}
		///<summary>Equals method for for Revision</summary>
		///<param name="obj">object to get revision</param>
		/// <returns>True/False</returns>
		public override bool Equals(object obj)
		{
			if (obj == null)
			{ return false; }
			if (obj.GetType() != this.GetType())
			{
				return false;
			}
			Revision o = obj as Revision;
            if (o?.Rev != this.Rev)
			{
				return false;
			}
			return true;
		}
        public override int GetHashCode() { return base.GetHashCode(); }
    }
    /// <summary>
    /// A revision specifier that is a date and time.
    /// </summary>
    public class DateTimeVersion : VersionSpec
	{
		/// <param name="date">The date/time.</param>
		public DateTimeVersion(DateTime date)
		{
			Version = date;
		}

        public DateTimeVersion(string revspec)
        {
            // skip over the "@" then parse the rest of the string
            try
            {
                Version = DateTime.ParseExact(revspec.Substring(1), DateTimeFormat,
                    System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                Version = DateTime.MinValue;
            }
        }

		///<summary>get Version as date/time</summary>
		public DateTime Version { get; private set; }
		///<summary>ToString method for for DateTimeVersion</summary>
		/// <returns>String date/time version</returns>
		public override string ToString()
		{
            return String.Format("@{0}", Version.ToString(DateTimeFormat,
                System.Globalization.CultureInfo.InvariantCulture));
		}
		///<summary>Equals method for for DateTimeVersion</summary>
		///<param name="obj">object to get date/time</param>
		/// <returns>True/False</returns>
		public override bool Equals(object obj)
		{
			if (obj == null)
			{ return false; }
			if (obj.GetType() != this.GetType())
			{
				return false;
			}
			DateTimeVersion o = obj as DateTimeVersion;
            if (o?.Version != this.Version)
			{
				return false;
			}
			return true;
		}
        public override int GetHashCode() { return base.GetHashCode(); }
    }

    /// <summary>
    /// A revision specifier that is a label name.
    /// </summary>
    public class LabelNameVersion : VersionSpec
	{
		/// <param name="LabelName">The label.</param>
        /// <param name="FromRevSpec">Set if decoding a revision specifier, like "@labelname"</param>
        public LabelNameVersion(string LabelName, bool FromRevSpec = false)
		{
            Version = FromRevSpec ? LabelName.Substring(1) : LabelName;
		}

		///<summary>get Version as label</summary>
		public string Version { get; private set; }

		///<summary>ToString method for for LabelNameVersion</summary>
		/// <returns>String label version</returns>
		public override string ToString()
		{
			return String.Format("@{0}",Version);
		}

		///<summary>Equals method for for LabelNameVersion</summary>
		///<param name="obj">object to get label</param>
		/// <returns>True/False</returns>
		public override bool Equals(object obj)
		{
			if (obj == null)
			{ return false; }
			if (obj.GetType() != this.GetType())
			{
				return false;
			}
			LabelNameVersion o = obj as LabelNameVersion;
            if (o?.Version != this.Version)
			{
				return false;
			}
			return true;
		}
        public override int GetHashCode() { return base.GetHashCode(); }

    }

    /// <summary>
    /// A revision specifier that is a changelist id.
    /// </summary>
    public class ChangelistIdVersion : VersionSpec
	{
		/// <param name="Changelist">The changelist.</param>
		public ChangelistIdVersion(int Changelist)
		{
			ChanglistId = Changelist;
		}
        /// <summary>
        ///  construct ChangelistIdVersion from a string "@42" for instance
        /// </summary>
        /// <param name="revspec"></param>
        public ChangelistIdVersion(string revspec) : this(int.Parse(revspec.Substring(1))) { }

		///<summary>get Version as changelist</summary>
		public int ChanglistId { get; private set; }
		///<summary>ToString method for for ChangelistIdVersion</summary>
		/// <returns>String changelist version</returns>
		public override string ToString()
		{
			return String.Format("@{0}", ChanglistId);
		}
		///<summary>Equals method for for ChangelistIdVersion</summary>
		///<param name="obj">object to get changelist</param>
		/// <returns>True/False</returns>
		public override bool Equals(object obj)
		{
			if (obj == null)
			{ return false; }
			if (obj.GetType() != this.GetType())
			{
				return false;
			}
			ChangelistIdVersion o = obj as ChangelistIdVersion;
			if (o.ChanglistId != this.ChanglistId)
			{
				return false;
			}
			return true;
		}
        public override int GetHashCode() { return base.GetHashCode(); }
    }

    /// <summary>
    /// A revision specifier that is a client name.
    /// </summary>
    public class ClientNameVersion : VersionSpec
	{
        /// <param name="clientName">The client name or revision spec./// </param>
        /// <param name="fromRevSpec">Set if passing a revspec like "@client_name" </param>
        public ClientNameVersion(string clientName, bool fromRevSpec = false)
		{
            Version = fromRevSpec ? clientName.Substring(1) : clientName;
		}
		///<summary>get Version as client name</summary>
		public string Version { get; private set; }
		///<summary>ToString method for for ClientNameVersion</summary>
		/// <returns>String client name version</returns>
		public override string ToString()
		{
			return String.Format("@{0}", Version);
		}
		///<summary>Equals method for for ClientNameVersion</summary>
		///<param name="obj">object to get client name</param>
		/// <returns>True/False</returns>
		public override bool Equals(object obj)
		{
			if (obj == null)
			{ return false; }
			if (obj.GetType() != this.GetType())
			{
				return false;
			}
			ClientNameVersion o = obj as ClientNameVersion;
            if (o?.Version != this.Version)
			{
				return false;
			}
			return true;
		}
        public override int GetHashCode() { return base.GetHashCode(); }
    }
    /// <summary>
    /// A revision specifier that is a file action.
    /// </summary>
    public class ActionVersion : VersionSpec
	{
		/// <param name="Action">The file action./// </param>
        public ActionVersion(string action, bool fromRevSpec = false)
		{
            Version = fromRevSpec ? action.Substring(1) : action;
		}
		///<summary>get Version as action</summary>
		public string Version { get; private set; }
		///<summary>ToString method for for ActionVersion</summary>
		/// <returns>String action version</returns>
		public override string ToString()
		{
            return String.Format("#{0}", Version);
		}
		///<summary>Equals method for for ActionVersion</summary>
		///<param name="obj">object to get action type</param>
		/// <returns>True/False</returns>
		public override bool Equals(object obj)
		{
			if (obj == null)
			{ return false; }
			if (obj.GetType() != this.GetType())
			{
				return false;
			}
			ActionVersion o = obj as ActionVersion;
			if (o.Version != this.Version)
			{
				return false;
			}
			return true;
		}
        public override int GetHashCode() { return base.GetHashCode(); }
    }

    /// <summary>
    /// A revision specifier for a file that is shelved in a changelist id.
    /// </summary>
    public class ShelvedInChangelistIdVersion : VersionSpec
	{
        /// <summary>Construct from revision id</summary>
        /// <param name="Changeid">The changelist id.</param>
        public ShelvedInChangelistIdVersion(int changeid)
		{
            changelistId = changeid;
		}

        /// <summary>Construct from revision spec</summary>
        /// <param name="revspec">The revision spec string</param>
        public ShelvedInChangelistIdVersion(string revspec) : this(int.Parse(revspec.Substring(2))) { }

		///<summary>get Version as changelist</summary>
        public int changelistId { get; private set; }
		///<summary>ToString method for for ChangelistIdVersion</summary>
		/// <returns>String changelist version</returns>
		public override string ToString()
		{
            return String.Format("@={0}", changelistId);
		}
        ///<summary>Equals method for ShelvedChangelistIdVersion</summary>
		///<param name="obj">object to get changelist</param>
		/// <returns>True/False</returns>
		public override bool Equals(object obj)
		{
			if (obj == null)
			{ return false; }
			if (obj.GetType() != this.GetType())
			{
				return false;
			}
            ShelvedInChangelistIdVersion o = obj as ShelvedInChangelistIdVersion;
            if (o.changelistId != this.changelistId)
			{
				return false;
			}
			return true;
		}
        public override int GetHashCode() { return base.GetHashCode(); }
    }
}
