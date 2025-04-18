using UnityEngine;
using UnityEngine.UI;
using Player;

namespace UI
{
    public class MainMenu : BaseUIElement
    {
        [Header("Ссылки")]
        [SerializeField] private CanvasGroup menuCanvas;
        [SerializeField] private Button playButton;
        [SerializeField] private Button resumeButton; // Кнопка возобновления игры
        [SerializeField] private Button exitButton;
        [SerializeField] private GameObject player; // Ссылка на игрока для блокировки управления
        
        private PlayerController _playerController;
        private bool _isPaused = true;
        
        private System.Action _onPlay;
        private System.Action _onResume;

        public void Initialize(System.Action onPlay, System.Action onResume)
        {
            base.Initialize();
            _onPlay = onPlay;
            _onResume = onResume;

            if (player != null)
            {
                _playerController = player.GetComponent<PlayerController>();
            }
            
            // Показываем меню при старте
            ShowMenu();
            
            if (playButton != null)
            {
                playButton.onClick.AddListener(OnPlayClick);
            }
            
            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(OnResumeClick);
                // Скрываем кнопку Resume при старте
                resumeButton.gameObject.SetActive(false);
            }
            
            if (exitButton != null)
            {
                exitButton.onClick.AddListener(Application.Quit);
            }
            
            // Блокируем только управление игрока
            if (_playerController != null)
            {
                _playerController.enabled = false;
            }
        }
        
        private void OnPlayClick()
        {
            _onPlay?.Invoke();
        }
        
        private void OnResumeClick()
        {
            _onResume?.Invoke();
        }
        
        private void ShowMenu()
        {
            _isPaused = true;
            
            if (menuCanvas != null)
            {
                menuCanvas.alpha = 1;
                menuCanvas.interactable = true;
                menuCanvas.blocksRaycasts = true;
            }
            
            // Показываем курсор
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // Блокируем только управление
            if (_playerController != null)
            {
                _playerController.enabled = false;
            }
        }
        
        private void HideMenu()
        {
            if (menuCanvas != null)
            {
                menuCanvas.alpha = 0;
                menuCanvas.interactable = false;
                menuCanvas.blocksRaycasts = false;
            }
        }
        
        private void Update()
        {
            // Если нажата клавиша Escape и меню скрыто - показываем его
            if (Input.GetKeyDown(KeyCode.Escape) && !_isPaused)
            {
                ShowMenu();
                
                // Блокируем управление игрока
                if (_playerController != null)
                {
                    _playerController.enabled = false;
                }
            }
        }

        public void SetResumeButtonActive(bool active)
        {
            if (resumeButton != null)
            {
                resumeButton.gameObject.SetActive(active);
            }
        }
    }
} 