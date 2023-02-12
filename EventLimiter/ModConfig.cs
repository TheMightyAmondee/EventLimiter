﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventLimiter
{
    class ModConfig
    {
        public int EventsPerDay { get; set; } = 4;
        public int EventsInARow { get; set; } = 2;
        public bool ExemptEventsCountTowardsLimit { get; set; } = true;
        public int[] Exceptions { get; set; } = { };
    }
}
