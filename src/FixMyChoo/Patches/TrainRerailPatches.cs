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
        private struct DefaultPositionHolder
        {
            public Track_Segment.TrackSegmentReference? TrackSegment;
            public float Position;
            public Track_Segment.Direction Direction;

            public DefaultPositionHolder()
            {
                TrackSegment = null;
                Position = 0;
                Direction = Track_Segment.Direction.Forward;
            }

            public DefaultPositionHolder(Track_Segment.TrackSegmentReference segment, float position, Track_Segment.Direction direction)
            {
                TrackSegment = segment;
                Position = position;
                Direction = direction;
            }
        }

        private static Dictionary<TrackTrain.TrainAxle, DefaultPositionHolder> AxleDefaultSegments = new Dictionary<TrackTrain.TrainAxle, DefaultPositionHolder>();

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
                    if (AxleDefaultSegments.TryGetValue(train.frontAxle, out DefaultPositionHolder holder) && holder.TrackSegment != null)
                    {
                        Plugin.Log.LogInfo($"Rerailing front axle on train {train.GUID} to its default starting rail.");
                        train.frontAxle.currentSegment = new Track_Segment.TrackSegmentReference(holder.TrackSegment.segment);
                        train.frontAxle.positionOnSegment = holder.Position;
                        train.frontAxle.directionOnSegment = holder.Direction;
                        rerailed = true;
                        changed = true;
                    }
                }
                if (train.backAxle != null && train.backAxle.currentSegment.segment == null)
                {
                    if (AxleDefaultSegments.TryGetValue(train.backAxle, out DefaultPositionHolder holder) && holder.TrackSegment != null)
                    {
                        Plugin.Log.LogInfo($"Rerailing back axle on train {train.GUID} to its default starting rail.");
                        train.backAxle.currentSegment = new Track_Segment.TrackSegmentReference(holder.TrackSegment.segment);
                        train.backAxle.positionOnSegment = holder.Position;
                        train.backAxle.directionOnSegment = holder.Direction;
                        if (train.frontAxle != null && train.frontAxle.currentSegment.segment == null)
                        {
                            train.frontAxle.currentSegment = new Track_Segment.TrackSegmentReference(holder.TrackSegment.segment);
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
                AxleDefaultSegments.Add(__instance.frontAxle, 
                    new DefaultPositionHolder(__instance.frontAxle.currentSegment, __instance.frontAxle.positionOnSegment, __instance.frontAxle.directionOnSegment));
            }
            if (__instance.backAxle.currentSegment.segment != null && !AxleDefaultSegments.ContainsKey(__instance.backAxle))
            {
                AxleDefaultSegments.Add(__instance.backAxle,
                    new DefaultPositionHolder(__instance.backAxle.currentSegment, __instance.backAxle.positionOnSegment, __instance.backAxle.directionOnSegment));
            }

            //Plugin.Log.LogInfo($"Default front axle segment of train {__instance.GUID} is {__instance.frontAxle.currentSegment.GUID}, default rear axle segment is {__instance.backAxle.currentSegment.GUID}");
        }
    }
}
