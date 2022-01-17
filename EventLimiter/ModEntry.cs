using System;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewModdingAPI.Events;
using StardewValley;
using HarmonyLib;

namespace EventLimiter
{
    public class ModEntry
        : Mod
    {
        private ModConfig config;

        // Counters for event tracking
        public static readonly PerScreen<int> EventCounterDay = new PerScreen<int>();
        public static readonly PerScreen<int> EventCounterRow = new PerScreen<int>();

        public override void Entry(IModHelper helper)
        {
            var harmony = new Harmony(this.ModManifest.UniqueID);

            // Try and read config, use default values if unable
            try
            {
                this.config = helper.ReadConfig<ModConfig>();
            }
            catch (Exception ex)
            {
                this.config = new ModConfig();
                this.Monitor.Log("Error reading config, using default values...", LogLevel.Warn);
                this.Monitor.Log($"An error occured reading the config. Details:\n{ex}");
            }
            
            // Add harmony patches
            Patches.Hook(harmony, this.Monitor, this.config);

            // Add event handlers
            helper.Events.Player.Warped += this.Warped;
            helper.Events.GameLoop.DayStarted += this.DayStarted;
        }

        private void Warped (object sender, WarpedEventArgs e)
        {
            // Reset events in a row counter after warping if needed
            if (EventCounterRow.Value > 0)
            {
                EventCounterRow.Value = 0;
                this.Monitor.Log("Resetting events in a row counter");
            }
        }

        private void DayStarted(object sender, DayStartedEventArgs e)
        {
            // Reset counters each day
            EventCounterDay.Value = 0;
            EventCounterRow.Value = 0;
            this.Monitor.Log("Resetting event counters");
        }
    }
}
