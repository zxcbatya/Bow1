using System;
using System.Collections;
using UnityEngine;
using Utils;
using System.Collections.Generic;
using Enemies;

namespace World
{
    /// <summary>
    /// Основной класс для генерации мира в виде спирали
    /// </summary>
    public class WorldGenerator : MonoBehaviour
    {
        [Header("Настройки генерации")]
        [SerializeField] private float spiralRadius = 12f;
        [SerializeField] private float blockSpacing = 1.2f;  
        [SerializeField] private float heightStep = 0.4f;   
        [SerializeField] private float minRadius = 5f;
        [SerializeField] private float maxRadius = 20f;
        [SerializeField] private float spiralTightness = 0.25f;
        
        [Header("Префабы")]
        [SerializeField] private GameObject blockPrefab;
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject enemyPrefab;
        
        [Header("Настройки спавна")]
        [SerializeField] private float spawnDelay = 0.02f;  
        [SerializeField] private int initialBlocks = 60;   
        [SerializeField] private int visibleDistance = 80;  
        [SerializeField] private float sideBlockChance = 0.7f; 
        [SerializeField] private int rowCount = 3;
        [SerializeField] private float rowSpacing = 3.0f;    
        [SerializeField] private float skeletonChance = 0.25f; 
        
        private Transform _container;
        private ObjectPool _blockPool;
        private ObjectPool _enemyPool;
        private GameObject _player;
        private bool _isInitialized;
        private int _currentBlockCount;
        private float _currentAngle;
        private float _currentHeight;
        private List<GameObject> blocks = new List<GameObject>();
        private List<GameObject> _recentBlocks = new List<GameObject>(); // Список последних созданных блоков для оптимизации проверки
        private int _maxRecentBlocksCount = 20; // Количество последних блоков для проверки
        private Dictionary<Vector3Int, GameObject> _blockGrid = new Dictionary<Vector3Int, GameObject>(); // Сетка для быстрой проверки позиций
        private Vector3Int _lastPlayerGridPos = Vector3Int.zero; // Последняя позиция игрока в сетке
        
        // Для отладки
        private List<Vector3> _spiralPoints = new List<Vector3>();
        
        private void Awake()
        {
            _container = new GameObject("WorldContainer").transform;
            
            // Увеличиваем размер пулов для большого числа блоков
            _blockPool = new ObjectPool(blockPrefab, 2000); // Увеличено до 2000 блоков
            _enemyPool = new ObjectPool(enemyPrefab, 100); // Увеличено до 100 скелетов
            
            spiralRadius = Mathf.Clamp(spiralRadius, minRadius, maxRadius);
            _currentAngle = 0f;
            _currentHeight = 0f;
            
            Debug.Log($"World initialized with spiral radius: {spiralRadius} and {rowCount} rows");
        }
        
        private void Start()
        {
            StartCoroutine(InitializeWorldCoroutine());
        }
        
