using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pospec.EnviromentSettings
{
    public class SettingsData
    {
        public float MusicVolume;
        public float SoundVolume;
        public DetailLevel ResolutionLevel;
        public bool FullScreen;
        public bool PostProcessing;

        public SettingsData()
        {
            MusicVolume = 1;
            SoundVolume = 1;
            ResolutionLevel = DetailLevel.Max;
            FullScreen = Screen.fullScreen;
            PostProcessing = true;
        }

        public SettingsData(float musicVolume, float soundVolume, DetailLevel resolutionLevel, bool fullScreen, bool postProcessing)
        {
            MusicVolume = musicVolume;
            SoundVolume = soundVolume;
            ResolutionLevel = resolutionLevel;
            FullScreen = fullScreen;
            PostProcessing = postProcessing;
        }
    }

    public enum DetailLevel { Low = 3, Middle = 2, High = 1, Max = 0 }
}
