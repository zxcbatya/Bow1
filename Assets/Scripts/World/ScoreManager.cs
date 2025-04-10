using UnityEngine;
using World.Score;

namespace World
{
    /// <summary>
    /// Менеджер счета для управления и инициализации всех компонентов счета в игре
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        [SerializeField] private Canvas scoreCanvas;
        [SerializeField] private ScoreView scoreViewPrefab;
        
        private ScoreModel _scoreModel;
        private ScoreView _scoreView;
        
        public static ScoreManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            InitializeScore();
        }
        
        private void InitializeScore()
        {
            
            if (scoreCanvas == null)
            {
                GameObject canvasObject = new GameObject("ScoreCanvas");
                scoreCanvas = canvasObject.AddComponent<Canvas>();
                scoreCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                
                var scaler = canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
                scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
                
                canvasObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                scoreCanvas.sortingOrder = 100;
                
                DontDestroyOnLoad(canvasObject);
            }
            
            _scoreModel = new ScoreModel();
            
            if (scoreViewPrefab != null)
            {
                _scoreView = Instantiate(scoreViewPrefab);
            }
            else
            {
                GameObject viewObject = new GameObject("ScoreView");
                _scoreView = viewObject.AddComponent<ScoreView>();
            }
            
            _scoreView.Initialize(scoreCanvas);
            
            // Подписываемся на события
            _scoreModel.OnScoreChanged += _scoreView.UpdateScore;
            BlockTrigger.OnBlockPassed += OnBlockPassed;
            Debug.Log("Subscribed to BlockTrigger.OnBlockPassed event");
        }
        
        private void OnDestroy()
        {
            if (_scoreModel != null)
            {
                _scoreModel.OnScoreChanged -= _scoreView.UpdateScore;
            }
            BlockTrigger.OnBlockPassed -= OnBlockPassed;
        }
        
        private void OnBlockPassed(BlockTrigger trigger)
        {
            if (_scoreModel != null)
            {
                bool scoreAdded = _scoreModel.TryAddScore(trigger.GetHeight(), trigger.GetRow());
            }
        }
        
        public void ResetScore()
        {
            if (_scoreModel != null)
            {
                _scoreModel.ResetScore();
            }
        }
    }
} 