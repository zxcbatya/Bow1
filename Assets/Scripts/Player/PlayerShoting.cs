using Player;
using UnityEngine;

public class PlayerShoting : MonoBehaviour
{
    [Header("Стрельба")]
    [SerializeField] private float drawSpeed = 0.5f; // Скорость натяжения лука
    [SerializeField] private float damage = 10f;
    [SerializeField] private float range = 100f;
    [SerializeField] private Transform arrowSpawnPoint;
    [SerializeField] private GameObject arrowPrefab; // Префаб стрелы
    [SerializeField] private LayerMask shootableLayers;
    
    [Header("Компоненты")]
    [SerializeField] private PlayerAnimationController animationController;
    
    private bool isDrawingBow = false;
    private Camera mainCamera;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        mainCamera = Camera.main;
        if (animationController == null)
        {
            animationController = GetComponent<PlayerAnimationController>();
        }
    }

    // Update is called once per frame
    private void Update()
    {
        HandleBowInput();
    }
    
    private void HandleBowInput()
    {
        // Начало натяжения лука
        if (Input.GetMouseButton(1) && !isDrawingBow)
        {
            isDrawingBow = true;
            animationController.StartDrawingBow();
        }
        
        // Удержание натянутого лука
        if (Input.GetMouseButton(1) && isDrawingBow)
        {
            if (animationController.IsBowDrawn())
            {
                animationController.BowDrawn();
            }
        }
        
        // Отпускание стрелы
        if (Input.GetMouseButtonUp(1) && isDrawingBow)
        {
            ShootArrow();
            isDrawingBow = false;
            animationController.ReleaseArrow();
        }
    }
    
    private void ShootArrow()
    {
        if (arrowPrefab != null && arrowSpawnPoint != null)
        {
            // Создаем стрелу
            GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, arrowSpawnPoint.rotation);
            Rigidbody arrowRb = arrow.GetComponent<Rigidbody>();
            
            if (arrowRb != null)
            {
                // Запускаем стрелу в направлении взгляда камеры
                arrowRb.linearVelocity = mainCamera.transform.forward * 30f; // Скорость стрелы
            }
        }
        
        RaycastHit hit;
        if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out hit, range, shootableLayers))
        {
            // Логика попадания
        }
    }
}