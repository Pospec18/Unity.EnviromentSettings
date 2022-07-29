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

        public event Action<float> onMusicChanged;
        public event Action<float> onSoundChanged;
        public event Action<DetailLevel> onResolutionChanged;
        public event Action<bool> onFullScreenChanged;
        public event Action<bool> onPostProcChanged;
        public event Action<float> onBrightnessChanged;
        public event Action onChanged;

        public string savePath => Path.Combine(Application.persistentDataPath, "EnviromentSettings.json");
        public static SettingsData Data { get; set; }

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

        private void Awake()
        {
            Data = LoadData();
        }

        private void Start()
        {
            SetupResolutionDropdown();
            SetMusicVolume(Data.MusicVolume);
            SetSoundVolume(Data.SoundVolume);
            SetResolution(Data.ResolutionLevel);
            SetFullScreen(Data.FullScreen);
            SetPostProcessing(Data.PostProcessing);

            SetupUI();
        }

        private SettingsData LoadData()
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
            catch(Exception ex)
            {
                Debug.LogError("Error while saving settings data: " + ex.Message);
            }
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
                SetupResolutionDropdown();
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
            onMusicChanged?.Invoke(volume);
            ValueChanged();
        }

        public void SetSoundVolume(float volume)
        {
            soundMixer?.SetFloat("Volume", SliderToMixer(volume));
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
            if (Resolutions.Count == 0)
                return;

            Resolution current = GetResolution(detailLevel);
            Screen.SetResolution(current.width, current.height, Screen.fullScreen);
            Data.ResolutionLevel = (DetailLevel)detailLevel;
            onResolutionChanged?.Invoke((DetailLevel)detailLevel);
            ValueChanged();
        }

        public void SetFullScreen(bool fullScreen)
        {
            Screen.fullScreen = fullScreen;
            Data.FullScreen = fullScreen;
            onFullScreenChanged?.Invoke(fullScreen);
            ValueChanged();
        }

        public void SetPostProcessing(bool postProc)
        {
            Data.PostProcessing = postProc;
            onPostProcChanged?.Invoke(postProc);
            ValueChanged();
        }

        private void SetBrightness(float brightness)
        {
            brightness = Mathf.Clamp01(brightness);
            Screen.brightness = brightness;
            onBrightnessChanged?.Invoke(brightness);
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
