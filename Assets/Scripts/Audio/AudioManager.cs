using UnityEngine;
using HauntedNight.Core;

namespace HauntedNight.Audio
{
    /// <summary>
    /// Music, stingers and pooled 3D one-shots. Uses a small pool so a jump scare
    /// never allocates during gameplay (matters on 2GB RAM phones).
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Clips")]
        public AudioClip ambientLoop;
        public AudioClip stingerClip;
        public AudioClip attackClip;
        public AudioClip whisperClip;

        [Header("Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource ambientSource;
        [SerializeField] private int poolSize = 8;

        private AudioSource[] _pool;
        private int _cursor;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _pool = new AudioSource[poolSize];
            for (int i = 0; i < poolSize; i++)
            {
                var go = new GameObject($"Sfx_{i}");
                go.transform.SetParent(transform);
                var src = go.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.spatialBlend = 1f;
                src.rolloffMode = AudioRolloffMode.Linear;
                src.maxDistance = 28f;
                _pool[i] = src;
            }
        }

        private void Start()
        {
            if (ambientSource != null && ambientLoop != null)
            {
                ambientSource.clip = ambientLoop;
                ambientSource.loop = true;
                ambientSource.Play();
            }
        }

        public void PlayAt(AudioClip clip, Vector3 position, float volume = 1f)
        {
            if (clip == null || _pool == null || _pool.Length == 0) return;
            var src = _pool[_cursor];
            _cursor = (_cursor + 1) % _pool.Length;
            src.transform.position = position;
            src.clip = clip;
            src.volume = volume;
            src.Play();
        }

        public void PlayMenuMusic(AudioClip clip)
        {
            if (musicSource == null || clip == null) return;
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }

        /// <summary>Raises tension as the match timer runs down.</summary>
        public void SetTension(float tension01)
        {
            if (musicSource == null) return;
            musicSource.pitch = Mathf.Lerp(0.95f, 1.12f, Mathf.Clamp01(tension01));
        }
    }
}
