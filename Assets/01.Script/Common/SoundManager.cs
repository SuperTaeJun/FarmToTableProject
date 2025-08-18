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
    Gacha,
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
    ForagePlant,
    ForageTree,

    StoreCash,
    Mount,
    Dismount,
    CarLoop,

    Gacha,
    GachaReward,
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
    [SerializeField] private AudioSource loopSfxSource;

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
            SetupLoopSfxSource();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void SetupLoopSfxSource()
    {
        if (loopSfxSource == null)
        {
            GameObject loopSfxObj = new GameObject("LoopSFXSource");
            loopSfxObj.transform.SetParent(transform);
            loopSfxSource = loopSfxObj.AddComponent<AudioSource>();
            loopSfxSource.loop = true;
            loopSfxSource.volume = 0f;
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

    public void PlayLoopSFX(SFXType sfxType)
    {
        if (sfxDictionary.TryGetValue(sfxType, out AudioClip clip))
        {
            if (loopSfxSource.clip != clip)
            {
                loopSfxSource.clip = clip;
            }
            
            if (!loopSfxSource.isPlaying)
            {
                loopSfxSource.Play();
            }
        }
    }

    public void SetLoopSFXVolume(float targetVolume, float fadeSpeed = 2f)
    {
        StopAllCoroutines();
        StartCoroutine(FadeLoopSFXVolume(targetVolume, fadeSpeed));
    }

    public void StopLoopSFX(float fadeSpeed = 2f)
    {
        StartCoroutine(FadeOutAndStop(fadeSpeed));
    }

    private System.Collections.IEnumerator FadeLoopSFXVolume(float targetVolume, float fadeSpeed)
    {
        float startVolume = loopSfxSource.volume;
        float time = 0f;

        while (time < 1f)
        {
            time += Time.deltaTime * fadeSpeed;
            loopSfxSource.volume = Mathf.Lerp(startVolume, targetVolume, time);
            yield return null;
        }
        
        loopSfxSource.volume = targetVolume;
    }

    private System.Collections.IEnumerator FadeOutAndStop(float fadeSpeed)
    {
        float startVolume = loopSfxSource.volume;
        float time = 0f;

        while (time < 1f && loopSfxSource.volume > 0f)
        {
            time += Time.deltaTime * fadeSpeed;
            loopSfxSource.volume = Mathf.Lerp(startVolume, 0f, time);
            yield return null;
        }
        
        loopSfxSource.volume = 0f;
        loopSfxSource.Stop();
    }
}