using BepInEx.Configuration;

namespace FixMyChoo.Settings;

public class FixMyChooSettings(ConfigFile config)
{
    public ConfigEntry<bool> RerailTrains = config.Bind<bool>("Minitrain Functionality", "RerailTrains", true, "On loading into the level, derailed trains will rerail themselves.");
    public ConfigEntry<bool> UpdateCouplerVisualsOnLoad = config.Bind<bool>("QoL", "UpdateCouplerVisualsOnLoad", true, "Make couplers be in the correct position on loading the level.");
}
