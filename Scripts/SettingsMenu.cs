using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour {

    public AudioMixer audioMixer;
    Resolution[] resolutions;
    public Dropdown resolutionOptions;

    private void Start()
    {
        resolutions = Screen.resolutions;
        resolutionOptions.ClearOptions();
        List<string> options = new List<string>();
        int currentRESIndex = 0;
        for (int i = 0; i < resolutions.Length ; i++)
        {
            string option = resolutions[i].width + " X " + resolutions[i].height;
            options.Add(option);

            if(resolutions[i].width == Screen.currentResolution.width && resolutions[i].height==Screen.currentResolution.height)
            {
                currentRESIndex = i;
            }
        }
        resolutionOptions.AddOptions(options);
        resolutionOptions.value = currentRESIndex;
        resolutionOptions.RefreshShownValue();
    }

    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("Volume", volume);
    }

    public void SetGraphics(int index)
    {
        QualitySettings.SetQualityLevel(index);
    }

    public void SetFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }

    public void ChangeResolution(int index)
    {
        Screen.SetResolution(resolutions[index].width, resolutions[index].height, Screen.fullScreen);
    }
}
