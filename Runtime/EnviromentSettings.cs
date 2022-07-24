using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

namespace Pospec.EnviromentSettings
{
    public class EnviromentSettings : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private AudioMixer musicMixer;
        [SerializeField] private AudioMixer soundMixer;

        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider soundSlider;

        [Header("Screen")]
        [SerializeField] private TMP_Dropdown resolutionsDropdown;
        [SerializeField] private Toggle fullScreenToggle;
        [SerializeField] private Toggle postProcToggle;
        [SerializeField] private Slider brightnessSlider;

        public string saveDir => Path.Combine(Application.dataPath, "Save");
        public string savePath => Path.Combine(saveDir, "EnviromentSettings.json");
        public static SettingsData Data { get; set; }

        private static List<Resolution> _resolutions;
        private static List<Resolution> Resolutions
        {
            get
            {
                if(_resolutions == null)
                {
                    Resolution[] allRes = Screen.resolutions;
                    AspectRatio ratio = new AspectRatio(allRes[allRes.Length - 1]);
                    _resolutions = new List<Resolution>();

                    for (int i = allRes.Length - 1; i >= 0; i--)
                    {
                        if (ratio.CorrespondsTo(allRes[i]))
                            _resolutions.Add(allRes[i]);
                    }
                }
                return _resolutions;
            }
        }

        #region Setup

        private void Start()
        {
            Data = LoadData();

            SetMusicVolume(Data.MusicVolume);
            SetSoundVolume(Data.SoundVolume);
            SetResolution(Data.ResolutionLevel);
            SetFullScreen(Data.FullScreen);
            SetPostProcessing(Data.PostProcessing);

            SetupUI();
        }

        private SettingsData LoadData()
        {
            if(!File.Exists(savePath))
            {
                return new SettingsData();
            }

            try
            {
                string json = File.ReadAllText(savePath);
                return JsonUtility.FromJson<SettingsData>(json);
            }
            catch
            {
                return new SettingsData();
            }
        }

        private void SaveData(SettingsData data)
        {
            if(!Directory.Exists(saveDir))
                Directory.CreateDirectory(saveDir);
            
            string json = JsonUtility.ToJson(data);
            File.WriteAllText(savePath, json);
        }

        private void SetupUI()
        {
            if(musicSlider != null)
            {
                musicSlider.onValueChanged.AddListener(SetMusicVolume);
                musicSlider.value = Data.MusicVolume;
            }
            if(soundSlider != null)
            {
                soundSlider.onValueChanged.AddListener(SetSoundVolume);
                soundSlider.value = Data.SoundVolume;
            }
            if(resolutionsDropdown != null)
            {
                resolutionsDropdown.onValueChanged.AddListener(SetResolution);
                resolutionsDropdown.value = (int)Data.ResolutionLevel;
            }
            if(fullScreenToggle != null)
            {
                fullScreenToggle.onValueChanged.AddListener(SetFullScreen);
                fullScreenToggle.isOn = Data.FullScreen;
            }
            if(postProcToggle != null)
            {
                postProcToggle.onValueChanged.AddListener(SetPostProcessing);
                postProcToggle.isOn = Data.PostProcessing;
            }
            if(brightnessSlider != null)
            {
                brightnessSlider.onValueChanged.AddListener(SetBrightness);
                brightnessSlider.value = Screen.brightness;
            }
        }

        private void OnValidate()
        {
            if(musicSlider != null)
            {
                musicSlider.minValue = 0.0001f;
                musicSlider.maxValue = 1;
                musicSlider.wholeNumbers = false;
                musicSlider.interactable = true;
            }

            if (soundSlider != null)
            {
                soundSlider.minValue = 0.0001f;
                soundSlider.maxValue = 1;
                soundSlider.wholeNumbers = false;
                soundSlider.interactable = true;
            }

            if (resolutionsDropdown != null)
            {
                resolutionsDropdown.ClearOptions();

                List<string> resText = new List<string>();
                foreach (DetailLevel detail in Enum.GetValues(typeof(DetailLevel)))
                {
                    Resolution res = GetResolution(detail);
                    resText.Add(string.Format("{0} ({1} x {2})", detail.ToString(), res.width, res.height));
                }

                resolutionsDropdown.AddOptions(resText);
                resolutionsDropdown.RefreshShownValue();
                resolutionsDropdown.interactable = true;
            }

            if(fullScreenToggle != null)
            {
                fullScreenToggle.interactable = true;
            }

            if (brightnessSlider != null)
            {
                brightnessSlider.minValue = 0;
                brightnessSlider.maxValue = 1;
                brightnessSlider.wholeNumbers = false;
                brightnessSlider.interactable = true;
            }
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
            if(postProcToggle != null)
            {
                postProcToggle.onValueChanged.RemoveListener(SetPostProcessing);
            }
            if (brightnessSlider != null)
            {
                brightnessSlider.onValueChanged.RemoveListener(SetBrightness);
            }
        }

        #endregion

        #region Audio

        public void SetMusicVolume(float volume)
        {
            musicMixer?.SetFloat("Volume", SliderToMixer(volume));
            Data.MusicVolume = volume;
            SaveData(Data);
        }

        public void SetSoundVolume(float volume)
        {
            soundMixer?.SetFloat("Volume", SliderToMixer(volume));
            Data.SoundVolume = volume;
            SaveData(Data);
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
            Resolution current = GetResolution(detailLevel);
            Screen.SetResolution(current.width, current.height, Screen.fullScreen);
            Data.ResolutionLevel = (DetailLevel)detailLevel;
            SaveData(Data);
        }

        public void SetFullScreen(bool fullScreen)
        {
            Screen.fullScreen = fullScreen;
            Data.FullScreen = fullScreen;
            SaveData(Data);
        }

        public void SetPostProcessing(bool isOn)
        {
            Data.PostProcessing = isOn;
            SaveData(Data);
        }

        private void SetBrightness(float brightness)
        {
            brightness = Mathf.Clamp01(brightness);
            Screen.brightness = brightness;
        }

        public static Resolution GetResolution(DetailLevel detail)
        {
            return GetResolution((int)detail);
        }

        public static Resolution GetResolution(int detail)
        {
            int i = detail * (Resolutions.Count - 1) / (int)DetailLevel.Low;
            return Resolutions[i];
        }

        #endregion
    }
}
