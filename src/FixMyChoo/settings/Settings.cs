using BepInEx.Configuration;

namespace FixMyChoo.Settings;

public class FixMyChooSettings(ConfigFile config)
{
    public ConfigEntry<bool> RerailTrains = config.Bind<bool>("Derailed Trains", "RerailTrains", true, "On loading into the level, derailed trains will rerail themselves.");
}
