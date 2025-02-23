using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using FixMyChoo.Settings;
using FixMyChoo.Patches;

namespace FixMyChoo;

[BepInPlugin(LCMPluginInfo.PLUGIN_GUID, LCMPluginInfo.PLUGIN_NAME, LCMPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log = null!;
    internal static FixMyChooSettings Settings = null!;
    internal static SavegameLoadedWatcher SavegameWatcher = null!;

    private void Awake()
    {
        Log = Logger;

        Settings = new(Config);

        // Start watching for savegame to be loaded
        SavegameWatcher = new SavegameLoadedWatcher();

        // Log our awake here so we can see it in LogOutput.txt file
        Log.LogInfo($"Plugin {LCMPluginInfo.PLUGIN_NAME} version {LCMPluginInfo.PLUGIN_VERSION} is loaded!");

        Harmony myHarmony = new(LCMPluginInfo.PLUGIN_GUID);

        if (Settings.RerailTrains.Value)
        {
            Log.LogInfo($" Patching derailed train recovery.");
            myHarmony.PatchAll(typeof(TrackTrainRerail));
        }
        if (Settings.UpdateCouplerVisualsOnLoad.Value)
        {
            myHarmony.PatchAll(typeof(TrainVisualPatches));
        }
    }

}
