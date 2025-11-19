using UnityEngine;

public class SfxManager : MonoBehaviour
{
    public static SfxManager Instance { get; private set; }

    [Header("Sources")]
    public AudioSource musicSource;   // arraste o AudioSource do _Music aqui
    public AudioSource sfxSource;     // arraste o AudioSource do _Audio (SFX_Source)

    [Header("Clipes")]
    public AudioClip sfxShoot;
    public AudioClip sfxHurt;
    public AudioClip sfxPickup;
    public AudioClip sfxGameOver;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // Opcional: DontDestroyOnLoad(gameObject);
        if (!sfxSource) Debug.LogWarning("[SfxManager] Missing sfxSource");
        if (!musicSource) Debug.LogWarning("[SfxManager] Missing musicSource");
    }

    // ---- SFX comuns ----
    public void PlayShoot()  { if (sfxSource && sfxShoot)  sfxSource.PlayOneShot(sfxShoot); }
    public void PlayHurt()   { if (sfxSource && sfxHurt)   sfxSource.PlayOneShot(sfxHurt); }
    public void PlayPickup() { if (sfxSource && sfxPickup) sfxSource.PlayOneShot(sfxPickup); }

    // ---- Game Over / Música ----
    public void PlayGameOver() { if (sfxSource && sfxGameOver) sfxSource.PlayOneShot(sfxGameOver); }

    public void StopMusic(float fadeSeconds = 0.0f)
    {
        if (!musicSource) return;
        if (fadeSeconds <= 0f) { musicSource.Stop(); return; }
        StopAllCoroutines();
        StartCoroutine(FadeOutAndStop(musicSource, fadeSeconds));
    }

    System.Collections.IEnumerator FadeOutAndStop(AudioSource src, float t)
    {
        float start = src.volume;
        float elapsed = 0f;
        while (elapsed < t)
        {
            elapsed += Time.unscaledDeltaTime;
            src.volume = Mathf.Lerp(start, 0f, Mathf.Clamp01(elapsed / t));
            yield return null;
        }
        src.Stop();
        src.volume = start; // restaura volume para próximos plays
    }
}
