using UnityEngine;
using World.Score;

namespace World
{
    /// <summary>
    /// Менеджер счета для управления и инициализации всех компонентов счета в игре
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }
        
        [Header("UI компоненты")]
        [SerializeField] private Canvas scoreCanvas;
        [SerializeField] private ScoreView scoreViewPrefab;
        
        private ScoreModel _scoreModel;
        private ScoreView _scoreView;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
                InitializeScore();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeScore()
        {
            if (scoreCanvas == null)
            {
                Debug.LogError("ScoreManager: не назначен Canvas!");
                enabled = false;
                return;
            }
            
            _scoreModel = new ScoreModel();
            
            if (scoreViewPrefab != null)
            {
                _scoreView = Instantiate(scoreViewPrefab, scoreCanvas.transform);
            }
            else
            {
                Debug.LogError("ScoreManager: не назначен префаб ScoreView!");
                enabled = false;
                return;
            }
            
            _scoreModel.OnScoreChanged += _scoreView.UpdateScore;
            BlockTrigger.OnBlockPassed += OnBlockPassed;
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
            _scoreModel?.TryAddScore(trigger.GetHeight(), trigger.GetRow());
        }
        
        public void ResetScore()
        {
            _scoreModel?.ResetScore();
        }
    }
} 