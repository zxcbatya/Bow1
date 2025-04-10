using TMPro;
using UnityEngine;

namespace World
{
    /// <summary>
    /// Менеджер счета для управления и инициализации всех компонентов счета в игре
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        [SerializeField] private Canvas scoreCanvas;
        [SerializeField] private TextMeshProUGUI scoreTextPrefab;
        
        private GameObject _scoreDisplayObject;
        private TextMeshProUGUI _scoreText;
        private int _currentScore = 0;
        
        public static ScoreManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                Debug.Log("ScoreManager initialized as singleton instance");
            }
            else
            {
                Debug.LogWarning("Multiple instances of ScoreManager found. Destroying this one.");
                Destroy(gameObject);
                return;
            }
            
            InitializeScoreDisplay();
        }
        
        private void InitializeScoreDisplay()
        {
            Debug.Log("Initializing score display...");
            
            // Если в сцене нет Canvas для UI, создаем его
            if (scoreCanvas == null)
            {
                GameObject canvasObject = new GameObject("ScoreCanvas");
                scoreCanvas = canvasObject.AddComponent<Canvas>();
                scoreCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                
                // Добавляем необходимые компоненты для Canvas
                UnityEngine.UI.CanvasScaler scaler = canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
                scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f; // Баланс между шириной и высотой
                
                canvasObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                
                // Устанавливаем Canvas как последний в иерархии, чтобы он рисовался поверх других UI
                scoreCanvas.sortingOrder = 100;
                Debug.Log("Created new ScoreCanvas");
            }
            
            // Создаем объект для отображения счета
            _scoreDisplayObject = new GameObject("ScoreDisplay");
            _scoreDisplayObject.transform.SetParent(scoreCanvas.transform, false);
            
            // Создаем объект с текстом
            GameObject textObject;
            if (scoreTextPrefab != null)
            {
                textObject = Instantiate(scoreTextPrefab.gameObject, _scoreDisplayObject.transform);
                Debug.Log("Created score text from prefab");
            }
            else
            {
                textObject = new GameObject("ScoreText");
                textObject.transform.SetParent(_scoreDisplayObject.transform, false);
                TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
                text.text = "Счёт: 0";
                text.fontSize = 36;
                text.alignment = TextAlignmentOptions.TopLeft;
                text.color = Color.white;
                text.fontStyle = FontStyles.Bold;
                
                // Добавляем тень для лучшей видимости на разных фонах
                UnityEngine.UI.Shadow shadow = textObject.AddComponent<UnityEngine.UI.Shadow>();
                shadow.effectColor = new Color(0, 0, 0, 0.8f);
                shadow.effectDistance = new Vector2(2, -2);
                
                // Добавляем обводку текста
                UnityEngine.UI.Outline outline = textObject.AddComponent<UnityEngine.UI.Outline>();
                outline.effectColor = Color.black;
                outline.effectDistance = new Vector2(1, -1);
                Debug.Log("Created default score text");
            }
            
            // Настраиваем позицию
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 1);
            textRect.anchorMax = new Vector2(0, 1);
            textRect.pivot = new Vector2(0, 1);
            textRect.anchoredPosition = new Vector2(20, -20);
            textRect.sizeDelta = new Vector2(200, 50);
            
            // Настраиваем сам объект для отображения счета
            RectTransform displayRect = _scoreDisplayObject.GetComponent<RectTransform>();
            if (displayRect == null)
            {
                displayRect = _scoreDisplayObject.AddComponent<RectTransform>();
            }
            displayRect.anchorMin = new Vector2(0, 1);
            displayRect.anchorMax = new Vector2(0, 1);
            displayRect.pivot = new Vector2(0, 1);
            displayRect.anchoredPosition = Vector2.zero;
            displayRect.sizeDelta = new Vector2(200, 50);
            
            // Получаем ссылку на компонент текста
            _scoreText = textObject.GetComponent<TextMeshProUGUI>();
            
            Debug.Log("Score display initialized successfully");
        }
        
        public void AddScore(int points)
        {
            _currentScore += points;
            UpdateScoreDisplay();
            Debug.Log($"Added {points} points. New score: {_currentScore}");
        }
        
        public void ResetScore()
        {
            _currentScore = 0;
            UpdateScoreDisplay();
            Debug.Log("Score reset to 0");
        }
        
        private void UpdateScoreDisplay()
        {
            if (_scoreText != null)
            {
                _scoreText.text = $"Счёт: {_currentScore}";
            }
            else
            {
                Debug.LogWarning("Cannot update score: _scoreText is null");
            }
        }
    }
} 