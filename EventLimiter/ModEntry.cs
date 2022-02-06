using System;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewModdingAPI.Events;
using StardewValley;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace EventLimiter
{
    /// <summary>The API which lets other mods add a config UI through Generic Mod Config Menu.</summary>
    public interface IGenericModConfigMenuApi
    {
        /****
        ** Must be called first
        ****/
        /// <summary>Register a mod whose config can be edited through the UI.</summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="reset">Reset the mod's config to its default values.</param>
        /// <param name="save">Save the mod's current config to the <c>config.json</c> file.</param>
        /// <param name="titleScreenOnly">Whether the options can only be edited from the title screen.</param>
        /// <remarks>Each mod can only be registered once, unless it's deleted via <see cref="Unregister"/> before calling this again.</remarks>
        void Register(IManifest mod, Action reset, Action save, bool titleScreenOnly = false);


        /****
        ** Basic options
        ****/

        /// <summary>Add a string option at the current position in the form.</summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="getValue">Get the current value from the mod config.</param>
        /// <param name="setValue">Set a new value in the mod config.</param>
        /// <param name="name">The label text to show in the form.</param>
        /// <param name="tooltip">The tooltip text shown when the cursor hovers on the field, or <c>null</c> to disable the tooltip.</param>
        /// <param name="allowedValues">The values that can be selected, or <c>null</c> to allow any.</param>
        /// <param name="formatAllowedValue">Get the display text to show for a value from <paramref name="allowedValues"/>, or <c>null</c> to show the values as-is.</param>
        /// <param name="fieldId">The unique field ID for use with <see cref="OnFieldChanged"/>, or <c>null</c> to auto-generate a randomized ID.</param>
        void AddTextOption(IManifest mod, Func<string> getValue, Action<string> setValue, Func<string> name, Func<string> tooltip = null, string[] allowedValues = null, Func<string, string> formatAllowedValue = null, string fieldId = null);

        /// <summary>Add an integer option at the current position in the form.</summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="getValue">Get the current value from the mod config.</param>
        /// <param name="setValue">Set a new value in the mod config.</param>
        /// <param name="name">The label text to show in the form.</param>
        /// <param name="tooltip">The tooltip text shown when the cursor hovers on the field, or <c>null</c> to disable the tooltip.</param>
        /// <param name="min">The minimum allowed value, or <c>null</c> to allow any.</param>
        /// <param name="max">The maximum allowed value, or <c>null</c> to allow any.</param>
        /// <param name="interval">The interval of values that can be selected.</param>
        /// <param name="formatValue">Get the display text to show for a value, or <c>null</c> to show the number as-is.</param>
        /// <param name="fieldId">The unique field ID for use with <see cref="OnFieldChanged"/>, or <c>null</c> to auto-generate a randomized ID.</param>
        void AddNumberOption(IManifest mod, Func<int> getValue, Action<int> setValue, Func<string> name, Func<string> tooltip = null, int? min = null, int? max = null, int? interval = null, Func<int, string> formatValue = null, string fieldId = null);

        /****
        ** Advanced
        ****/

        /// <summary>Add an option at the current position in the form using custom rendering logic.</summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="name">The label text to show in the form.</param>
        /// <param name="draw">Draw the option in the config UI. This is called with the sprite batch being rendered and the pixel position at which to start drawing.</param>
        /// <param name="tooltip">The tooltip text shown when the cursor hovers on the field, or <c>null</c> to disable the tooltip.</param>
        /// <param name="beforeMenuOpened">A callback raised just before the menu containing this option is opened.</param>
        /// <param name="beforeSave">A callback raised before the form's current values are saved to the config (i.e. before the <c>save</c> callback passed to <see cref="Register"/>).</param>
        /// <param name="afterSave">A callback raised after the form's current values are saved to the config (i.e. after the <c>save</c> callback passed to <see cref="Register"/>).</param>
        /// <param name="beforeReset">A callback raised before the form is reset to its default values (i.e. before the <c>reset</c> callback passed to <see cref="Register"/>).</param>
        /// <param name="afterReset">A callback raised after the form is reset to its default values (i.e. after the <c>reset</c> callback passed to <see cref="Register"/>).</param>
        /// <param name="beforeMenuClosed">A callback raised just before the menu containing this option is closed.</param>
        /// <param name="height">The pixel height to allocate for the option in the form, or <c>null</c> for a standard input-sized option. This is called and cached each time the form is opened.</param>
        /// <param name="fieldId">The unique field ID for use with <see cref="OnFieldChanged"/>, or <c>null</c> to auto-generate a randomized ID.</param>
        /// <remarks>The custom logic represented by the callback parameters is responsible for managing its own state if needed. For example, you can store state in a static field or use closures to use a state variable.</remarks>
        void AddComplexOption(IManifest mod, Func<string> name, Action<SpriteBatch, Vector2> draw, Func<string> tooltip = null, Action beforeMenuOpened = null, Action beforeSave = null, Action afterSave = null, Action beforeReset = null, Action afterReset = null, Action beforeMenuClosed = null, Func<int> height = null, string fieldId = null);

    }

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
            helper.Events.GameLoop.GameLaunched += this.GameLaunched;
            helper.Events.GameLoop.DayStarted += this.DayStarted;
            helper.Events.Input.ButtonPressed += this.ButtonPressed;
        }

        private void ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // Reset events in a row counter if needed, button press used as event because warped event won't consider some events for resetting counters
            if (EventCounterRow.Value > 0 && Game1.CurrentEvent == null && Context.CanPlayerMove == true)
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

        private void GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
            {
                return;
            }         

            // register mod
            configMenu.Register(
                mod: this.ModManifest,
                reset: () => this.config = new ModConfig(),
                save: () => this.Helper.WriteConfig(this.config)
            );

            configMenu.AddNumberOption(
                mod: this.ModManifest, 
                getValue: () => this.config.EventsPerDay, 
                setValue: value => this.config.EventsPerDay = value,
                min: 0,
                tooltip: () => "The maximum number of events shown in a day",
                name: () => "Events per day");

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                getValue: () => this.config.EventsInARow,
                setValue: value => this.config.EventsInARow = value,
                min: 0,
                tooltip: () => "The maximum number of events shown when entering a new location",
                name: () => "Events in a row");

            configMenu.AddTextOption(
                mod: this.ModManifest,
                name: () => "Exceptions",
                tooltip: () => "Event ids which will never be skipped. Enter only numbers seperated by commas",
                getValue: () => string.Join(", ", this.config.Exceptions), 
                setValue: value => this.config.Exceptions = GetExceptionsFromString(value));


        }

        private int[] GetExceptionsFromString(string value)
        {
            var formattedstring = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();

            var ints = from field in formattedstring.Where((x) => { int dummy; return Int32.TryParse(x, out dummy); })
                       select Int32.Parse(field);

            return ints.ToArray();
        }
    }
}