        private IEnumerator InitializeWorldCoroutine()
        {
            yield return new WaitForSeconds(0.1f);
            
            // Очищаем существующие списки при повторной инициализации
            _spiralPoints.Clear();
            _blockGrid.Clear();
            blocks.Clear();
            _recentBlocks.Clear();
            _currentBlockCount = 0;
            
            // Если есть старый контейнер мира, удаляем его
            if (_container != null && _container.childCount > 0)
            {
                foreach (Transform child in _container)
                {
                    if (child != null)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
            
            // Создаем первый блок и точку старта
            Vector3 startPosition = new Vector3(spiralRadius, 0, 0);
            CreateBlockAt(startPosition);
            
            // Спавним игрока
            SpawnPlayer();
            yield return new WaitForSeconds(0.2f);
            
            // Генерируем начальную спираль
            for (int i = 0; i < initialBlocks; i++)
            {
                GenerateNextSpiralSection();
                
                // Ускоряем генерацию, генерируя по несколько блоков за кадр
                if (i % 5 == 0)
                {
                    yield return new WaitForSeconds(spawnDelay);
                }
            }
            
            _isInitialized = true;
            Debug.Log($"Initial world generation completed with {_currentBlockCount} blocks");
        }
        
        private void Update()
        {
            if (!_isInitialized || _player == null) return;
            
            // Получаем текущую позицию игрока в сетке
            Vector3Int playerGridPos = GetGridPosition(_player.transform.position);
            
            // Если игрок переместился на новую ячейку сетки, обновляем видимые блоки
            if (playerGridPos != _lastPlayerGridPos)
            {
                _lastPlayerGridPos = playerGridPos;
                
                // Генерируем новые блоки, если нужно
                if (ShouldGenerateMoreBlocks())
                {
                    for (int i = 0; i < 10; i++) // Генерируем 10 секций спирали сразу
                    {
                        GenerateNextSpiralSection();
                    }
                }
                
                // Управляем видимостью блоков
                ManageBlockVisibility();
            }
        }
        
        // Преобразует мировую позицию в позицию в сетке (для эффективной проверки близости)
        private Vector3Int GetGridPosition(Vector3 worldPos)
        {
            int gridSize = 3;
            return new Vector3Int(
                Mathf.FloorToInt(worldPos.x / gridSize),
                Mathf.FloorToInt(worldPos.y / gridSize),
                Mathf.FloorToInt(worldPos.z / gridSize)
            );
        }
        
        // Управляет видимостью блоков, активируя только те, что близко к игроку
        private void ManageBlockVisibility()
        {
            // Если блоков мало, нет смысла оптимизировать
            if (_currentBlockCount < visibleDistance * 2) return;
            
            foreach (GameObject block in blocks)
            {
                if (block == null) continue;
                
                float distance = Vector3.Distance(_player.transform.position, block.transform.position);
                
                // Блоки в пределах видимости активны, дальние деактивированы
                if (distance < visibleDistance)
                {
                    if (!block.activeSelf) block.SetActive(true);
                }
                else
                {
                    if (block.activeSelf) block.SetActive(false);
                }
            }
        }
        
        private void SpawnPlayer()
        {
            Vector3 spawnPosition = new Vector3(spiralRadius, 1.5f, 0f);
            _player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            Debug.Log($"Player spawned at position: {spawnPosition}");
        }
        
        private bool ShouldGenerateMoreBlocks()
        {
            if (blocks.Count == 0) return true;
            
            // Находим расстояние до самого дальнего блока
            float maxDistance = 0f;
            foreach (GameObject block in blocks)
            {
                if (block == null) continue;
                float distance = Vector3.Distance(_player.transform.position, block.transform.position);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                }
            }
            
            // Если максимальное расстояние меньше видимой дистанции, нужно больше блоков
            return maxDistance < visibleDistance * 1.2f;
        }
        
        private void GenerateNextSpiralSection()
        {
            // Увеличиваем угол для спирали
            _currentAngle += blockSpacing / spiralRadius;
            
            // Увеличиваем высоту (важно для 3D спирали)
            _currentHeight += heightStep * spiralTightness;
            
            // Генерируем основной блок на спирали
            Vector3 mainPosition = CalculateSpiralPosition(spiralRadius, _currentAngle, _currentHeight);
            bool mainBlockCreated = CreateBlockAt(mainPosition);
            
            // Сохраняем точку для отрисовки спирали
            if (mainBlockCreated)
            {
                _spiralPoints.Add(mainPosition);
            }
            
            // Генерируем блоки для дополнительных рядов спирали
            for (int row = 1; row < rowCount; row++)
            {
                // Внешние ряды с тем же углом и высотой, но большим радиусом
                float rowRadius = spiralRadius + (rowSpacing * row);
                Vector3 rowPosition = CalculateSpiralPosition(rowRadius, _currentAngle, _currentHeight);
                bool rowBlockCreated = CreateBlockAt(rowPosition);
                
                // Добавляем соединительные блоки между рядами с большей вероятностью
                if (rowBlockCreated && mainBlockCreated && UnityEngine.Random.value < sideBlockChance)
                {
                    // Промежуточные блоки для соединения рядов
                    int connectionBlocks = UnityEngine.Random.Range(1, 3); // 1-2 соединительных блока
                    
                    for (int i = 0; i < connectionBlocks; i++)
                    {
                        float t = (i + 1.0f) / (connectionBlocks + 1.0f); // Равномерно распределяем
                        float intermediateRadius = Mathf.Lerp(spiralRadius, rowRadius, t);
                        
                        // Небольшое случайное отклонение для разнообразия
                        float intermediateAngle = _currentAngle + (UnityEngine.Random.value * 0.1f - 0.05f);
                        
                        // Небольшое изменение высоты для сложности паркура
                        float heightVariation = UnityEngine.Random.value * 0.5f - 0.25f;
                        
                        Vector3 intermediatePosition = CalculateSpiralPosition(
                            intermediateRadius, 
                            intermediateAngle, 
                            _currentHeight + heightVariation
                        );
                        
                        CreateBlockAt(intermediatePosition);
                    }
                }
            }
            
            // Спавним скелетов с увеличенной вероятностью
            if (mainBlockCreated && _player != null && UnityEngine.Random.value < skeletonChance)
            {
                TrySpawnSkeleton(mainPosition);
            }
        }
        
        private Vector3 CalculateSpiralPosition(float radius, float angle, float height)
        {
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            return new Vector3(x, height, z);
        }
        
        private bool CreateBlockAt(Vector3 position)
        {
            // Проверяем, нет ли уже блока рядом
            if (!IsValidPosition(position)) return false;
            
            // Проверяем, что пул блоков инициализирован
            if (_blockPool == null) 
            {
                Debug.LogError("Block pool is not initialized!");
                return false;
            }
            
            // Берем блок из пула
            GameObject block = _blockPool.GetObject();
            if (block == null) return false;
            
            // Устанавливаем позицию и родителя
            block.transform.position = position;
            block.transform.rotation = Quaternion.identity;
            block.transform.SetParent(_container);
            
            // Устанавливаем номер ряда для BlockTrigger
            BlockTrigger trigger = block.GetComponent<BlockTrigger>();
            if (trigger != null)
            {
                // Для основного спирального ряда - ряд 0
                // Для внешних рядов - ряд N в зависимости от расстояния от центра
                float distance = Mathf.Sqrt(position.x * position.x + position.z * position.z);
                
                // Определяем номер ряда на основе радиуса позиции
                int row = 0;
                if (distance < spiralRadius * 0.9f)
                {
                    row = 0; // Внутренние блоки
                }
                else if (distance < spiralRadius * 1.1f)
                {
                    row = 1; // Основной ряд спирали
                }
                else
                {
                    // Вычисляем номер внешнего ряда
                    row = Mathf.CeilToInt((distance - spiralRadius) / rowSpacing) + 1;
                }
                
                trigger.SetRow(row);
            }
            
            // Добавляем в списки блоков
            blocks.Add(block);
            _blockGrid[GetGridPosition(position)] = block;
            
            // Добавляем в список последних блоков для оптимизации проверки коллизий
            _recentBlocks.Add(block);
            if (_recentBlocks.Count > _maxRecentBlocksCount)
            {
                _recentBlocks.RemoveAt(0); // Удаляем самый старый блок из списка
            }
            
            _currentBlockCount++;
            return true;
        }
        
        private bool IsValidPosition(Vector3 position)
        {
            // Используем сетку для быстрой проверки соседних ячеек
            Vector3Int gridPos = GetGridPosition(position);
            float minDistance = blockSpacing * 0.8f; // Увеличено для предотвращения перекрытий
            
            // Проверяем текущую и соседние ячейки сетки (расширенный диапазон поиска)
            for (int x = -2; x <= 2; x++)
            {
                for (int y = -2; y <= 2; y++)
                {
                    for (int z = -2; z <= 2; z++)
                    {
                        Vector3Int checkPos = gridPos + new Vector3Int(x, y, z);
                        if (_blockGrid.TryGetValue(checkPos, out GameObject existingBlock))
                        {
                            if (existingBlock != null)
                            {
                                float distance = Vector3.Distance(position, existingBlock.transform.position);
                                if (distance < minDistance)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            
            // Проверяем только последние созданные блоки для оптимизации
            foreach (GameObject block in _recentBlocks)
            {
                if (block == null) continue;
                
                float distance = Vector3.Distance(position, block.transform.position);
                if (distance < minDistance)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        private void TrySpawnSkeleton(Vector3 position)
        {
            if (_player == null)
            {
                Debug.LogWarning("Cannot spawn skeleton: player not found");
                return;
            }
            
            // Вычисляем позицию для спавна скелета над блоком
            Vector3 spawnPosition = position + Vector3.up * 1.5f;
            
            // Проверяем, нет ли над блоком других блоков
            bool isSpaceFree = true;
            Vector3Int gridPos = GetGridPosition(spawnPosition);
            
            // Проверяем соседние ячейки сетки над блоком
            for (int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    // Проверяем несколько ячеек вверх, чтобы гарантировать достаточно места
                    for (int y = 0; y <= 1; y++)
                    {
                        Vector3Int checkPos = gridPos + new Vector3Int(x, y, z);
                        if (_blockGrid.TryGetValue(checkPos, out GameObject blockAbove) && blockAbove != null)
                        {
                            float verticalDistance = Mathf.Abs(blockAbove.transform.position.y - spawnPosition.y);
                            if (verticalDistance < 1.5f) // Минимальное расстояние для скелета
                            {
                                isSpaceFree = false;
                                break;
                            }
                        }
                    }
                }
            }
            
            if (!isSpaceFree)
            {
                Debug.LogWarning("Cannot spawn skeleton: not enough space");
                return;
            }
            
            // Проверяем, нет ли слишком близко других скелетов (минимум 20 единиц)
            foreach (Transform child in _container)
            {
                if (child != null && child.CompareTag("Enemy"))
                {
                    float distanceToOtherEnemy = Vector3.Distance(spawnPosition, child.position);
                    if (distanceToOtherEnemy < 20f)
                    {
                        Debug.LogWarning($"Cannot spawn skeleton: too close to existing enemy ({distanceToOtherEnemy} units)");
                        return;
                    }
                }
            }
            
            // Проверяем инициализацию пула врагов
            if (_enemyPool == null)
            {
                Debug.LogError("Enemy pool is null, cannot spawn skeleton");
                return;
            }
            
            // Берем скелета из пула
            GameObject enemy = _enemyPool.GetObject();
            
            if (enemy == null)
            {
                Debug.LogError("Failed to get enemy from pool");
                return;
            }
            
            // Убедимся, что у скелета правильный тег
            if (enemy.tag != "Enemy")
            {
                enemy.tag = "Enemy";
                Debug.Log($"Set tag 'Enemy' to {enemy.name}");
            }
            
            // Настраиваем скелета
            enemy.transform.position = spawnPosition;
            enemy.transform.rotation = Quaternion.Euler(-90f, 0f, 0f); // X=-90 для модели скелета
            enemy.transform.SetParent(_container);
            
            // Проверяем, активен ли объект
            if (!enemy.activeSelf)
            {
                enemy.SetActive(true);
                Debug.Log($"Activated skeleton at {spawnPosition}");
            }
            
            // Устанавливаем компонент скелета
            EnemySkeleton skeleton = enemy.GetComponent<EnemySkeleton>();
            if (skeleton != null)
            {
                skeleton.Initialize(_player.transform);
                Debug.Log($"Initialized skeleton with player reference at position {spawnPosition}");
            }
            else
            {
                Debug.LogError($"Enemy prefab has no EnemySkeleton component!");
            }
        }
        
        public void ResetWorld()
        {
            Debug.Log("Resetting world...");
            
            // Сначала деактивируем существующих скелетов, чтобы вернуть их стрелы в пул
            foreach (Transform child in _container)
            {
                if (child != null && child.CompareTag("Enemy"))
                {
                    EnemySkeleton skeleton = child.GetComponent<EnemySkeleton>();
                    if (skeleton != null)
                    {
                        skeleton.TakeDamage(); // Метод TakeDamage возвращает стрелы в пул
                    }
                }
            }
            
            // Короткая задержка для возвращения стрел
            StartCoroutine(ResetWorldAfterDelay(0.2f));
        }
        
        private IEnumerator ResetWorldAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            // Возвращаем все блоки в пул
            foreach (Transform child in _container)
            {
                if (child == null) continue;
                
                if (child.CompareTag("Block"))
                {
                    _blockPool.ReturnObject(child.gameObject);
                }
                else if (child.CompareTag("Enemy"))
                {
                    _enemyPool.ReturnObject(child.gameObject);
                }
                else
                {
                    Destroy(child.gameObject);
                }
            }
            
            // Очищаем все списки и сбрасываем счетчики
            blocks.Clear();
            _recentBlocks.Clear();
            _blockGrid.Clear();
            _spiralPoints.Clear();
            _currentBlockCount = 0;
            _currentAngle = 0f;
            _currentHeight = 0f;
            
            try
            {
                BlockTrigger.ResetScore();
            }
            catch (Exception e)
            {
                Debug.LogWarning("Failed to reset score: " + e.Message);
            }
            
            StartCoroutine(InitializeWorldCoroutine());
        }
        
        private void OnDrawGizmos()
        {
            // Рисуем основной радиус спирали
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(Vector3.zero, spiralRadius);
            
            // Рисуем радиусы для дополнительных рядов
            for (int i = 1; i < rowCount; i++)
            {
                float outerRadius = spiralRadius + (rowSpacing * i);
                Gizmos.color = new Color(0, 1, 1, 0.5f);
                Gizmos.DrawWireSphere(Vector3.zero, outerRadius);
            }
            
            // Визуализируем текущую спираль
            if (_spiralPoints.Count > 1)
            {
                Gizmos.color = Color.red;
                for (int i = 0; i < _spiralPoints.Count - 1; i++)
                {
                    Gizmos.DrawLine(_spiralPoints[i], _spiralPoints[i + 1]);
                }
            }
            
            Gizmos.color = Color.green;
            Vector3 prevPoint = Vector3.zero;
            float totalAngle = 10f * Mathf.PI * 2; // 10 полных оборотов
            for (float angle = 0; angle < totalAngle; angle += 0.1f)
            {
                float height = (angle / (2 * Mathf.PI)) * heightStep * 20f * spiralTightness;
                Vector3 point = CalculateSpiralPosition(spiralRadius, angle, height);
                
                if (prevPoint != Vector3.zero)
                {
                    Gizmos.DrawLine(prevPoint, point);
                }
                prevPoint = point;
            }
            
            if (_player != null && Application.isPlaying)
            {
                Gizmos.color = new Color(1, 0, 0, 0.2f);
                Gizmos.DrawWireSphere(_player.transform.position, visibleDistance);
            }
        }
    }
}