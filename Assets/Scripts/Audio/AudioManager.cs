using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }

    [Header("Mixer")]
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [Header("Shooting SFX")]
    [SerializeField] private AudioClip shootingClip;
    [SerializeField][Range(0, 1)] private float shootingVolume = 1f;

    [Header("Damage SFX")]
    [SerializeField] private AudioClip damageClip;
    [SerializeField][Range(0, 1)] private float damageVolume = 1f;

    private AudioSource sfxSource;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        CreateAudioSource();
    }

    private void CreateAudioSource()
    {
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.outputAudioMixerGroup = sfxMixerGroup;
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f; // 2D SFX
    }

    public void PlayShootingSFX()
    {
        PlaySFX(shootingClip, shootingVolume);
    }

    public void PlayDamageSFX()
    {
        PlaySFX(damageClip, damageVolume);
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        sfxSource.PlayOneShot(clip, volume);
    }
}
