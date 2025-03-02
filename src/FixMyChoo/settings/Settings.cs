using BepInEx.Configuration;
using System.Collections.Generic;

namespace FixMyChoo.Settings;

public class FixMyChooSettings(ConfigFile config)
{
    public ConfigEntry<bool> RerailTrains = config.Bind<bool>("Minitrain Functionality", "RerailTrains", true, "On loading into the level, derailed trains will rerail themselves.");
    public ConfigEntry<bool> DecoupleGhostTrains = config.Bind<bool>("Minitrain Functionality", "DecoupleGhostTrains", true, 
        "On loading into the level, fix any ghost-coupled minitrains/wagons.");
    public ConfigEntry<float> GhostTrainCouplerDistance = config.Bind<float>("Minitrain Functionality", "GhostTrainCouplerDistance", 2f,
        "Distance between couplers (in meters) to consider a connection between 2 minitrains/wagons to be a 'ghost' connection.");
    public ConfigEntry<bool> UpdateCouplerVisualsOnLoad = config.Bind<bool>("QoL", "UpdateCouplerVisualsOnLoad", true, "Make couplers be in the correct position on loading the level.");
}
