using UI;
using UnityEngine;
using World;

namespace Core
{
    public class GameManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private SpiralWorldGenerator _worldGenerator;
        [SerializeField] private UIController _uiController;
    
        private bool _isPaused;

        private void Awake()
        {
            _uiController.Initialize(StartGame, ResumeGame);
            _worldGenerator.GenerateInitial();
        }

        private void StartGame()
        {
            _uiController.ShowGameUI();
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void ResumeGame()
        {
            _isPaused = false;
            _uiController.ShowGameUI();
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _isPaused = !_isPaused;
                Time.timeScale = _isPaused ? 0 : 1;
                _uiController.ShowPauseMenu();
                Cursor.lockState = _isPaused ? CursorLockMode.None : CursorLockMode.Locked;
            }
        }
    }
}