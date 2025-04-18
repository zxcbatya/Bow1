using UnityEngine;
using TMPro;

namespace World.Score
{
    public class ScoreView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private Vector2 anchoredPosition = new Vector2(20, -20);
        [SerializeField] private Vector2 sizeDelta = new Vector2(200, 50);
        
        private void Awake()
        {
            if (scoreText == null)
            {
                // Если текст не назначен, пробуем найти его в дочерних объектах
                scoreText = GetComponentInChildren<TextMeshProUGUI>();
            }
        }
        
        public void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Очки: {score}";
            }
            else
            {
                Debug.LogWarning("ScoreText component is missing!");
            }
        }
        
        public void Initialize(Canvas parentCanvas)
        {
            if (scoreText == null)
            {
                GameObject textObject = new GameObject("ScoreText");
                textObject.transform.SetParent(transform);
                
                scoreText = textObject.AddComponent<TextMeshProUGUI>();
                scoreText.fontSize = 36;
                scoreText.alignment = TextAlignmentOptions.TopLeft;
                scoreText.color = Color.white;
                scoreText.fontStyle = FontStyles.Bold;
                
                // Добавляем тень
                var shadow = textObject.AddComponent<UnityEngine.UI.Shadow>();
                shadow.effectColor = new Color(0, 0, 0, 0.8f);
                shadow.effectDistance = new Vector2(2, -2);
                
                // Добавляем обводку
                var outline = textObject.AddComponent<UnityEngine.UI.Outline>();
                outline.effectColor = Color.black;
                outline.effectDistance = new Vector2(1, -1);
                
                // Настраиваем RectTransform
                RectTransform textRect = textObject.GetComponent<RectTransform>();
                textRect.anchorMin = new Vector2(0, 1);
                textRect.anchorMax = new Vector2(0, 1);
                textRect.pivot = new Vector2(0, 1);
                textRect.anchoredPosition = anchoredPosition;
                textRect.sizeDelta = sizeDelta;
            }
            
            // Настраиваем RectTransform самого объекта
            RectTransform rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
            }
            
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = sizeDelta;
            
            transform.SetParent(parentCanvas.transform, false);
            gameObject.SetActive(true);
        }
    }
} 