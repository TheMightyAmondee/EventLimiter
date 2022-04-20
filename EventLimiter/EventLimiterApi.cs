using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventLimiter
{
    public class EventLimiterApi
    {
        public static ModConfig config;
        public static List<int> internalexceptions;
        public static void GetConfigValues(ModConfig config, List<int> internalexceptions)
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
                foreach(int modexception in internalexceptions)
                {
                    exceptions.Add(modexception);
                }
            }

            foreach (int exception in config.Exceptions)
            {
                exceptions.Add(exception);
            }

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
                internalexceptions.Add(eventid);
                return true;
            }
        }
    }

}
