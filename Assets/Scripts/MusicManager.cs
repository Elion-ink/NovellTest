using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    public AudioSource audioSource;
    public AudioClip defaultMusic;
    public AudioClip sleep_sceneMusic;
    //public AudioClip badEndingMusic;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // музыка не пропадёт при смене сцен
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        PlayMusic(defaultMusic); // стандартная музыка при старте
    }

    public void PlayMusic(AudioClip clip)
    {
        if (audioSource.clip == clip) return; // не перезапускаем ту же музыку
        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();
    }
}

