namespace UI
{
    using UnityEngine;
    using UnityEngine.UI;

    public class UIController : MonoBehaviour
    {
        [Header("Main UI Components")] [SerializeField]
        private Canvas _mainCanvas;

        [SerializeField] private CanvasGroup _mainMenu;
        [SerializeField] private CanvasGroup _gameUI;
        [SerializeField] private CanvasGroup _pauseMenu;

        [Header("Buttons")] [SerializeField] private Button _playButton;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _exitButton;

        public void Initialize(System.Action onPlay, System.Action onResume)
        {
            _playButton.onClick.AddListener(() => onPlay?.Invoke());
            _resumeButton.onClick.AddListener(() => onResume?.Invoke());
            _exitButton.onClick.AddListener(Application.Quit);

            ShowMainMenu();
        }

        public void ShowMainMenu()
        {
            SetUIState(_mainMenu, true);
            SetUIState(_gameUI, false);
            SetUIState(_pauseMenu, false);
        }

        public void ShowGameUI()
        {
            SetUIState(_mainMenu, false);
            SetUIState(_gameUI, true);
            SetUIState(_pauseMenu, false);
        }

        public void ShowPauseMenu()
        {
            SetUIState(_mainMenu, false);
            SetUIState(_gameUI, false);
            SetUIState(_pauseMenu, true);
        }

        private void SetUIState(CanvasGroup group, bool state)
        {
            group.alpha = state ? 1 : 0;
            group.interactable = state;
            group.blocksRaycasts = state;
        }
    }
}