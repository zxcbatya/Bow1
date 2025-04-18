using UI;
using UnityEngine;
using World;

namespace Core
{
    public class GameManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private StructuredSpiralGenerator _worldGenerator;
        [SerializeField] private UIController _uiController;
        
        private bool _gameStarted = false;
        private bool _isPaused;

        private void Awake()
        {
            if (_uiController != null)
            {
                _uiController.Initialize();
            }
            else
            {
                Debug.LogError("UIController не назначен в инспекторе!", this);
            }
        }
        
        private void Start()
        {
            // При запуске игры сразу показываем главное меню
            if (_uiController != null)
            {
                _uiController.ShowMainMenu();
                Time.timeScale = 1f; // Останавливаем время до начала игры
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public void StartGame()
        {
            _gameStarted = true;
            _isPaused = false;
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _uiController.ShowGameUI();
        }

        private void ResumeGame()
        {
            _isPaused = false;
            _uiController.ShowGameUI();
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (_uiController == null) return;
            
            if (_gameStarted && Input.GetKeyDown(KeyCode.Escape))
            {
                _isPaused = !_isPaused;
                Time.timeScale = _isPaused ? 0 : 1;
                if (_isPaused)
                {
                    _uiController.ShowPauseMenu();
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else
                {
                    _uiController.ShowGameUI();
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }
    }
}