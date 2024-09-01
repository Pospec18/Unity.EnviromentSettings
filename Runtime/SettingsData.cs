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

        public SettingsData() : this(1, 1, 1, DetailLevel.Max, Screen.fullScreen) { }

        public SettingsData(float masterVolume, float musicVolume, float soundVolume, DetailLevel resolutionLevel, bool fullScreen)
        {
            MasterVolume = masterVolume;
            MusicVolume = musicVolume;
            SoundVolume = soundVolume;
            ResolutionLevel = resolutionLevel;
            FullScreen = fullScreen;
        }
    }

    public enum DetailLevel { Low = 3, Middle = 2, High = 1, Max = 0 }
}
