using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Music and sound effects for the whole game, ported from the pygame music.py. It creates itself when the game
// starts and stays between scenes, so no scene needs it. Clips are loaded by name from Resources/Audio.
// Music: track1..track14 in a loop at the saved volume; M or the Options buttons turn it on/off (saved), N skips to the next track.
// Effects: one channel, so a new effect replaces the one playing, as in pygame. Every UI button clicks.
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private const int TrackCount = 14;

    private AudioSource music;
    private AudioSource effects;
    private int currentTrack;
    private bool musicOn;
    private InputAction toggleMusic;
    private InputAction nextTrack;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Create()
    {
        var manager = new GameObject("AudioManager");
        DontDestroyOnLoad(manager);
        Instance = manager.AddComponent<AudioManager>();
    }

    private void Awake()
    {
        music = gameObject.AddComponent<AudioSource>();
        music.volume = SaveService.Data.musicVolume;
        effects = gameObject.AddComponent<AudioSource>();

        // Project-wide actions (InputSystem_Actions).
        toggleMusic = InputSystem.actions.FindAction("Game/ToggleMusic");
        nextTrack = InputSystem.actions.FindAction("Game/NextTrack");
        toggleMusic.Enable();
        nextTrack.Enable();

        SceneManager.sceneLoaded += (scene, mode) => AddClickSounds();
        musicOn = SaveService.Data.musicOn;
        PlayTrack(0);
        if (!musicOn) music.Pause();
    }

    private void Update()
    {
        if (toggleMusic.WasPressedThisFrame())
            SetMusicOn(!musicOn);

        if (!musicOn) return;
        if (nextTrack.WasPressedThisFrame() || !music.isPlaying)
            PlayTrack((currentTrack + 1) % TrackCount); // N, or the track has ended
    }

    private void PlayTrack(int index)
    {
        currentTrack = index;
        AudioClip old = music.clip;
        music.clip = Resources.Load<AudioClip>($"Audio/Music/track{index + 1}");
        music.Play();
        if (old != null) Resources.UnloadAsset(old); // keep only the playing track in memory
    }

    // M and the Options buttons. Saved, so the music stays off the next time the game starts.
    public void SetMusicOn(bool on)
    {
        musicOn = on;
        if (on) music.UnPause();
        else music.Pause();
        SaveService.Data.musicOn = on;
        SaveService.Save();
    }

    // Called by the volume slider in the options. The volume is saved, so it stays the next time the game starts.
    public void SetVolume(float volume)
    {
        music.volume = volume;
        SaveService.Data.musicVolume = volume;
        SaveService.Save();
    }

    // `name` is the file name in Resources/Audio/Effects, e.g. "coin_collect".
    public void PlayEffect(string name)
    {
        effects.clip = Resources.Load<AudioClip>($"Audio/Effects/{name}");
        effects.Play();
    }

    private void AddClickSounds()
    {
        foreach (Button button in FindObjectsByType<Button>(FindObjectsInactive.Include))
            button.onClick.AddListener(() => PlayEffect("button_sound"));
    }
}
