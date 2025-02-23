using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using static TrackTrain.TrainData;

namespace FixMyChoo.Patches
{
    public class TrackTrainRerail
    {
        private static Dictionary<TrackTrain.TrainAxle, Track_Segment.TrackSegmentReference> AxleDefaultSegments = new Dictionary<TrackTrain.TrainAxle, Track_Segment.TrackSegmentReference>();

        // Called after savegame has finished deserializing
        public static void FixupAllTrainAxles()
        {
            foreach (var item in TrainController.instance.trackTrains)
            {
                TrackTrain train = item.Value as TrackTrain;
                bool rerailed = false;
                bool changed = false;
                if (train.frontAxle != null && train.frontAxle.currentSegment.segment == null)
                {
                    if (AxleDefaultSegments.TryGetValue(train.frontAxle, out Track_Segment.TrackSegmentReference Segment))
                    {
                        Plugin.Log.LogInfo($"Rerailing front axle on train {train.GUID} to its default starting rail.");
                        train.frontAxle.currentSegment = new Track_Segment.TrackSegmentReference(Segment.segment);
                        rerailed = true;
                        changed = true;
                    }
                }
                if (train.backAxle != null && train.backAxle.currentSegment.segment == null)
                {
                    if (AxleDefaultSegments.TryGetValue(train.backAxle, out Track_Segment.TrackSegmentReference Segment))
                    {
                        Plugin.Log.LogInfo($"Rerailing back axle on train {train.GUID} to its default starting rail.");
                        train.backAxle.currentSegment = new Track_Segment.TrackSegmentReference(Segment.segment);
                        if (train.frontAxle != null && train.frontAxle.currentSegment.segment == null)
                        {
                            train.frontAxle.currentSegment = new Track_Segment.TrackSegmentReference(Segment.segment);
                        }
                        changed = true;
                    }
                    else if (rerailed && train.frontAxle != null)
                    {
                        train.backAxle.currentSegment = new Track_Segment.TrackSegmentReference(train.frontAxle.currentSegment.segment);
                        changed = true;
                    }
                }

                if (changed)
                {
                    train.CalculatePositionAndRotation();
                }
            }
        }

        // Cache the default values of the track segments, as set in the level data.
        // Start is called before any savedata is deserialized
        [HarmonyPatch(typeof(TrackTrain), nameof(TrackTrain.Start))]
        [HarmonyPostfix]
        static void TrackTrain_Start_Postfix(TrackTrain __instance)
        {
            // TrackTrain.Start can be called multiple times.
            if (__instance.frontAxle.currentSegment.segment != null && !AxleDefaultSegments.ContainsKey(__instance.frontAxle))
            {
                AxleDefaultSegments.Add(__instance.frontAxle, __instance.frontAxle.currentSegment);
            }
            if (__instance.backAxle.currentSegment.segment != null && !AxleDefaultSegments.ContainsKey(__instance.backAxle))
            {
                AxleDefaultSegments.Add(__instance.backAxle, __instance.backAxle.currentSegment);
            }

            //Plugin.Log.LogInfo($"Default front axle segment of train {__instance.GUID} is {__instance.frontAxle.currentSegment.GUID}, default rear axle segment is {__instance.backAxle.currentSegment.GUID}");
        }
    }
}
