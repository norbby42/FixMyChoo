using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;

namespace FixMyChoo.Patches
{
    public class TrackTrainRerail
    {
        public static Dictionary<TrackTrain.TrainAxle, Track_Segment.TrackSegmentReference> AxleDefaultSegments = new Dictionary<TrackTrain.TrainAxle, Track_Segment.TrackSegmentReference>();

        // Start is the last method called before savedata is deserialized in TrackTrain.OnLoadingGame.
        // This enables us to cache the default values of the track segments, as set in the level data.
        [HarmonyPatch(typeof(TrackTrain), nameof(TrackTrain.Start))]
        [HarmonyPostfix]
        static void TrackTrain_Start_Postfix(TrackTrain __instance)
        {
            Plugin.Log.LogInfo($"Default front axle segment of train {__instance.GUID} is {__instance.frontAxle.currentSegment.GUID}, default reare axle segment is {__instance.backAxle.currentSegment.GUID}");
            AxleDefaultSegments.Add(__instance.frontAxle, __instance.frontAxle.currentSegment);
            AxleDefaultSegments.Add(__instance.backAxle, __instance.backAxle.currentSegment);
        }

        // The axles run OnLoadGame a few frames after the save data is deserialized (presumably to allow dynamically spawned tracks to finish spawning in)
        [HarmonyPatch(typeof(TrackTrain.TrainAxle), nameof(TrackTrain.TrainAxle.OnLoadGame))]
        [HarmonyPostfix]
        static void TrackTrain_TrainAxle_OnLoadGame_Postfix(TrackTrain.TrainAxle __instance)
        {
            if (__instance.currentSegment.segment == null)
            {
                if (AxleDefaultSegments.TryGetValue(__instance, out Track_Segment.TrackSegmentReference Segment))
                {
                    Plugin.Log.LogInfo("Rerailing axle to its default starting rail.");
                    __instance.currentSegment.segment = Segment.segment;
                }
                else
                {
                    Plugin.Log.LogWarning("Failed to find a default track segment to rerail axle to.");
                }
            }
        }
    }
}
