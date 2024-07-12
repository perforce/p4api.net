/*******************************************************************************
 * Name		: Utility.cs
 *
 * Author	: shraddha.borate
 *
 * Description	: Static class used to define common methods used.
 *
 ******************************************************************************/
namespace Perforce.P4
{
    static class Utility
    {
       public static ViewMap GetViewMapEntries(TaggedObject objectProperties)
        {
            int idx = 0;
            string key = $"View{idx}";
            string comment = $"ViewComment{idx}";
            ViewMap ViewMap = new ViewMap();
            if (objectProperties.TryGetValue(key, out var keyValue))
            {
               
                while (true)
                {
                    if (objectProperties.TryGetValue(comment, out var commentValue))
                    {
                        if (!string.IsNullOrEmpty(commentValue))
                        {
                            ViewMap.Add(keyValue + commentValue);
                        }
                        else if (!string.IsNullOrEmpty(keyValue))
                        {
                            ViewMap.Add(keyValue);
                        }
                    }
                    else if (!string.IsNullOrEmpty(keyValue))
                    {
                        ViewMap.Add(keyValue);
                    }

                    idx++;
                    key = $"View{idx}";
                    comment = $"ViewComment{idx}";

                    if (!objectProperties.TryGetValue(key, out keyValue))
                    {
                        break;
                    }
                }
            }

            return ViewMap;
        }
    }
}
