# Event Limiter

Event limiter is a mod for Stardew Valley that allows for the configurable limitation of the amount of events seen by the player each day, and in a row. 
This can help with storyline immersion or in multiplayer where time continues during events. 
Skipped events will be played the next time they will trigger, provided an event limit has not been reached.
Hardcoded events (weddings) and PlayerKilled events won't be skipped.

Exclusions to which events are affected can also be configured. If there's a cutscene you never want skipped this can be done by listing its id in the Exceptions config option.

Now with GMCM support! Long exception lists will extend past the textbox in the menu though.

The mod does use Harmony, just FYI.

Installation and use:
1. Download to mod here
2. Unzip the download file and place the EventLimiter folder in your Mods folder
3. Run the game at least once to generate the config
4. Edit the config as desired and enjoy!

## Using the api ##

Version 1.2.0 added an api to allow SMAPI mods to access config data or add event exceptions.

To use the api:
1. Copy the public methods (all methods beyond the public api comment) you need access to from the EventLimiterApi class into a public interface named IEventLimiterApi in your code.

It should look something like this:
```
{
  public interface IEventLimiterApi
  {
    public int GetDayLimit();
    
    public int GetRowLimit();
    
    public List<int> GetExceptions(bool includeinternal = true);
    
    public bool AddInternalException(int eventid);
  }
}
```
2. In the GameLaunched event, call the API.

To prevent errors, when using the api ensure that the returned api is not null whenever using its methods. This ensures that Event Limiter is not needed as a dependency.

### Versions: ###
1.0.0 Initial release

1.1.0 Added Generic Mod Config Menu support

1.2.0 Added Event Limiter api
