using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Utilities.UTask;

[DefaultExecutionOrder(100)]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Serializable]
    public class SoundEntry
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    [Header("Base Multipliers")]
    [SerializeField, Range(0f, 1f)] private float defaultSfxVolumeMultiplier = 1f;
    [SerializeField, Range(0f, 1f)] private float defaultMusicVolumeMultiplier = 1f;

    [Header("Sound Library")]
    [SerializeField] private SoundEntry[] soundEntries;

    [Header("BG Music library")]
    [SerializeField] private SoundEntry[] bgMusicEntries;

    [Header("Music Settings")]
    public AudioSource musicSource;

    [Header("SFX Pool Settings")]
    [SerializeField] private int initialSfxPoolSize = 10;
    private List<AudioSource> sfxPool;

    private Dictionary<string, SoundEntry> soundDict;
    private Dictionary<string, SoundEntry> bgMusicDict;
    private HashSet<string> playingOnce = new HashSet<string>();

    private int audioSourceCounter = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        soundDict = new Dictionary<string, SoundEntry>(soundEntries.Length);
        foreach (var entry in soundEntries)
        {
            if (entry == null || string.IsNullOrEmpty(entry.name) || entry.clip == null)
                continue;
            soundDict[entry.name] = entry;
        }

        bgMusicDict = new Dictionary<string, SoundEntry>(bgMusicEntries.Length);
        foreach (var entry in bgMusicEntries)
        {
            if (entry == null || string.IsNullOrEmpty(entry.name) || entry.clip == null)
                continue;
            bgMusicDict[entry.name] = entry;
        }

        sfxPool = new List<AudioSource>(initialSfxPoolSize);
        ExpandSfxPool(initialSfxPoolSize);
    }

    private void Start()
    {
        PlayMusic("Cute Village Loop", true);
        ApplySavedVolumes();
    }

    public AudioClip PlaySound(string soundName)
    {
        if (!soundDict.TryGetValue(soundName, out var entry))
        {
            Debug.LogWarning($"Sound '{soundName}' not found in SoundManager library.");
            return null;
        }

        var src = GetAvailableSfxSource();
        src.spatialBlend = 0f;

        float master = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float sfxVol = PlayerPrefs.GetFloat("SfxVolume", 1f);
        float volumeScale = master * sfxVol * defaultSfxVolumeMultiplier * entry.volume;

        src.PlayOneShot(entry.clip, volumeScale);
        return entry.clip;
    }

    public AudioClip PlaySoundOnceUntilComplete(string soundName)
    {
        if (playingOnce.Contains(soundName))
            return null;

        if (!soundDict.TryGetValue(soundName, out var entry))
        {
            Debug.LogWarning($"Sound '{soundName}' not found in SoundManager library.");
            return null;
        }

        var src = GetAvailableSfxSource();
        src.spatialBlend = 0f;

        float master = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float sfxVol = PlayerPrefs.GetFloat("SfxVolume", 1f);
        float volumeScale = master * sfxVol * defaultSfxVolumeMultiplier * entry.volume;

        src.PlayOneShot(entry.clip, volumeScale);

        playingOnce.Add(soundName);
        _ = RemoveFromOnceAfterDelay(soundName, entry.clip.length);

        return entry.clip;
    }

    public AudioClip PlaySoundAtPosition(string soundName, Vector3 position)
    {
        if (!soundDict.TryGetValue(soundName, out var entry))
        {
            Debug.LogWarning($"Sound '{soundName}' not found in SoundManager library.");
            return null;
        }

        var src = GetAvailableSfxSource();
        src.spatialBlend = 1f;
        src.transform.position = position;

        float master = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float sfxVol = PlayerPrefs.GetFloat("SfxVolume", 1f);
        float volumeScale = master * sfxVol * defaultSfxVolumeMultiplier * entry.volume;

        src.PlayOneShot(entry.clip, volumeScale);
        return entry.clip;
    }

    public AudioClip PlaySoundOnceAtPositionUntilComplete(string soundName, Vector3 position)
    {
        if (playingOnce.Contains(soundName))
            return null;

        if (!soundDict.TryGetValue(soundName, out var entry))
        {
            Debug.LogWarning($"Sound '{soundName}' not found in SoundManager library.");
            return null;
        }

        var src = GetAvailableSfxSource();
        src.spatialBlend = 1f;
        src.transform.position = position;

        float master = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float sfxVol = PlayerPrefs.GetFloat("SfxVolume", 1f);
        float volumeScale = master * sfxVol * defaultSfxVolumeMultiplier * entry.volume;

        src.PlayOneShot(entry.clip, volumeScale);

        playingOnce.Add(soundName);
        _ = RemoveFromOnceAfterDelay(soundName, entry.clip.length);

        return entry.clip;
    }

    private async Task RemoveFromOnceAfterDelay(string soundName, float delay)
    {
        await UTaskEx.Delay(delay).AsTask();
        playingOnce.Remove(soundName);
    }

    public void PlayMusic(string soundName, bool loop = true)
    {
        if (musicSource == null)
            return;

        if (!bgMusicDict.TryGetValue(soundName, out var entry))
        {
            Debug.LogWarning($"Music '{soundName}' not found in SoundManager library.");
            return;
        }

        float master = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float finalVolume = master * musicVol * defaultMusicVolumeMultiplier * entry.volume;

        musicSource.clip = entry.clip;
        musicSource.loop = loop;
        musicSource.volume = finalVolume;
        musicSource.Play();
    }

    public void ApplySavedVolumes()
    {
        float master = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float music = PlayerPrefs.GetFloat("MusicVolume", 1f);

        if (musicSource != null && musicSource.clip != null)
        {
            foreach (var entry in bgMusicEntries)
            {
                if (entry.clip == musicSource.clip)
                {
                    musicSource.volume = master * music * defaultMusicVolumeMultiplier * entry.volume;
                    break;
                }
            }
        }
    }

    public void StopMusic()
    {
        musicSource?.Stop();
    }

    private List<AudioSource> ExpandSfxPool(int count)
    {
        var newList = new List<AudioSource>(count);
        for (int i = 0; i < count; i++)
        {
            GameObject go = new GameObject($"SFX_AudioSource_{audioSourceCounter++}");
            go.transform.parent = this.transform;

            AudioSource src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = false;
            src.minDistance = 1f;
            src.maxDistance = 20f;
            src.rolloffMode = AudioRolloffMode.Linear;

            sfxPool.Add(src);
            newList.Add(src);
        }
        return newList;
    }

    private AudioSource GetAvailableSfxSource()
    {
        return sfxPool.Find(s => !s.isPlaying) ?? ExpandSfxPool(1)[0];
    }

    public List<AudioSource> GetAllSfxSources()
    {
        return sfxPool;
    }
}
