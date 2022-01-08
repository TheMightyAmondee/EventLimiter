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

            this.config = helper.ReadConfig<ModConfig>();

            Patches.Hook(harmony, this.Monitor, this.config);

            helper.Events.Player.Warped += this.Warped;
            helper.Events.GameLoop.DayStarted += this.DayStarted;
            helper.ConsoleCommands.Add("get_state", "get events seen and other data tracked by the mod", this.State);
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

        private void State(string command, string[] arg)
        {
            var eventsseen = Game1.player.eventsSeen.ToString();

            this.Monitor.Log($"{eventsseen}\n {EventCounterRow.Value}, {EventCounterDay.Value}", LogLevel.Info);
        }
    }
}
