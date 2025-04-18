using UnityEngine;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] 
        private Transform orientation;
        private const string HorizontalAxis = "Horizontal";
        private const string VerticalAxis = "Vertical";
        private const string BlockTag = "Block";

        [Header("Movement")] 
        [SerializeField] float moveSpeed = 6f;
        [SerializeField] private float airMultiplier = 0.8f;
        [SerializeField] private float fallMultiplier = 2.5f;
        
        private const float MovementMultiplier = 10f;

        [Header("Sprinting")] 
        [SerializeField] float walkSpeed = 4f;
        [SerializeField] float sprintSpeed = 6f;
        [SerializeField] float acceleration = 10f;

        [Header("Sprinting Effects")] 
        [SerializeField] private Camera cam;
        [SerializeField] private float sprintFOV = 100f;
        [SerializeField] private float normalFOV = 90f;

        [Header("Jumping")] 
        public float jumpForce = 6f;
        public float jumpRate = 15f;
        public float jumpCooldown = 0.2f;

        [Header("Crouching")] 
        public float crouchScale = 0.75f;
        public float crouchSpeed = 3f;
        private Vector3 _originalScale;
        private const float CrouchMultiplier = 5f;

        [Header("Keybinds")] 
        [SerializeField] KeyCode jumpKey = KeyCode.Space;
        [SerializeField] private KeyCode sprintKey = KeyCode.LeftControl;
        [SerializeField] private KeyCode crouchKey = KeyCode.LeftShift;

        [Header("Drag")] 
        [SerializeField] private float groundDrag = 6f;
        [SerializeField] private float airDrag = 0.5f;

        private float _horizontalMovement;
        private float _verticalMovement;

        [Header("Ground Detection")] 
        [SerializeField] Transform groundCheck;
        [SerializeField] LayerMask groundMask;
        [SerializeField] float groundDistance = 0.4f;
        private bool IsGrounded { get; set; }

        [HideInInspector] public bool isCrouching;
        [HideInInspector] public bool isSprinting;
        [HideInInspector] public bool isMoving;

        private Vector3 _moveDirection;
        private Rigidbody _rb;
        private float _nextTimeToJump = 0f;
        private bool _startedJump = false;

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
            if (_rb == null)
            {
                Debug.LogError("Rigidbody не найден на игроке!", this);
                enabled = false;
                return;
            }

            _originalScale = transform.localScale;

            _rb.freezeRotation = true;
            _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            _rb.constraints = RigidbodyConstraints.FreezeRotation;
            
            if (groundCheck == null)
            {
                Debug.LogError("groundCheck не назначен!", this);
            }
            
            if (cam == null)
            {
                Debug.LogError("camera не назначена!", this);
            }
        }

        private void Update()
        {
            IsGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

            ProcessInput();
            
            ControlDrag();
            
            ControlSpeed();
            
            HandleJump();
            
            HandleCrouch();
            
            UpdateCameraEffects();
            
            DebugGroundedState();
        }

        private void ProcessInput()
        {
            _horizontalMovement = UnityEngine.Input.GetAxisRaw(HorizontalAxis);
            _verticalMovement = UnityEngine.Input.GetAxisRaw(VerticalAxis);

            _moveDirection = orientation.forward * _verticalMovement + orientation.right * _horizontalMovement;
            isMoving = _moveDirection.sqrMagnitude > 0.1f;
        }
        
        private void HandleJump()
        {
            if (UnityEngine.Input.GetKeyDown(jumpKey) && IsGrounded && Time.time >= _nextTimeToJump)
            {
                _nextTimeToJump = Time.time + jumpCooldown;
                _startedJump = true;
            }
        }
        
        private void HandleCrouch()
        {
            if (UnityEngine.Input.GetKeyDown(crouchKey) && !isCrouching)
            {
                Crouch();
            }
            else if (UnityEngine.Input.GetKeyUp(crouchKey) && isCrouching)
            {
                UnCrouch();
            }
        }
        
        private void UpdateCameraEffects()
        {
            float targetFOV = isSprinting && isMoving ? sprintFOV : normalFOV;
            if (cam != null)
            {
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, 8f * Time.deltaTime);
            }
        }
        
        private void DebugGroundedState()
        {
            Debug.DrawRay(groundCheck.position, Vector3.down * groundDistance, IsGrounded ? Color.green : Color.red);
        }

        private void Jump()
        {
            if (!IsGrounded) return;
            
            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
            
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            
            _startedJump = false;
        }

        private void Crouch()
        {
            Vector3 targetScale = new Vector3(_originalScale.x, _originalScale.y * crouchScale, _originalScale.z);
            transform.localScale = targetScale;
            
            if (_rb != null)
            {
                _rb.centerOfMass = Vector3.down * 0.5f;
            }
            
            isCrouching = true;
        }

        private void UnCrouch()
        {
            transform.localScale = _originalScale;
            
            if (_rb)
            {
                _rb.centerOfMass = Vector3.zero;
            }
            
            isCrouching = false;
        }

        private void ControlSpeed()
        {
            float targetSpeed;
            
            if (isCrouching)
            {
                targetSpeed = crouchSpeed;
                isSprinting = false;
            }
            else if (UnityEngine.Input.GetKey(sprintKey) && isMoving && IsGrounded)
            {
                targetSpeed = sprintSpeed;
                isSprinting = true;
            }
            else
            {
                targetSpeed = walkSpeed;
                isSprinting = false;
            }
            
            moveSpeed = Mathf.Lerp(moveSpeed, targetSpeed, acceleration * Time.deltaTime);
        }

        private void ControlDrag()
        {
            if (!_rb) return;
            
            _rb.linearDamping = IsGrounded ? groundDrag : airDrag;
        }

        private void FixedUpdate()
        {
            if (!_rb) return;
            
            if (_startedJump)
            {
                Jump();
            }
            
            MovePlayer();
            
            ApplyFallMultiplier();
            
            LimitVelocity();
        }

        private void LimitVelocity()
        {
            Vector3 horizontalVel = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
            if (!(horizontalVel.magnitude > moveSpeed * 2)) return;
            Vector3 limitedVel = horizontalVel.normalized * (moveSpeed * 2);
            _rb.linearVelocity = new Vector3(limitedVel.x, _rb.linearVelocity.y, limitedVel.z);
        }

        private void ApplyFallMultiplier()
        {
            if (_rb.linearVelocity.y < 0)
            {
                _rb.linearVelocity += Vector3.up * (Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime);
            }
        }

        private void MovePlayer()
        {
            if (_moveDirection.sqrMagnitude < 0.1f) return;
            
            Vector3 normalizedDirection = _moveDirection.normalized;
            
            if (IsGrounded)
            {
                if (isCrouching)
                {
                    _rb.AddForce(normalizedDirection * (moveSpeed * CrouchMultiplier), ForceMode.Acceleration);
                }
                else
                {
                    _rb.AddForce(normalizedDirection * (moveSpeed * MovementMultiplier), ForceMode.Acceleration);
                }
            }
            else
            {
                _rb.AddForce(normalizedDirection * (moveSpeed * MovementMultiplier * airMultiplier), ForceMode.Acceleration);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(BlockTag))
            {
                if (isSprinting && cam != null)
                {
                    cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, normalFOV, 8f * Time.deltaTime);
                    isSprinting = false;
                }
            }
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") || 
                collision.gameObject.CompareTag(BlockTag))
            {
                foreach (ContactPoint contact in collision.contacts)
                {
                    if (Vector3.Dot(contact.normal, Vector3.up) > 0.5f)
                    {
                        IsGrounded = true;
                        break;
                    }
                }
            }
        }
        
        public bool IsPlayerGrounded()
        {
            return IsGrounded;
        }
    }
}