namespace UI
{
    using UnityEngine;
    using UnityEngine.UI;

    public class UIController : MonoBehaviour, IUIManager
    {
        [Header("UI Elements")]
        [SerializeField] private MainMenu _mainMenu;
        [SerializeField] private GameUI _gameUI;
        [SerializeField] private PauseMenu _pauseMenu;

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (_mainMenu != null) _mainMenu.Initialize(StartGame, ResumeGame);
            if (_gameUI != null) _gameUI.Initialize();
            if (_pauseMenu != null) _pauseMenu.Initialize(ResumeGame, ShowMainMenu, ShowSettings);
        }

        private void StartGame()
        {
            ShowGameUI();
        }

        private void ResumeGame()
        {
            ShowGameUI();
        }

        private void ShowSettings()
        {
            // TODO: Реализовать показ настроек
        }

        public void ShowMainMenu()
        {
            HideAll();
            if (_mainMenu != null) _mainMenu.Show();
        }

        public void ShowGameUI()
        {
            HideAll();
            if (_gameUI != null) _gameUI.Show();
        }

        public void ShowPauseMenu()
        {
            HideAll();
            if (_pauseMenu != null) _pauseMenu.Show();
        }

        private void HideAll()
        {
            if (_mainMenu != null) _mainMenu.Hide();
            if (_gameUI != null) _gameUI.Hide();
            if (_pauseMenu != null) _pauseMenu.Hide();
        }
    }
}