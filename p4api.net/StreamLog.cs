using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perforce.P4
{
    public class StreamLog
    {
        public string Action { get; }
        public int Change { get; }
        public string Stream { get; }
        public int Date { get; }
        public string User { get; }
        public int AssociatedChange { get; }
        public string Client { get; }
        public string Description { get; }
        public List<StreamIntegrationLog> StreamIntegrationLogs { get; }
        public StreamLog() { }
        public StreamLog(TaggedObject obj, int i) 
        {
            string key;
            int parserdInt;
            
            key = "action" + i.ToString();            
            if (obj.ContainsKey(key))
            {
                Action = obj[key];
            }

            key = "change" + i.ToString();
            if (obj.ContainsKey(key))
            {
                if (int.TryParse(obj[key], out parserdInt))
                {
                    Change = parserdInt;
                }                
            }

            key = "time" + i.ToString();
            if (obj.ContainsKey(key))
            {
                if (int.TryParse(obj[key], out parserdInt))
                {
                    Date = parserdInt;
                }
            }

            key = "user" + i.ToString();
            if (obj.ContainsKey(key))
            {
                User = obj[key];
            }

            key = "client" + i.ToString();
            if (obj.ContainsKey(key))
            {
                Client = obj[key];
            }

            key = "associatedChange" + i.ToString();
            if (obj.ContainsKey(key))
            {
                if (int.TryParse(obj[key], out parserdInt))
                {
                    AssociatedChange = parserdInt;
                }
            }

            key = "desc" + i.ToString();
            if (obj.ContainsKey(key))
            {
                Description = obj[key];
            }

            int k = 0;
            StreamIntegrationLogs = new List<StreamIntegrationLog>();
            while (obj.ContainsKey("stream" + i.ToString() + "," + k.ToString()))
            {
                StreamIntegrationLogs.Add(new StreamIntegrationLog(obj, i.ToString() + "," + k.ToString()));
                k++;
            }
        }
    }
}
