using UnityEngine;
using System.Collections.Generic;

public enum BGMType
{
    Title,
    Am,
    Pm,
    Store,
    Loading,
    Clothing,
    
}

public enum SFXType
{
    ButtonHover,
    ButtonPressed,
    Cultivate,
    Watering,
    Harvest,
    Seed,
    Build,
    Step,
}

[System.Serializable]
public class BGMData
{
    public BGMType type;
    public AudioClip clip;
}

[System.Serializable]
public class SFXData
{
    public SFXType type;
    public AudioClip clip;
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")]
    [SerializeField] private BGMData[] bgmData;
    [SerializeField] private SFXData[] sfxData;

    private Dictionary<BGMType, AudioClip> bgmDictionary = new Dictionary<BGMType, AudioClip>();
    private Dictionary<SFXType, AudioClip> sfxDictionary = new Dictionary<SFXType, AudioClip>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioDictionaries();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeAudioDictionaries()
    {
        foreach (var data in bgmData)
            bgmDictionary[data.type] = data.clip;

        foreach (var data in sfxData)
            sfxDictionary[data.type] = data.clip;
    }

    public void PlayBGM(BGMType bgmType, bool loop = true)
    {
        if (bgmDictionary.TryGetValue(bgmType, out AudioClip clip))
        {
            bgmSource.clip = clip;
            bgmSource.loop = loop;
            bgmSource.Play();
        }
    }

    public void PlaySFX(SFXType sfxType)
    {
        if (sfxDictionary.TryGetValue(sfxType, out AudioClip clip))
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void SetBGMVolume(float volume) => bgmSource.volume = Mathf.Clamp01(volume);
    public void SetSFXVolume(float volume) => sfxSource.volume = Mathf.Clamp01(volume);

    public void StopBGM() => bgmSource.Stop();
    public void PauseBGM() => bgmSource.Pause();
    public void ResumeBGM() => bgmSource.UnPause();
}