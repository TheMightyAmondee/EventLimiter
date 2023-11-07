﻿using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewValley;
using HarmonyLib;

namespace EventLimiter
{
    class Patches
    {
        private static IMonitor monitor;
        private static ModConfig config;
        private static List<string> internalexceptions;

        public static void Hook(Harmony harmony, IMonitor monitor, ModConfig config, List<string> internalexceptions)
        {
            Patches.monitor = monitor;
            Patches.config = config;
            Patches.internalexceptions = internalexceptions;

            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.startEvent)),
                postfix: new HarmonyMethod(typeof(Patches), nameof(Patches.startEvent_postfix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.exitEvent)),
                postfix: new HarmonyMethod(typeof(Patches), nameof(Patches.exitEvent_postfix)));

            monitor.Log("Initialised harmony patches...");
        }

        private string GetTrimmedId(string id)
        {
            var idparts = id.Split("/");
            return idparts[0];
        }

        public static void startEvent_postfix(GameLocation __instance, Event evt)
        {
            try
            {
                if (evt.id != "PlayerKilled" && GetTrimmedId(evt.id) != "60367" && evt.isFestival == false)
                {
                    // Check if the event is an exception, skip the rest of the method if so
                    if (config.Exceptions != null && config.Exceptions.Contains(evt.id) == true)
                    {
                        monitor.Log("Made exception for event with id " + evt.id);
                        return;
                    }

                    // Check for mod added exceptions, skip the rest of the method if so
                    if (internalexceptions != null && internalexceptions.Contains(evt.id) == true)
                    {
                        monitor.Log("Made mod added exception for event with id " + evt.id);
                        return;
                    }

                    // Check if day limit is reached, skip event if so
                    if (ModEntry.EventCounterDay.Value >= config.EventsPerDay)
                    {
                        monitor.Log("Day limit reached! Skipping event...");
                        Game1.eventUp = false;
                        Game1.displayHUD = true;
                        Game1.player.CanMove = true;
                        __instance.currentEvent = null;
                        return;
                    }

                    // Check if row limit is reached, skip event if so
                    else if (ModEntry.EventCounterRow.Value >= config.EventsInARow)
                    {
                        monitor.Log("Continuous event limit reached! Skipping event...");
                        Game1.eventUp = false;
                        Game1.displayHUD = true;
                        Game1.player.CanMove = true;
                        __instance.currentEvent = null;
                        return;
                    }
                }

            }
            catch (Exception ex)
            {
                monitor.Log($"Failed in {nameof(startEvent_postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static void exitEvent_postfix(Event __instance)
        {
            try
            {
                // Exit method if counters shouldn't increment (event exception not counting towards limit)
                if (config.ExemptEventsCountTowardsLimit == false 
                    && (
                        (config.Exceptions != null && config.Exceptions.Contains(__instance.id) == true) 
                        || 
                        (internalexceptions != null && internalexceptions.Contains(__instance.id) == true)
                       )
                   )
                {
                    return;
                }

                // Increment counters after a non-hardcoded event is finished
                if (__instance.id > 0 && __instance.id != 60367 && __instance.isFestival == false)
                {
                    ModEntry.EventCounterDay.Value++;
                    ModEntry.EventCounterRow.Value++;
                }
            }
            catch (Exception ex)
            {
                monitor.Log($"Failed in {nameof(exitEvent_postfix)}:\n{ex}", LogLevel.Error);
            }

        }
    }
}
