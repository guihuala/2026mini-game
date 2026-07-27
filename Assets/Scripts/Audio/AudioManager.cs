using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[Serializable]
public class AudioInfo
{
    public string audioName;
    public AudioSource audioSource;
}

public class AudioManager : SingletonPersistent<AudioManager>
{
    [Header("Vision Mode BGM")]
    [SerializeField] private List<string> blueVisionBgmNames = new List<string>();
    [SerializeField] private List<string> redVisionBgmNames = new List<string>();
    [SerializeField, Min(0f)] private float visionBgmFadeDuration = 0.5f;

    // 存储所有BGM的音频信息
    public List<AudioInfo> bgmAudioInfoList;

    // 存储所有SFX的音频信息
    public List<AudioInfo> sfxAudioInfoList;

    // 音量控制全局音量
    public float mainVolume;

    // BGM音量因子，实际音量 = mainVolume * bgmVolumeFactor
    public float bgmVolumeFactor;

    // SFX音量因子，实际音量 = mainVolume * sfxVolumeFactor
    public float sfxVolumeFactor;

    // 音频资源的根节点
    public AudioDatas audioDatas;

    private GameObject _bgmSourcesRootGO;
    private GameObject _sfxSourcesRootGO;
    private string _currentVisionBgmName;

    // 引用AudioMixer
    public AudioMixer audioMixer;

    // 暴露参数名称
    private const string BGM_VOLUME_PARAM = "BGM";
    private const string SFX_VOLUME_PARAM = "Sfx";
    
    protected override void Awake()
    {
        // The manager persists between scenes, while each scene can override its
        // Vision BGM lists on the manager prefab instance. Copy those overrides
        // to the surviving singleton before the duplicate is destroyed.
        if (Instance != null && Instance != this)
        {
            Instance.ApplyVisionBgmConfiguration(this);
            base.Awake();
            return;
        }

        base.Awake();
        if (Instance != this) return;

        bgmAudioInfoList ??= new List<AudioInfo>();
        sfxAudioInfoList ??= new List<AudioInfo>();
        if (audioDatas == null)
            audioDatas = Resources.Load<AudioDatas>("Data/AudioDataListSO");

        // 创建BGM和SFX的AudioSource根节点
        _bgmSourcesRootGO = new GameObject("BGM_ROOT");
        _sfxSourcesRootGO = new GameObject("SFX_ROOT");

        _bgmSourcesRootGO.transform.SetParent(transform);
        _sfxSourcesRootGO.transform.SetParent(transform);

        // 加载存储的音量设置
        mainVolume = PlayerPrefs.GetFloat("MainVolume", 1f);
        bgmVolumeFactor = PlayerPrefs.GetFloat("BgmVolumeFactor", .8f);
        sfxVolumeFactor = PlayerPrefs.GetFloat("SfxVolumeFactor", .8f);
    }

    private void ApplyVisionBgmConfiguration(AudioManager source)
    {
        if (!string.IsNullOrWhiteSpace(_currentVisionBgmName))
        {
            AudioInfo currentBgm = bgmAudioInfoList.Find(
                info => info.audioName == _currentVisionBgmName && info.audioSource != null);
            if (currentBgm != null)
                StopBgm(_currentVisionBgmName, source.visionBgmFadeDuration);
        }

        blueVisionBgmNames = source.blueVisionBgmNames != null
            ? new List<string>(source.blueVisionBgmNames)
            : new List<string>();
        redVisionBgmNames = source.redVisionBgmNames != null
            ? new List<string>(source.redVisionBgmNames)
            : new List<string>();
        visionBgmFadeDuration = source.visionBgmFadeDuration;
        _currentVisionBgmName = null;
    }

    private void Start()
    {
        // 初始化AudioMixer的音量
        ChangeBgmVolume(bgmVolumeFactor);
        ChangeSfxVolume(sfxVolumeFactor);
    }

