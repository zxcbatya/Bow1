using UnityEngine;
using TMPro;

namespace World
{
    /// <summary>
    /// Компонент, отвечающий за отображение счета на сцене
    /// </summary>
    public class ScoreDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private bool findTextComponentAutomatically = true;
        
        private ScoreCounter _scoreCounter;
        
        private void Awake()
        {
            if (findTextComponentAutomatically && scoreText == null)
            {
                scoreText = GetComponentInChildren<TextMeshProUGUI>();
            }
            
            _scoreCounter = GetComponent<ScoreCounter>();
            
            if (_scoreCounter == null)
            {
                _scoreCounter = gameObject.AddComponent<ScoreCounter>();
            }
            
            if (scoreText == null)
            {
                Debug.LogError("ScoreDisplay: TextMeshProUGUI компонент не найден!");
            }
        }
        
        private void Start()
        {
            UpdateDisplay(0);
        }
        
        public void UpdateDisplay(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Счёт: {score}";
            }
        }

        public void FindTextComponent()
        {
            if (scoreText == null)
            {
                scoreText = GetComponentInChildren<TextMeshProUGUI>();
                
                if (scoreText == null)
                {
                    Debug.LogError("ScoreDisplay: TextMeshProUGUI компонент не найден при вызове FindTextComponent!");
                }
            }
        }
    }
} 