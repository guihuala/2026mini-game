using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : BasePanel
{
    [Header("通用组件 - 音频")]
    public Slider bgmVolumeSlider;
    public Slider sfxVolumeSlider;

    [Header("通用组件 - 数据")]
    public Button clearDataButton;

    [Header("按钮")]
    public Button backButton;

    private void Start()
    {
        InitAudioSettings();
        InitButtons();
    }
    
    private void InitAudioSettings()
    {
        bgmVolumeSlider.value = AudioManager.Instance.bgmVolumeFactor;
        sfxVolumeSlider.value = AudioManager.Instance.sfxVolumeFactor;
        
        bgmVolumeSlider.onValueChanged.AddListener(ChangeBgmVolume);
        sfxVolumeSlider.onValueChanged.AddListener(ChangeSfxVolume);
    }

    private void InitButtons()
    {
        if(backButton) backButton.onClick.AddListener(OnBackButtonClick);
        if(clearDataButton) clearDataButton.onClick.AddListener(OnClearDataClick);
    }

    #region 音量控制

    private void ChangeBgmVolume(float value)
    {
        AudioManager.Instance.ChangeBgmVolume(value);
    }

    private void ChangeSfxVolume(float value)
    {
        AudioManager.Instance.ChangeSfxVolume(value);
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("MainVolume", AudioManager.Instance.mainVolume);
        PlayerPrefs.SetFloat("BgmVolumeFactor", AudioManager.Instance.bgmVolumeFactor);
        PlayerPrefs.SetFloat("SfxVolumeFactor", AudioManager.Instance.sfxVolumeFactor);

        PlayerPrefs.Save();
        Debug.Log("Settings Saved!");
    }

    #endregion

    #region 按钮回调
    
    private void OnClearDataClick()
    {
        // 1. 清空所有 PlayerPrefs
        PlayerPrefs.DeleteAll(); 
        PlayerPrefs.Save();
        
        Debug.Log("所有存档数据已清空！");

        // 2. 同步音量和 UI 到默认状态
        ApplyDefaultAudioSettings();
    }

    private void OnBackButtonClick()
    {
        SaveSettings();
        UIManager.Instance.ClosePanel(panelName);
    }

    private void ApplyDefaultAudioSettings()
    {
        bgmVolumeSlider.value = 0.8f;
        sfxVolumeSlider.value = 0.8f;

        AudioManager.Instance.ChangeMainVolume(1f);
        ChangeBgmVolume(0.8f);
        ChangeSfxVolume(0.8f);
    }

    #endregion
}
