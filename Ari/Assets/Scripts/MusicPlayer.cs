using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance { get; private set; }

    [Header("Tema")]
    public AudioClip theme;
    [Range(0f, 1f)] public float musicVolume = 0.6f;
    public bool playOnStart = true;

    private AudioSource src;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        src = GetComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = true;          // reinicia automaticamente quando acabar
        src.spatialBlend = 0f;    // 2D (global)
        src.volume = musicVolume;

        if (theme) src.clip = theme;
        if (playOnStart && theme) src.Play();
    }

    public void Play()  { if (src && theme) { src.clip = theme; src.loop = true; src.Play(); } }
    public void Stop()  { if (src) src.Stop(); }
    public void Pause() { if (src) src.Pause(); }
    public void SetVolume(float v) { if (src) src.volume = Mathf.Clamp01(v); }
}
