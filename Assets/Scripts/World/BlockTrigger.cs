using System;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace World
{
    /// <summary>
    /// Компонент, который отслеживает взаимодействие игрока с блоком и отправляет события
    /// </summary>
    public class BlockTrigger : MonoBehaviour
    {
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private float activationHeight = 0.5f; // Высота над блоком для активации
        
        private bool _wasActivated = false;
        private BoxCollider _triggerCollider;
        private int _row = 0; // Номер ряда, к которому принадлежит блок
        private float _angle = 0; // Угол блока в спирали
        
        // События
        public static event Action<BlockTrigger> OnBlockPassed;
        
        // Статические переменные для отслеживания прогресса игрока
        private static float _lastBlockY = float.MinValue;
        private static float _lastBlockAngle = 0;
        private static TextMeshProUGUI _scoreText;
        private static int _totalScore = 0;
        
        // Словарь для хранения посещенных блоков по рядам
        private static Dictionary<int, HashSet<Vector3>> _visitedBlocksInRows = new Dictionary<int, HashSet<Vector3>>();
        
        // Последний посещенный ряд и угол в этом ряду
        private static int _lastVisitedRow = -1;
        private static float _lastRowAngle = 0;

        private void Awake()
        {
            // Настройка коллайдеров
            Collider mainCollider = GetComponent<Collider>();
            
            if (mainCollider == null)
            {
                mainCollider = gameObject.AddComponent<BoxCollider>();
            }
            
            if (mainCollider is BoxCollider boxCollider)
            {
                boxCollider.isTrigger = false; // Основной коллайдер - не триггер
            }
            
            // Добавляем триггер-коллайдер для блока
            _triggerCollider = gameObject.AddComponent<BoxCollider>();
            _triggerCollider.isTrigger = true;
            
            if (mainCollider is BoxCollider boxCollider2)
            {
                _triggerCollider.center = boxCollider2.center + new Vector3(0, 0.5f, 0);
                _triggerCollider.size = boxCollider2.size + new Vector3(0.1f, 1.0f, 0.1f);
            }
            else
            {
                _triggerCollider.center = new Vector3(0, 0.5f, 0);
                _triggerCollider.size = new Vector3(1.1f, 2f, 1.1f);
            }
        }
        
        private void OnEnable()
        {
            _wasActivated = false;
            
            // Вычисляем угол блока при активации
            Vector3 position = transform.position;
            _angle = Mathf.Atan2(position.z, position.x);
            if (_angle < 0) _angle += Mathf.PI * 2;
            
            // Инициализируем словарь посещенных блоков, если нужно
            if (_visitedBlocksInRows == null)
            {
                _visitedBlocksInRows = new Dictionary<int, HashSet<Vector3>>();
            }
        }
        
        private void Start()
        {
            if (_scoreText == null)
            {
                _scoreText = GameObject.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();
            }
        }
        
        /// <summary>
        /// Устанавливает ряд блока
        /// </summary>
        public void SetRow(int row)
        {
            _row = row;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(playerTag)) return;
            
            // Проверяем, находится ли игрок над блоком
            CheckIfPassed(other.transform);
        }
        
        private void OnTriggerStay(Collider other)
        {
            if (!_wasActivated && other.CompareTag(playerTag))
            {
                CheckIfPassed(other.transform);
            }
        }
        
        private void CheckIfPassed(Transform playerTransform)
        {
            // Если игрок находится над блоком
            if (playerTransform.position.y > transform.position.y + activationHeight)
            {
                // Если блок еще не был активирован
                if (!_wasActivated)
                {
                    _wasActivated = true;
                    OnBlockPassed?.Invoke(this);
                    
                    // Проверяем, прыгнул ли игрок на новый блок в ряду или на новый ряд
                    bool shouldAddScore = false;
                    
                    // Если это первый блок ряда или мы перешли на новый ряд
                    if (!_visitedBlocksInRows.ContainsKey(_row))
                    {
                        _visitedBlocksInRows[_row] = new HashSet<Vector3>();
                    }
                    
                    // Проверяем, посещали ли мы этот блок раньше
                    Vector3 blockPos = transform.position;
                    if (!_visitedBlocksInRows[_row].Contains(blockPos))
                    {
                        // Новый блок в этом ряду
                        _visitedBlocksInRows[_row].Add(blockPos);
                        
                        // Добавляем очки, если мы продвигаемся вперед по спирали или перешли в новый ряд
                        if (_row != _lastVisitedRow || IsMovingForwardInSpiral())
                        {
                            shouldAddScore = true;
                        }
                        
                        // Обновляем последний посещенный ряд и угол
                        _lastVisitedRow = _row;
                        _lastRowAngle = _angle;
                    }
                    
                    if (shouldAddScore)
                    {
                        _totalScore++;
                        UpdateScoreDisplay();
                    }
                }
            }
        }
        
        private bool IsMovingForwardInSpiral()
        {
            // В спирали, движение вперед означает увеличение угла
            // Но нужно учесть переход через 2PI
            float angleDiff = _angle - _lastRowAngle;
            
            // Нормализуем разницу углов в диапазоне [-PI, PI]
            if (angleDiff > Mathf.PI) angleDiff -= Mathf.PI * 2;
            if (angleDiff < -Mathf.PI) angleDiff += Mathf.PI * 2;
            
            // Если разница положительная, значит мы двигаемся вперед по спирали
            return angleDiff > 0;
        }
        
        private static void UpdateScoreDisplay()
        {
            if (_scoreText != null)
            {
                _scoreText.text = $"Очки: {_totalScore}";
            }
        }
        
        public static void ResetScore()
        {
            _totalScore = 0;
            _lastBlockY = float.MinValue;
            _lastBlockAngle = 0;
            _lastVisitedRow = -1;
            _lastRowAngle = 0;
            
            // Очищаем все посещенные блоки
            if (_visitedBlocksInRows != null)
            {
                _visitedBlocksInRows.Clear();
            }
            else
            {
                _visitedBlocksInRows = new Dictionary<int, HashSet<Vector3>>();
            }
            
            UpdateScoreDisplay();
        }
    }
} 