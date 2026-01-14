using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class SoundManager : MonoBehaviour
{
    public static SoundManager soundManager;

    [SerializeField] public AudioSource effectsSource;
    [SerializeField] public AudioSource musicSource;

    private AudioClip lastPlayedClip;

    private GameData gameData;

    public float fadeTime = 1.5f; // You can set this in the inspector

    private float originalVolume;
    private float savedMusicTime = 0f;

    [Header("Music")]
    public AudioClip[] musicClips;

    [Header("Sound Clips")]
    public AudioClip[] soundClips;

    public AudioClip CurrentMusicClip => musicSource.clip;

    private LogManager logClass;
    private const string className = "SoundManager:";

    //loading AUDIO
    private const string AUDIO_ROOT = "Sound/Audio/LT";
    private Dictionary<string, Dictionary<string, AudioClip>> audioCache =
    new Dictionary<string, Dictionary<string, AudioClip>>();

    private HashSet<string> loadedFolders = new HashSet<string>();

    //private static bool isAudioCacheLoaded = false;

    //public static SoundManager Instance;

    private void Awake()
    {
        if (soundManager == null)
        {
            soundManager = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        LoadSoundData();        
    }

    public void LoadSoundData()
    {
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();
        logClass = GameObject.FindWithTag("Log").GetComponent<LogManager>();

/*        try
        {
            //lastPlayedClip = musicClips[gameDataClass.saveData.currentPlayingClipIndex];
            musicSource.clip = lastPlayedClip;
        }
        catch (System.Exception ex)
        {
            logClass.WriteSysLog($"{className}Failed to set music clip: {ex.Message}\n{ex.StackTrace}");
        }*/

        if(gameData != null)
        {
            MuteSound(gameData.saveData.soundToggle);
            MuteMusic(gameData.saveData.musicToggle);
            SetSoundSpeed(gameData.saveData.soundSpeed);
            SetVolume("sound");
            SetVolume("music");
        }

       

/*        if (lastPlayedClip != null && musicSource.mute == false)
        {
            PlayMusic(lastPlayedClip);
        }*/

        //set sound speed
    }


    public void SetVolume(string type)
    {
        if (gameData != null)
        {
            if (type == "sound")
            {
                effectsSource.volume = gameData.saveData.soundVolume;
            }

            if (type == "music")
            {
                musicSource.volume = gameData.saveData.musicVolume;
                originalVolume = musicSource.volume;
            }
        }        
    }

    public void MuteSound(bool value)
    {
        if (value)
        {
            effectsSource.mute = false;
        }
        else
        {
            effectsSource.mute = true;
        }
    }

    public void MuteMusic(bool value)
    {
        if(value)
        {            
            musicSource.mute = false;
            //musicSource.time = savedMusicTime;
            PlayMusic(lastPlayedClip);
        }
        else
        {
            //savedMusicTime = musicSource.time;            
            musicSource.mute = true;
            lastPlayedClip = musicSource.clip;
        }            
    }


    public void PlaySound(AudioClip clip)
    {
        if (gameData.saveData.soundToggle == true)
        {
            effectsSource.PlayOneShot(clip);            
            StartCoroutine(ResetLastPlayedClip(clip.length));
        }        
    }

    private IEnumerator ResetLastPlayedClip(float duration)
    {
        yield return new WaitForSeconds(duration);
        lastPlayedClip = null;
    }

    public void PlayMusic(AudioClip clip)
    {
        //don't restart same music
        if (CurrentMusicClip == clip && musicSource.isPlaying)
            return;

        if (gameData.saveData.musicToggle == true)
        {
            if (musicSource != null && musicSource.isPlaying)
            {
                originalVolume = musicSource.volume;
                MusicFader("fadeIn", "play", clip); //fade out and play music
            }
            else
            {
                musicSource.clip = clip;
                musicSource.volume = originalVolume;
                MusicFader("fadeIn", "play", clip);
            }
        }
    }

    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            originalVolume = musicSource.volume;
            MusicFader("fadeOut", "stop");
        }
    }

    public void MusicFader(string fadeType, string action, AudioClip clip = null)
    {
        StartCoroutine(FadeMusicCoroutine(fadeType, action, clip));
    }

    private IEnumerator FadeMusicCoroutine(string fadeType, string action, AudioClip newClip = null)
    {
        float startVolume = musicSource.volume;
        float t = 0f;

        if (fadeType == "fadeOut")
        {
            while (t < fadeTime)
            {
                t += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeTime);
                yield return null;
            }

            // Don't use Stop() if you want to resume later
            if (action == "stop")
            {
                savedMusicTime = musicSource.time;
                musicSource.Pause();  // Keeps position
                yield break;
            }

            musicSource.Pause();  // Pause before switching clip
        }

        if (action == "play")
        {
            if (newClip != null && musicSource.clip != newClip)
            {
                musicSource.clip = newClip;
                savedMusicTime = 0f;
            }

            musicSource.volume = (fadeType == "fadeIn") ? 0f : originalVolume;
            //musicSource.time = savedMusicTime;
            musicSource.Play();

            if (fadeType == "fadeIn")
            {
                t = 0f;
                while (t < fadeTime)
                {
                    t += Time.deltaTime;
                    musicSource.volume = Mathf.Lerp(0f, originalVolume, t / fadeTime);
                    yield return null;
                }
            }
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (musicClips == null || musicClips.Length == 0)
            return;

        /*        if (scene.buildIndex < 0 || scene.buildIndex >= musicClips.Length)
                {
                    Debug.LogWarning(
                        $"No music clip assigned for scene index {scene.buildIndex}"
                    );
                    return;
                }*/

        lastPlayedClip = musicClips[0];

        if (gameData != null)
            PlayMusic(lastPlayedClip);
    }


    public void QuitGame()
    {
        AudioClip qClip = musicClips[4];

        musicSource.clip = qClip;

        MusicFader("fadeIn", "play", qClip);
    }

    public void ReturnToGame()
    {
        AudioClip gClip = musicClips[2];

        musicSource.clip = gClip;

        MusicFader("fadeIn", "play", gClip);
    }


    public void ButtonClick()
    {
        //set click sound
        AudioClip aClip = Resources.Load<AudioClip>("Sound/Effects/btn_click01_aclip");
        
        if (aClip == null)
            Debug.LogError("Failed to load audio clip!");

        if (aClip != null)
            effectsSource.PlayOneShot(aClip);
    }

    public void SoundToggle()
    {
        bool toggle = gameData.saveData.soundToggle;

        if (toggle)
        {
            gameData.saveData.soundToggle = false;
            MuteSound(false);
        }
        else
        {
            gameData.saveData.soundToggle = true;
            MuteSound(true);
        }
    }

    public void MusicToggle()
    {
        bool toggle = gameData.saveData.musicToggle;

        if (toggle)
        {
            gameData.saveData.musicToggle = false;
            MuteMusic(false);
        }
        else
        {
            gameData.saveData.musicToggle = true;
            MuteMusic(true);
        }
    }

    public void SetSoundSpeed(float speed)
    {             
        //set and save speed
        effectsSource.pitch = speed;
    }



    public AudioClip LoadAudioClipByName(string folder, string clipName)
    {
        if (string.IsNullOrEmpty(folder) || string.IsNullOrEmpty(clipName))
            return null;

        clipName = Path.GetFileNameWithoutExtension(clipName);

        if (!audioCache.ContainsKey(folder))
        {
            LoadAudioFolder(folder);
        }

        if (audioCache[folder].TryGetValue(clipName, out AudioClip clip))
        {
            return clip;
        }

        Debug.LogWarning($"AudioClip not found: {folder}/{clipName}");
        
        return null;
    }

    private void LoadAudioFolder(string folder)
    {
        if (audioCache.ContainsKey(folder))
            return;

        string path = $"{AUDIO_ROOT}/{folder}";
        AudioClip[] clips = Resources.LoadAll<AudioClip>(path);

        var dict = new Dictionary<string, AudioClip>();

        foreach (var clip in clips)
        {
            if (!dict.ContainsKey(clip.name))
                dict.Add(clip.name, clip);
        }

        audioCache.Add(folder, dict);

        Debug.Log($"Loaded {clips.Length} audio clips from {path}");
    }   

}
