using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;

namespace Audio
{
    public class MusicManager : MonoBehaviour
    {

        public static MusicManager Instance { get; private set; }
        
        [Header("Настройки звука")]
        [SerializeField] private AudioMixerGroup musicMixerGroup;
        [SerializeField] private List<AudioClip> musicTracks;
        [SerializeField] private float minTimeBetweenTracks = 5f;
        [SerializeField] private float maxTimeBetweenTracks = 10f;
        
        // Название параметра громкости в аудио-миксере
        [SerializeField] private string volumeParam = "Music";
        
        private AudioSource _audioSource;
        private int _currentTrackIndex = -1;
        private float _nextTrackTime;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
                InitializeAudio();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeAudio()
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            
            if (musicMixerGroup == null)
            {
                Debug.LogError("MusicManager: не назначена группа микшера!");
                enabled = false;
                return;
            }
            
            _audioSource.outputAudioMixerGroup = musicMixerGroup;
            _audioSource.loop = false;
            
            if (musicTracks == null || musicTracks.Count == 0)
            {
                Debug.LogError("MusicManager: не назначены музыкальные треки!");
                enabled = false;
                return;
            }
            
            PlayNextTrack();
        }
        
        private void Update()
        {
            if (_audioSource != null && !_audioSource.isPlaying && Time.time >= _nextTrackTime)
            {
                PlayNextTrack();
            }
        }
        
        private void PlayNextTrack()
        {
            if (musicTracks == null || musicTracks.Count == 0 || _audioSource == null) return;
            
            _currentTrackIndex = (_currentTrackIndex + 1) % musicTracks.Count;
            _audioSource.clip = musicTracks[_currentTrackIndex];
            _audioSource.Play();
            
            _nextTrackTime = Time.time + _audioSource.clip.length + 
                            Random.Range(minTimeBetweenTracks, maxTimeBetweenTracks);
        }
        
        public void SetVolume(float normalizedVolume)
        {
            if (musicMixerGroup != null && musicMixerGroup.audioMixer != null)
            {
                float volumeDB = normalizedVolume > 0 ? 20f * Mathf.Log10(normalizedVolume) : -80f;
                
                try
                {
                    musicMixerGroup.audioMixer.SetFloat(volumeParam, volumeDB);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Ошибка установки громкости: {e.Message}. Параметр '{volumeParam}' не найден в миксере.");
                }
            }
        }
        
        public void ToggleMusic(bool isOn)
        {
            if (_audioSource == null) return;
            
            if (isOn)
            {
                _audioSource.UnPause();
            }
            else
            {
                _audioSource.Pause();
            }
        }
    }
} 