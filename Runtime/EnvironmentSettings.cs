using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

namespace Pospec.EnvironmentSettings
{
    public class EnvironmentSettings : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField, Tooltip("Expose parameters '" + masterVolumeName + "' , '" + musicVolumeName + "' and '" + soundVolumeName + "' on AudioMixer volume tabs")]
        private AudioMixer audioMixer;

        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider soundSlider;

        [Header("Screen")]
        [SerializeField] private TMP_Dropdown resolutionsDropdown;
        [SerializeField] private Toggle fullScreenToggle;
        [SerializeField] private List<RuntimePlatform> ignoreResolutionsPlatforms;

        public event Action<float> onMasterChanged;
        public event Action<float> onMusicChanged;
        public event Action<float> onSoundChanged;
        public event Action<DetailLevel> onResolutionChanged;
        public event Action<bool> onFullScreenChanged;
        public event Action onChanged;

        private const string masterVolumeName = "MasterVolume";
        private const string musicVolumeName = "MusicVolume";
        private const string soundVolumeName = "SoundVolume";

        public static string savePath => Path.Combine(Application.persistentDataPath, "EnvironmentSettings.json");

        private static SettingsData _data;
        public static SettingsData Data
        {
            get
            {
                if (_data == null)
                    _data = LoadData();
                return _data;
            }
        }

        private static List<Resolution> _resolutions;
        public static List<Resolution> Resolutions
        {
            get
            {
                if (_resolutions == null)
                {
                    _resolutions = new List<Resolution>();
                    try
                    {
                        Resolution[] allRes = Screen.resolutions;
                        AspectRatio ratio = new AspectRatio(allRes[allRes.Length - 1]);

                        for (int i = allRes.Length - 1; i >= 0; i--)
                        {
                            if (ratio.CorrespondsTo(allRes[i]))
                                _resolutions.Add(allRes[i]);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Error while setting native resolutions: " + ex.Message);
                    }
                }
                return _resolutions;
            }
        }

        #region Setup

        private void Start()
        {
            try
            {
                SetMasterVolume(Data.MasterVolume);
                SetMusicVolume(Data.MusicVolume);
                SetSoundVolume(Data.SoundVolume);
                SetFullScreen(Data.FullScreen);
                SetupResolutionDropdown();
                SetResolution(Data.ResolutionLevel);
            }
            catch (Exception ex)
            {
                Debug.LogError("Error while applying settings: " + ex.Message);
            }

            SetupUI();
        }

        private static SettingsData LoadData()
        {
            try
            {
                if (!File.Exists(savePath))
                {
                    return new SettingsData();
                }

                string json = File.ReadAllText(savePath);
                return JsonUtility.FromJson<SettingsData>(json);
            }
            catch
            {
                return new SettingsData();
            }
        }

        private void SaveData()
        {
            SaveData(Data);
        }

        private void SaveData(SettingsData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data);
                File.WriteAllText(savePath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError("Error while saving settings data: " + ex.Message);
            }
        }

        private void SetupUI()
        {
            if (masterSlider != null)
            {
                masterSlider.onValueChanged.AddListener(SetMasterVolume);
                masterSlider.value = Data.MasterVolume;
            }
            if (musicSlider != null)
            {
                musicSlider.onValueChanged.AddListener(SetMusicVolume);
                musicSlider.value = Data.MusicVolume;
            }
            if (soundSlider != null)
            {
                soundSlider.onValueChanged.AddListener(SetSoundVolume);
                soundSlider.value = Data.SoundVolume;
            }
            if (resolutionsDropdown != null)
            {
                resolutionsDropdown.onValueChanged.AddListener(SetResolution);
                resolutionsDropdown.value = (int)Data.ResolutionLevel;
            }
            if (fullScreenToggle != null)
            {
                fullScreenToggle.onValueChanged.AddListener(SetFullScreen);
                fullScreenToggle.isOn = Data.FullScreen;
            }
        }

        private void OnValidate()
        {
            if (masterSlider != null)
            {
                masterSlider.minValue = 0.0001f;
                masterSlider.maxValue = 1;
                masterSlider.wholeNumbers = false;
                masterSlider.interactable = true;
                if (audioMixer == null)
                    Debug.LogWarning("No mixer", this);
                else
                    audioMixer.GetFloat(masterVolumeName, out _);
            }

            if (musicSlider != null)
            {
                musicSlider.minValue = 0.0001f;
                musicSlider.maxValue = 1;
                musicSlider.wholeNumbers = false;
                musicSlider.interactable = true;
                if (audioMixer == null)
                    Debug.LogWarning("No mixer", this);
                else
                    audioMixer.GetFloat(musicVolumeName, out _);
            }

            if (soundSlider != null)
            {
                soundSlider.minValue = 0.0001f;
                soundSlider.maxValue = 1;
                soundSlider.wholeNumbers = false;
                soundSlider.interactable = true;
                if (audioMixer == null)
                    Debug.LogWarning("No mixer", this);
                else
                    audioMixer.GetFloat(soundVolumeName, out _);
            }

            if (resolutionsDropdown != null)
            {
                SetupResolutionDropdown();
                resolutionsDropdown.interactable = true;
            }

            if (fullScreenToggle != null)
            {
                fullScreenToggle.interactable = true;
            }
        }

        private void SetupResolutionDropdown()
        {
            if (resolutionsDropdown == null)
                return;

            resolutionsDropdown.ClearOptions();
            if (Resolutions.Count == 0)
            {
                resolutionsDropdown.RefreshShownValue();
                return;
            }

            List<string> resText = new List<string>();
            foreach (DetailLevel detail in Enum.GetValues(typeof(DetailLevel)))
            {
                Resolution res = GetResolution(detail);
                resText.Add(string.Format("{0} ({1} x {2})", detail.ToString(), res.width, res.height));
            }

            resolutionsDropdown.AddOptions(resText);
            resolutionsDropdown.RefreshShownValue();
        }

        private void OnDestroy()
        {
            if (musicSlider != null)
            {
                musicSlider.onValueChanged.RemoveListener(SetMusicVolume);
            }
            if (soundSlider != null)
            {
                soundSlider.onValueChanged.RemoveListener(SetSoundVolume);
            }
            if (resolutionsDropdown != null)
            {
                resolutionsDropdown.onValueChanged.RemoveListener(SetResolution);
            }
            if (fullScreenToggle != null)
            {
                fullScreenToggle.onValueChanged.RemoveListener(SetFullScreen);
            }
        }

        #endregion

        #region Audio

        public void SetMasterVolume(float volume)
        {
            audioMixer?.SetFloat(masterVolumeName, SliderToMixer(volume));
            Data.MasterVolume = volume;
            onMasterChanged?.Invoke(volume);
            ValueChanged();
        }


        public void SetMusicVolume(float volume)
        {
            audioMixer?.SetFloat(musicVolumeName, SliderToMixer(volume));
            Data.MusicVolume = volume;
            onMusicChanged?.Invoke(volume);
            ValueChanged();
        }

        public void SetSoundVolume(float volume)
        {
            audioMixer?.SetFloat(soundVolumeName, SliderToMixer(volume));
            Data.SoundVolume = volume;
            onSoundChanged?.Invoke(volume);
            ValueChanged();
        }

        private static float SliderToMixer(float sliderVal) => Mathf.Log10(sliderVal) * 20;

        #endregion

        #region Screen

        public void SetResolution(DetailLevel detailLevel)
        {
            SetResolution((int)detailLevel);
        }

        private void SetResolution(int detailLevel)
        {
            if (resolutionsDropdown == null || Resolutions.Count == 0 || ignoreResolutionsPlatforms.Contains(Application.platform))
                return;

            Debug.Log("changing resolution to " + detailLevel.ToString());

            Resolution current = GetResolution(detailLevel);
            Screen.SetResolution(current.width, current.height, Screen.fullScreen);
            Data.ResolutionLevel = (DetailLevel)detailLevel;
            onResolutionChanged?.Invoke((DetailLevel)detailLevel);
            ValueChanged();
        }

        public void SetFullScreen(bool fullScreen)
        {
            Debug.Log("changing fullscreen to " + fullScreen.ToString());

            Screen.fullScreen = fullScreen;
            Data.FullScreen = fullScreen;
            onFullScreenChanged?.Invoke(fullScreen);
            ValueChanged();
        }

        public static Resolution GetResolution(DetailLevel detail)
        {
            return GetResolution((int)detail);
        }

        public static Resolution GetResolution(int detail)
        {
            if (Resolutions.Count == 0)
                return Screen.currentResolution;

            int i = detail * (Resolutions.Count - 1) / (int)DetailLevel.Low;
            return Resolutions[i];
        }

        #endregion

        private void ValueChanged()
        {
            SaveData();
            onChanged?.Invoke();
        }
    }
}
