using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using static TrackTrain.TrainData;

namespace FixMyChoo.Patches
{
    public class TrainVisualPatches
    {
        // Force update the axles and couplers of attached wagons after loading the game (ie those that do not have their own train instance)
        [HarmonyPatch(typeof(TrackTrain), nameof(TrackTrain.OnLoadingGameDelay))]
        [HarmonyPostfix]
        static IEnumerator TrackTrain_OnLoadingGameDelay_Postfix(IEnumerator __result, TrackTrain __instance)
        {
            // Wait until the base coroutine has finished running over all frames.
            while (__result.MoveNext())
            {
                yield return __result.Current;
            }

            if (__instance.train.wagons.Count > 1)
            {
                foreach (WagonData wagon in __instance.train.wagons)
                {
                    if (wagon.wagon != __instance)
                    {
                        wagon.wagon.backAxle.OnLoadGame();
                        wagon.wagon.frontAxle.OnLoadGame();
                        wagon.wagon.frontCoupler?.OnLoadGame();
                        wagon.wagon.backCoupler?.OnLoadGame();
                    }
                }
            }
        }
    }
}
