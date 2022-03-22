using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perforce.P4
{
    public class StreamIntegrationLog
    {
        public string Stream { get;  }
        public string How { get; }
        public string Field { get; }
        public int EndFromChange { get; }
        public int StartFromChange { get; }

        public StreamIntegrationLog(TaggedObject obj, string suffix)
        {
            string key;
            int parserdInt;

            key = "stream" + suffix;
            if (obj.ContainsKey(key))
            {
                Stream = obj[key];
            }

            key = "how" + suffix;
            if (obj.ContainsKey(key))
            {
                How = obj[key];
            }

            key = "field" + suffix;
            if (obj.ContainsKey(key))
            {
                Field = obj[key];
            }

            key = "endFromChange" + suffix;
            if (obj.ContainsKey(key))
            {
                if (int.TryParse(obj[key], out parserdInt))
                {
                    EndFromChange = parserdInt;
                }
            }

            key = "startFromChange" + suffix;
            if (obj.ContainsKey(key))
            {
                if (int.TryParse(obj[key], out parserdInt))
                {
                    StartFromChange = parserdInt;
                }
            }
        }
    }
}