    /// <summary>
    /// 播放当前视觉模式配置的BGM，并淡出上一个视觉模式的BGM。
    /// 音乐名称对应 AudioDataListSO 中的 audioName。
    /// </summary>
    public void PlayVisionModeBgm(VisionMode mode)
    {
        List<string> candidates = mode == VisionMode.Blue ? blueVisionBgmNames : redVisionBgmNames;
        string nextBgmName = ChooseVisionBgm(candidates);
        if (string.IsNullOrWhiteSpace(nextBgmName) || nextBgmName == _currentVisionBgmName)
            return;

        if (audioDatas == null || audioDatas.audioDataList == null ||
            audioDatas.audioDataList.Find(x => x.audioName == nextBgmName) == null)
        {
            Debug.LogWarning($"未找到 {mode} Vision BGM：{nextBgmName}");
            return;
        }

        string previousBgmName = _currentVisionBgmName;
        _currentVisionBgmName = nextBgmName;
        PlayBgm(nextBgmName, previousBgmName, visionBgmFadeDuration, visionBgmFadeDuration);
    }

    private string ChooseVisionBgm(List<string> candidates)
    {
        if (candidates == null || candidates.Count == 0)
            return null;

        List<string> availableNames = candidates.FindAll(name =>
            !string.IsNullOrWhiteSpace(name) &&
            (candidates.Count == 1 || name != _currentVisionBgmName));

        if (availableNames.Count == 0)
            return null;

        return availableNames[UnityEngine.Random.Range(0, availableNames.Count)];
    }


    /// <summary>
    /// 播放BGM
    /// </summary>
    public void PlayBgm(string fadeInMusicName, string fadeOutMusicName = "", float fadeInDuration = 0.5f, float fadeOutDuration = 0.5f, bool loop = true)
    {
        Sequence s = DOTween.Sequence();

        // 如果需要淡出某个BGM
        if (!string.IsNullOrEmpty(fadeOutMusicName))
        {
            AudioInfo fadeOutInfo = bgmAudioInfoList.Find(x => x.audioName == fadeOutMusicName);

            if (fadeOutInfo == null)
            {
                Debug.LogWarning("未找到BGM：" + fadeOutMusicName);
                return;
            }

            s.Append(fadeOutInfo.audioSource.DOFade(0, fadeOutDuration).OnComplete(() =>
            {
                fadeOutInfo.audioSource.Pause();
            }));
        }

        // 检查是否已存在需要播放的BGM
        AudioInfo audioInfo = bgmAudioInfoList.Find(x => x.audioName == fadeInMusicName);

        if (audioInfo != null)
        {
            s.Append(audioInfo.audioSource.DOFade(mainVolume * bgmVolumeFactor, fadeInDuration).OnComplete(() =>
            {
                audioInfo.audioSource.Play();
            }));
            return;
        }

        // 从资源加载并播放新的BGM
        AudioData fadeInData = audioDatas.audioDataList.Find(x => x.audioName == fadeInMusicName);

        if (fadeInData == null)
        {
            Debug.LogWarning("未找到BGM：" + fadeInMusicName);
            return;
        }

        GameObject fadeInAudioGO = new GameObject(fadeInMusicName);
        fadeInAudioGO.transform.SetParent(_bgmSourcesRootGO.transform);

        AudioSource fadeInAudioSource = fadeInAudioGO.AddComponent<AudioSource>();
        fadeInAudioSource.clip = Resources.Load<AudioClip>(fadeInData.audioPath);
        fadeInAudioSource.loop = loop;
        fadeInAudioSource.volume = fadeInDuration > 0 ? 0 : mainVolume * bgmVolumeFactor;
        
        fadeInAudioSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Master")[1]; // 设置为音频混合器的 "Master" 组，确保应用音量控制
        
        fadeInAudioSource.Play();

        if (fadeInDuration > 0)
        {
            s.Append(fadeInAudioSource.DOFade(mainVolume * bgmVolumeFactor, fadeInDuration));
        }

        AudioInfo info = new AudioInfo
        {
            audioName = fadeInMusicName,
            audioSource = fadeInAudioSource
        };

        bgmAudioInfoList.Add(info);
        StartCoroutine(DetectingAudioPlayState(info, true));
    }
    
