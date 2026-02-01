using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Library")]
        [SerializeField] private List<AudioClip> musicClips;
        [SerializeField] private List<SfxClip>  sfxClips;

        [Header("Mixer")]
        [SerializeField] private AudioMixer mixer;

        private readonly Dictionary<string, AudioClip> _sfx = new();

        private void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this; DontDestroyOnLoad(gameObject);

            PlayMusic(0);
            foreach (var clip in sfxClips) _sfx[clip.id] = clip.clip;
            ApplySavedVolumes();
        }
        
        public void PlayMusic(int index, bool loop = true)
        {
            if (index < 0 || index >= musicClips.Count) return;
            musicSource.clip  = musicClips[index];
            musicSource.loop  = loop;
            musicSource.Play();
        }

        public void PlaySfx(string id, float pitchRandom = 0.05f)
        {
            if (!_sfx.TryGetValue(id, out var clip)) return;
            sfxSource.pitch = 1f + Random.Range(-pitchRandom, pitchRandom);
            sfxSource.PlayOneShot(clip);
        }

        public void SetMusicVolume(float linear)
        {
            mixer.SetFloat("MusicVol", LinearToDb(linear));
            PlayerPrefs.SetFloat("MusicVol", linear);
        }
        public void SetSfxVolume(float linear)
        {
            mixer.SetFloat("SFXVol", LinearToDb(linear));
            PlayerPrefs.SetFloat("SFXVol", linear);
        }

        private static float LinearToDb(float v) => v <= 0.0001f ? -80f : Mathf.Log10(v) * 20f;

        private void ApplySavedVolumes()
        {
            SetMusicVolume(PlayerPrefs.GetFloat("MusicVol", .5f));
            SetSfxVolume  (PlayerPrefs.GetFloat("SFXVol", .5f));
        }

        [System.Serializable] public struct SfxClip { public string id; public AudioClip clip; }
    }
}
