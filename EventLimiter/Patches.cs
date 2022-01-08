using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewValley;
using HarmonyLib;

namespace EventLimiter
{
    class Patches
    {
        private static IMonitor monitor;
        private static ModConfig config;

        public static void Hook(Harmony harmony, IMonitor monitor, ModConfig config)
        {
            Patches.monitor = monitor;
            Patches.config = config;

            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.startEvent)), 
                postfix: new HarmonyMethod(typeof(Patches), nameof(Patches.startEvent_postfix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.exitEvent)), 
                postfix: new HarmonyMethod(typeof(Patches), nameof(Patches.exitEvent_postfix)));

            monitor.Log("Initialised harmony patches...");
        }

        public static void startEvent_postfix(GameLocation __instance, Event evt)
        {
            try
            {
                if (evt.id > 0)
                {
                    if (config.Exceptions != null && config.Exceptions.Count() > 0)
                    {
                        foreach (var exceptionids in config.Exceptions)
                        {
                            if (evt.id.Equals(exceptionids) == true)
                            {
                                monitor.Log("Made exception for event with id " + evt.id);
                                return;
                            }
                        }
                    }

                    if (ModEntry.EventCounterDay.Value >= config.EventsPerDay)
                    {
                        monitor.Log("Day limit reached! Skipping event...");
                        Game1.eventUp = false;
                        Game1.displayHUD = true;
                        Game1.player.CanMove = true;
                        __instance.currentEvent = null;
                        return;
                    }

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
            if (__instance.id > 0)
            {
                ModEntry.EventCounterDay.Value++;
                ModEntry.EventCounterRow.Value++;
            }
        }
    }
}
