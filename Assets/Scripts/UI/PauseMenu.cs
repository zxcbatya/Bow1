using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PauseMenu : BaseUIElement
    {
        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button settingsButton;

        private System.Action _onResume;
        private System.Action _onMainMenu;
        private System.Action _onSettings;

        public void Initialize(System.Action onResume, System.Action onMainMenu, System.Action onSettings)
        {
            base.Initialize();
            
            _onResume = onResume;
            _onMainMenu = onMainMenu;
            _onSettings = onSettings;

            if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeClick);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuClick);
            if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsClick);
        }

        private void OnResumeClick()
        {
            _onResume?.Invoke();
        }

        private void OnMainMenuClick()
        {
            _onMainMenu?.Invoke();
        }

        private void OnSettingsClick()
        {
            _onSettings?.Invoke();
        }

        private void OnDestroy()
        {
            if (resumeButton != null) resumeButton.onClick.RemoveListener(OnResumeClick);
            if (mainMenuButton != null) mainMenuButton.onClick.RemoveListener(OnMainMenuClick);
            if (settingsButton != null) settingsButton.onClick.RemoveListener(OnSettingsClick);
        }
    }
} 