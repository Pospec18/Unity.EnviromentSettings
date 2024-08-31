using UnityEngine;

namespace Pospec.EnvironmentSettings
{
    public class SettingsData
    {
        public float MasterVolume;
        public float MusicVolume;
        public float SoundVolume;
        public DetailLevel ResolutionLevel;
        public bool FullScreen;
        public bool PostProcessing;

        public SettingsData() : this(1, 1, 1, DetailLevel.Max, Screen.fullScreen, true) { }

        public SettingsData(float masterVolume, float musicVolume, float soundVolume, DetailLevel resolutionLevel, bool fullScreen, bool postProcessing)
        {
            MasterVolume = masterVolume;
            MusicVolume = musicVolume;
            SoundVolume = soundVolume;
            ResolutionLevel = resolutionLevel;
            FullScreen = fullScreen;
            PostProcessing = postProcessing;
        }
    }

    public enum DetailLevel { Low = 3, Middle = 2, High = 1, Max = 0 }
}
