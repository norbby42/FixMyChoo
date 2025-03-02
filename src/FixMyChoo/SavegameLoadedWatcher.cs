using FixMyChoo.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FixMyChoo
{
    public class SavegameLoadedWatcher
    {
        public static bool IsLoading = false;
        public SavegameLoadedWatcher() 
        {
            SaveController.LoadingDone += OnLoadingDone;
            SaveController.LoadingStarted += OnLoadingStart;
        }

        ~SavegameLoadedWatcher()
        {
            SaveController.LoadingDone -= OnLoadingDone;
            SaveController.LoadingStarted -= OnLoadingStart;
        }

        public void OnLoadingDone()
        {
            IsLoading = false;

            Plugin.Log.LogInfo("FixMyChoo -> OnLoadingDone");

            if (Plugin.Settings.RerailTrains.Value)
            {
                TrackTrainRerail.FixupAllTrainAxles();
            }

            Plugin.instance?.SavegameLoaded();
        }

        public void OnLoadingStart()
        {
            IsLoading = true;
        }
    }
}
