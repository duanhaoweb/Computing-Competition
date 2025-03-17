using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    public GameObject settingPanel;
    public Toggle fullscreenToggle;
    public Text toggleLabel;
    public TMP_Dropdown resolutionDropdown;

    private Resolution[] availableResolutions;
    private Resolution defaultResolution;
    public Button defaultButton;
    public Button closeButton;

    public static SettingManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeResolutions();
        fullscreenToggle.isOn = Screen.fullScreenMode == FullScreenMode.FullScreenWindow;
        UpdateToggleLabel(fullscreenToggle.isOn);

        fullscreenToggle.onValueChanged.AddListener(SetDisplayMode);
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        closeButton.onClick.AddListener(CloseSetting);
        defaultButton.onClick.AddListener(ResetSetting);

        settingPanel.SetActive(true);
    }

    public void ShowSettingPanel() 
    {
        settingPanel.SetActive(true);
    }

    void InitializeResolutions() 
    {
        availableResolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        var resolutionMap = new Dictionary<string, Resolution>();
        int curentResolutionIndex = 0;
        foreach (var res in availableResolutions)
        {
            const float aspectRatio = 16f / 9f;
            const float epsilon = 0.01f;

            if(Mathf.Abs((float)res.width/res.height - aspectRatio)>epsilon)
                continue;

            string option = res.width +  "X" +res.height;
            if (!resolutionMap.ContainsKey(option)) 
            {
                resolutionMap[option] = res;
                resolutionDropdown.options.Add(new TMP_Dropdown.OptionData(option));
                if (res.width == Screen.currentResolution.width && res.height == Screen.currentResolution.height) 
                {
                    curentResolutionIndex = resolutionDropdown.options.Count - 1;
                    defaultResolution = res;
                }
            }
        }

        resolutionDropdown.value = curentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    void SetDisplayMode(bool isFullscreen) 
    {
        Screen.fullScreenMode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        UpdateToggleLabel(isFullscreen);
    }

    void UpdateToggleLabel(bool isFullscreen) 
    {
        toggleLabel.text = isFullscreen ? "Fullscreen":"Windowed";
    }

    void SetResolution(int index) 
    {
        string[] dismesions = resolutionDropdown.options[index].text.Split('x');
        int width = int.Parse(dismesions[0].Trim());
        int height = int.Parse(dismesions[1].Trim());
        Screen.SetResolution(width, height, Screen.fullScreenMode);
    }

    public void CloseSetting() 
    {
        settingPanel.SetActive(false);
    }

    void ResetSetting() 
    {
        resolutionDropdown.value = resolutionDropdown.options.FindIndex(
            option => option.text == $"{defaultResolution.width}x{defaultResolution.height}"
            );
        fullscreenToggle.isOn = true;
    }
}
