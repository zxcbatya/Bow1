using UnityEngine;
using UnityEngine.UI;
using Audio;

namespace UI
{
    public class VolumeSlider : MonoBehaviour
    {
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private MusicManager _musicManager;
        
        private void Start()
        {
            if (volumeSlider == null)
            {
                volumeSlider = GetComponent<Slider>();
            }
            
            if (_musicManager == null)
            {
                _musicManager = MusicManager.Instance; // Используй синглтон
            }
            if (volumeSlider != null && _musicManager != null)
            {
                volumeSlider.onValueChanged.AddListener(_musicManager.SetVolume);
                volumeSlider.value = 1f;
            }
        }
    }
} 