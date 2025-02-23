using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using FixMyChoo.Settings;
using FixMyChoo.Patches;

namespace FixMyChoo;

/*
  Here are some basic resources on code style and naming conventions to help
  you in your first CSharp plugin!

  https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions
  https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names
  https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-namespaces
*/

[BepInPlugin(LCMPluginInfo.PLUGIN_GUID, LCMPluginInfo.PLUGIN_NAME, LCMPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log = null!;
    internal static FixMyChooSettings Settings = null!;

    private void Awake()
    {
        Log = Logger;

        Settings = new(Config);

        // Log our awake here so we can see it in LogOutput.txt file
        Log.LogInfo($"Plugin {LCMPluginInfo.PLUGIN_NAME} version {LCMPluginInfo.PLUGIN_VERSION} is loaded!");

        Harmony myHarmony = new(LCMPluginInfo.PLUGIN_GUID);

        if (Settings.RerailTrains.Value)
        {
            Log.LogInfo($" Patching derailed train recovery.");
            myHarmony.PatchAll(typeof(TrackTrainRerail));
        }
        

    }

}