    /// <summary>
    /// 暂停BGM
    /// </summary>
    /// <param name="pauseBgmName">要暂停的片段名称</param>
    /// <param name="fadeOutDuration">淡出间隔</param>
    public void PauseBgm(string pauseBgmName, float fadeOutDuration = 0.5f)
    {
        AudioInfo audioInfo = bgmAudioInfoList.Find(x => x.audioName == pauseBgmName);

        if (audioInfo == null)
        {
            Debug.LogWarning("未找到BGM：" + pauseBgmName);
            return;
        }

        Sequence s = DOTween.Sequence();

        s.Append(audioInfo.audioSource.DOFade(0, fadeOutDuration).OnComplete(() =>
        {
            audioInfo.audioSource.Pause();
        }));
    }


    /// <summary>
    /// 停止BGM
    /// </summary>
    /// <param name="stopBgmName">要停止的片段名称</param>
    /// <param name="fadeOutDuration">淡出间隔</param>
    public void StopBgm(string stopBgmName, float fadeOutDuration = 0.5f)
    {
        AudioInfo audioInfo = bgmAudioInfoList.Find(x => x.audioName == stopBgmName);

        if (audioInfo == null)
        {
            Debug.LogWarning("未找到BGM：" +  stopBgmName);
            return;
        }

        Sequence s = DOTween.Sequence();

        s.Append(audioInfo.audioSource.DOFade(0, fadeOutDuration).OnComplete(() =>
        {
            audioInfo.audioSource.Stop();

            Destroy(audioInfo.audioSource.gameObject);
        }));

        bgmAudioInfoList.Remove(audioInfo);

    }
    
