using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using FixMyChoo.Settings;
using FixMyChoo.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

namespace FixMyChoo;

[BepInPlugin(LCMPluginInfo.PLUGIN_GUID, LCMPluginInfo.PLUGIN_NAME, LCMPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log = null!;
    internal static FixMyChooSettings Settings = null!;
    internal static SavegameLoadedWatcher SavegameWatcher = null!;

    public static Plugin? instance;

    private void Awake()
    {
        instance = this;
        Log = Logger;

        Settings = new(Config);

        // Start watching for savegame to be loaded
        SavegameWatcher = new SavegameLoadedWatcher();

        // Log our awake here so we can see it in LogOutput.txt file
        Log.LogInfo($"Plugin {LCMPluginInfo.PLUGIN_NAME} version {LCMPluginInfo.PLUGIN_VERSION} is loaded!");

        Harmony myHarmony = new(LCMPluginInfo.PLUGIN_GUID);

        if (Settings.RerailTrains.Value)
        {
            Log.LogInfo($" Patching derailed train recovery (YOU PROBABLY SHOULDN'T BE DOING THIS!).");
            myHarmony.PatchAll(typeof(TrackTrainRerail));
        }
    }

    public void SavegameLoaded()
    {
        StartCoroutine(DelayedDecoupleGhostTrains());
    }

    private IEnumerator DelayedDecoupleGhostTrains()
    {
        // TrackTrain takes 7 frames to finish setup post gameload.  To not step on its toes, we wait 8 frames before decoupling trains
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        if (Plugin.Settings.DecoupleGhostTrains.Value)
        {
            DecoupleGhostTrainsByDistance();
        }
    }

    public static void DecoupleGhostTrainsByDistance()
    {
        foreach (TrackTrain tr in TrainController.instance.trackTrains.Values)
        {
            if (tr.train.wagons.Count > 1)
            {
                // Conceptually belongs to a train (meaning a consist of 2 or more wagons that move together)
                // Might be a ghost train (ie where the wagons are not physically adjacent), let's find out by checking our couplers

                Log.LogInfo($"Checking train wagon {tr.name} {tr.GUID}");

                TrackTrain.TrainCoupler frontCoupler = tr.frontCoupler;
                TrackTrain.TrainCoupler backCoupler = tr.backCoupler;

                if (frontCoupler.coupledTo != null && frontCoupler.GetDistance(true) > Plugin.Settings.GhostTrainCouplerDistance.Value)
                {
                    Log.LogInfo($"  Front Coupler was connected at too great a distance.  Disconnected.");
                    frontCoupler.Release(false);
                }
                
                if (backCoupler.coupledTo != null && backCoupler.GetDistance(true) > Plugin.Settings.GhostTrainCouplerDistance.Value)
                {
                    Log.LogInfo($"  Rear Coupler was connected at too great a distance.  Disconnected.");
                    backCoupler.Release(false);
                }

                // At this point, we may have decoupled from 1 or both sides
                // But we need to make sure that we aren't still a member of a train that we shouldn't be connected to via coupler
                TrackTrain.TrainCoupler.Type lowerIndiceSide = TrackTrain.TrainCoupler.Type.None;
                if (tr.train.wagons.Count > 1)
                {
                    var index = tr.train.GetPosition(tr);
                    if (index > 1)
                    {
                        // We are coupled on the lower indiced side.  Theoretically
                        TrackTrain otherTr = tr.train.wagons[index - 1].wagon;
                        if (otherTr.frontCoupler.coupledTo == frontCoupler)
                        {
                            lowerIndiceSide = TrackTrain.TrainCoupler.Type.Front;
                        }
                        else if (otherTr.frontCoupler.coupledTo == backCoupler)
                        {
                            lowerIndiceSide = TrackTrain.TrainCoupler.Type.Back;
                        }
                        
                        if (otherTr.backCoupler.coupledTo == frontCoupler)
                        {
                            lowerIndiceSide = TrackTrain.TrainCoupler.Type.Front;
                        }
                        else if (otherTr.backCoupler.coupledTo == backCoupler)
                        {
                            lowerIndiceSide = TrackTrain.TrainCoupler.Type.Back;
                        }

                        if (lowerIndiceSide == TrackTrain.TrainCoupler.Type.None)
                        {
                            // We are somehow in the same consist as the adjacent wagon, but we aren't actually coupled to it!
                            // Solution: Detach from that side of the consist
                            Log.LogInfo($"  Ghost connected to wagon {otherTr.name} {otherTr.GUID} on the 'lower indice' side, but neither coupler is connected to that wagon.  Disconnected.");
                            tr.train.SplitTrain(0);
                        }
                    }
                }

                // Check again, but on the upper indice side
                TrackTrain.TrainCoupler.Type upperIndiceSide = TrackTrain.TrainCoupler.Type.None;
                if (tr.train.wagons.Count > 1)
                {
                    var index = tr.train.GetPosition(tr);
                    if (index < tr.train.wagons.Count - 1)
                    {
                        // We are coupled on the upper indiced side.  Theoretically
                        TrackTrain otherTr = tr.train.wagons[index + 1].wagon;
                        if (otherTr.frontCoupler.coupledTo == frontCoupler)
                        {
                            upperIndiceSide = TrackTrain.TrainCoupler.Type.Front;
                        }
                        else if (otherTr.frontCoupler.coupledTo == backCoupler)
                        {
                            upperIndiceSide = TrackTrain.TrainCoupler.Type.Back;
                        }

                        if (otherTr.backCoupler.coupledTo == frontCoupler)
                        {
                            upperIndiceSide = TrackTrain.TrainCoupler.Type.Front;
                        }
                        else if (otherTr.backCoupler.coupledTo == backCoupler)
                        {
                            upperIndiceSide = TrackTrain.TrainCoupler.Type.Back;
                        }

                        if (upperIndiceSide == TrackTrain.TrainCoupler.Type.None)
                        {
                            // We are somehow in the same consist as the adjacent wagon, but we aren't actually coupled to it!
                            // Solution: Detach from that side of the consist
                            Log.LogInfo($"  Ghost connected to wagon {otherTr.name} {otherTr.GUID} on the 'upper indice' side, but neither coupler is connected to that wagon.  Disconnected.");
                            tr.train.SplitTrain(index);
                        }
                    }
                }
            }
        }
    }
}
