using FixMyChoo.Patches;
using System;
using System.Collections.Generic;
using System.Text;

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

            if (Plugin.Settings.RerailTrains.Value)
            {
                TrackTrainRerail.FixupAllTrainAxles();
            }
            
        }

        public void OnLoadingStart()
        {
            IsLoading = true;
        }
    }
}
