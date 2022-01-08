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

        public static readonly PerScreen<int> EventCounterDay = new PerScreen<int>();
        public static readonly PerScreen<int> EventCounterRow = new PerScreen<int>();

        public override void Entry(IModHelper helper)
        {
            var harmony = new Harmony(this.ModManifest.UniqueID);

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
            

            Patches.Hook(harmony, this.Monitor, this.config);

            helper.Events.Player.Warped += this.Warped;
            helper.Events.GameLoop.DayStarted += this.DayStarted;
        }

        private void Warped (object sender, WarpedEventArgs e)
        {
            if (EventCounterRow.Value > 0)
            {
                EventCounterRow.Value = 0;
                this.Monitor.Log("Resetting events in a row counter");
            }
        }

        private void DayStarted(object sender, DayStartedEventArgs e)
        {
            EventCounterDay.Value = 0;
            EventCounterRow.Value = 0;
            this.Monitor.Log("Resetting event counters");
        }
    }
}