    /// <summary>
    /// 停止播放所有BGM
    /// </summary>
    /// <param name="fadeOutDuration">淡出间隔</param>
    public void StopAllBGM(float fadeOutDuration = 0.5f)
    {
        foreach (var bgmInfo in bgmAudioInfoList.ToArray())
        {
            StopBgm(bgmInfo.audioName, fadeOutDuration);
        }
        StopAllCoroutines();
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="sfxName">要播放的音效片段名称</param>
    /// <param name="loop">是否循环</param>
    public void PlaySfx(string sfxName, bool loop = false)
    {
        if (audioDatas == null || audioDatas.audioDataList == null)
        {
            Debug.LogWarning("AudioDatas 未配置，无法播放音效：" + sfxName);
            return;
        }

        if (loop && sfxAudioInfoList.Exists(x => x.audioName == sfxName && x.audioSource != null))
            return;

        // 从音频列表中寻找
        AudioData sfxData = audioDatas.audioDataList.Find(x => x.audioName == sfxName);

        if (sfxData == null)
        {
            Debug.LogWarning("未找到sfx：" + sfxName);
            return;
        }

        // 创建音频播放器
        GameObject sfxAudioGO = new GameObject(sfxName);
        sfxAudioGO.transform.SetParent(_sfxSourcesRootGO.transform);

        AudioSource sfxAudioSource = sfxAudioGO.AddComponent<AudioSource>();
        sfxAudioSource.clip = Resources.Load<AudioClip>(sfxData.audioPath);
        if (sfxAudioSource.clip == null)
        {
            Debug.LogWarning($"音效资源加载失败：{sfxData.audioPath}");
            Destroy(sfxAudioGO);
            return;
        }
        sfxAudioSource.loop = loop;
        sfxAudioSource.volume = mainVolume * sfxVolumeFactor;
        
        if (audioMixer != null)
        {
            AudioMixerGroup[] groups = audioMixer.FindMatchingGroups("Sfx");
            if (groups.Length == 0)
                groups = audioMixer.FindMatchingGroups("Master");
            if (groups.Length > 0)
                sfxAudioSource.outputAudioMixerGroup = groups[groups.Length - 1];
        }

        sfxAudioSource.Play();

        AudioInfo info = new AudioInfo();
        info.audioName = sfxName;
        info.audioSource = sfxAudioSource;
        sfxAudioInfoList.Add(info);

        StartCoroutine(DetectingAudioPlayState(info, false));
    }

    /// <summary>
    /// 暂停音效
    /// </summary>
    /// <param name="pauseSfxName">要暂停的音效名称</param>
    public void PauseSfx(string pauseSfxName)
    {
        AudioInfo audioInfo = sfxAudioInfoList.Find(x => x.audioName == pauseSfxName);

        if (audioInfo == null)
        {
            Debug.LogWarning("未找到sfx：" + pauseSfxName);
            return;
        }

        audioInfo.audioSource.Pause();
    }
    
    /// <summary>
    /// 停止音效
    /// </summary>
    /// <param name="stopSfxName">要停止的音效名称</param>
    public void StopSfx(string stopSfxName)
    {
        foreach (AudioInfo audioInfo in sfxAudioInfoList.FindAll(x => x.audioName == stopSfxName))
        {
            if (audioInfo.audioSource != null)
            {
                audioInfo.audioSource.Stop();
                Destroy(audioInfo.audioSource.gameObject);
            }
            sfxAudioInfoList.Remove(audioInfo);
        }
    }

    /// <summary>
    /// 修改全局音量，并保存到PlayerPrefs
    /// </summary>
    /// <param name="volume">新的全局音量</param>
    public void ChangeMainVolume(float volume)
    {
        mainVolume = volume;
        PlayerPrefs.SetFloat("MainVolume", mainVolume);

        foreach (var info in bgmAudioInfoList)
        {
            info.audioSource.volume = mainVolume * bgmVolumeFactor;
        }
        foreach (var info in sfxAudioInfoList)
        {
            info.audioSource.volume = mainVolume * sfxVolumeFactor;
        }
        Debug.Log($"MainVolume changed to {mainVolume}");
    }

    /// <summary>
    /// 修改BGM音量并使用AudioMixer控制音量
    /// </summary>
    public void ChangeBgmVolume(float factor)
    {
        bgmVolumeFactor = factor;
        
        bgmVolumeFactor = Mathf.Clamp(bgmVolumeFactor, 0f, 1f);

        PlayerPrefs.SetFloat("BgmVolumeFactor", bgmVolumeFactor);
        
        if (audioMixer == null) return;
        if (bgmVolumeFactor == 0 || mainVolume == 0)
        {
            audioMixer.SetFloat(BGM_VOLUME_PARAM, -80f);
        }
        else
        {
            audioMixer.SetFloat(BGM_VOLUME_PARAM, Mathf.Log10(mainVolume * bgmVolumeFactor) * 20);
        }
    }

    /// <summary>
    /// 修改音效音量并使用AudioMixer控制音量
    /// </summary>
    public void ChangeSfxVolume(float factor)
    {
        sfxVolumeFactor = factor;
        
        sfxVolumeFactor = Mathf.Clamp(sfxVolumeFactor, 0f, 1f);

        PlayerPrefs.SetFloat("SfxVolumeFactor", sfxVolumeFactor);

        // 如果因子为0，则设置为非常小的音量接近静音
        if (audioMixer == null) return;
        if (sfxVolumeFactor == 0 || mainVolume == 0)
        {
            audioMixer.SetFloat(SFX_VOLUME_PARAM, -80f);
        }
        else
        {
            audioMixer.SetFloat(SFX_VOLUME_PARAM, Mathf.Log10(mainVolume * sfxVolumeFactor) * 20);
        }
    }


    /// <summary>
    /// 检测音频播放状态并清理结束播放的音频资源
    /// </summary>
    IEnumerator DetectingAudioPlayState(AudioInfo info, bool isBgm)
    {
        AudioSource audioSource = info.audioSource;
        while (audioSource != null && audioSource.isPlaying)
        {
            yield return null;
        }
        if (isBgm)
        {
            bgmAudioInfoList.Remove(info);
        }
        else
        {
            sfxAudioInfoList.Remove(info);
        }

        if (info.audioSource != null)
            Destroy(info.audioSource.gameObject);
    }
}
