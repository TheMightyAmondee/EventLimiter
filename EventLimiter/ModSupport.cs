using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventLimiter
{
    class ModSupport
    {
      
        public class EventLimiterApi
        {
            private static ModConfig config;
            private static List<int> internalexceptions;
            public void GetConfigValues(ModConfig config, List<int> internalexceptions)
            {
                EventLimiterApi.config = config;
                EventLimiterApi.internalexceptions = internalexceptions;
            }

            public int GetDayLimit()
            {
                return config.EventsPerDay;
            }

            public int GetRowLimit()
            {
                return config.EventsInARow;
            }

            public List<int> GetExceptions(bool includeinternal = true)
            {
                List<int> exceptions = new List<int>();

                if (includeinternal == true)
                {
                    exceptions.AddRange(internalexceptions);
                }

                exceptions.AddRange(config.Exceptions);

                return exceptions;
            }

            public bool AddInternalException(int eventid)
            {
                if (eventid < 0)
                {
                    return false;
                }

                else
                {
                    internalexceptions.Append(eventid);
                    return true;
                }
            }
        }
    }
}
