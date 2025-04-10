using UnityEngine;

namespace Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] 
        private Transform orientation;
        private const string HorizontalAxis = "Horizontal";
        private const string VerticalAxis = "Vertical";
        private const string BlockTag = "Block";

        [Header("Movement")] [SerializeField] float moveSpeed = 6f;
        [SerializeField] private float airMultiplier = 0.4f;
        [SerializeField] private float fallMultiplier = 2.5f;
        
        private const float MovementMultiplier = 10f;

        [Header("Sprinting")] [SerializeField] float walkSpeed = 4f;
        [SerializeField] float sprintSpeed = 6f;
        [SerializeField] float acceleration = 10f;

        [Header("Sprinting Effects")] [SerializeField]
        private Camera cam;

        [SerializeField] 
        private float sprintFOV = 100f;

        [Header("Jumping")] 
        public float jumpForce = 5f;
        public float jumpRate = 15f;

        [Header("Crouching")] 
        public float crouchScale = 0.75f;
        public float crouchSpeed = 1f;
        private const float CrouchMultiplier = 5f;

        [Header("Keybinds")] 
        [SerializeField] 
        KeyCode jumpKey = KeyCode.Space;
        [SerializeField] 
        private KeyCode sprintKey = KeyCode.LeftControl;
        [SerializeField] 
        private KeyCode crouchKey = KeyCode.LeftShift;

        [Header("Drag")] 
        [SerializeField]
        private float groundDrag = 6f;
        [SerializeField] private float airDrag = 0.1f;

        private float _horizontalMovement;
        private float _verticalMovement;

        [Header("Ground Detection")] [SerializeField]
        Transform groundCheck;

        [SerializeField] LayerMask groundMask;
        [SerializeField] float groundDistance = 0.2f;
        private bool IsGrounded { get; set; }

        [HideInInspector] public bool isCrouching;
        [HideInInspector] public bool isSprinting;
        [HideInInspector] public bool isMoving;

       private Vector3 _moveDirection;

       private Rigidbody _rb;

       private float _nextTimeToJump = 0f;

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.freezeRotation = true;
        }

        private void Update()
        {
            IsGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

            isMoving = _moveDirection != new Vector3(0f, 0f, 0f);

            Input();
            ControlDrag();
            ControlSpeed();

            if (UnityEngine.Input.GetKey(jumpKey) && Time.time >= _nextTimeToJump)
            {
                _nextTimeToJump = Time.time + 1f / jumpRate;
                Jump();
            }

            if (UnityEngine.Input.GetKeyDown(crouchKey) && !isCrouching)
            {
                Crouch();
            }
            else if (UnityEngine.Input.GetKeyUp(crouchKey) && isCrouching)
            {
                UnCrouch();
            }

            switch (isSprinting)
            {
                case true when isMoving:
                    cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, sprintFOV, 8f * Time.deltaTime);
                    break;
                case false when isMoving:
                case false when !isMoving:
                    cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 90f, 8f * Time.deltaTime);
                    break;
            }
        }

        private void Input()
        {
            _horizontalMovement = UnityEngine.Input.GetAxisRaw(HorizontalAxis);
            _verticalMovement = UnityEngine.Input.GetAxisRaw(VerticalAxis);

            _moveDirection = orientation.forward * _verticalMovement + orientation.right * _horizontalMovement;
        }

        private void Jump()
        {
            if (IsGrounded)
            {
                _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
                _rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            }
        }

        private void Crouch()
        {
            Vector3 localScale = new Vector3(transform.localScale.x, crouchScale, transform.localScale.z);
            transform.localScale = localScale;

            isCrouching = true;
        }

        private void UnCrouch()
        {
            Vector3 normalScale = new Vector3(transform.localScale.x, 0.9f, transform.localScale.z);
            transform.localScale = normalScale;

            isCrouching = false;
        }

        private void ControlSpeed()
        {
            if (UnityEngine.Input.GetKey(sprintKey) && isMoving)
            {
                moveSpeed = Mathf.Lerp(moveSpeed, sprintSpeed, acceleration * Time.deltaTime);
                isSprinting = true;
            }
            else
            {
                moveSpeed = Mathf.Lerp(moveSpeed, walkSpeed, acceleration * Time.deltaTime);
                isSprinting = false;
            }

            moveSpeed = Mathf.Lerp(moveSpeed, UnityEngine.Input.GetKey(crouchKey) ? crouchSpeed : walkSpeed, acceleration * Time.deltaTime);
        }

        private void ControlDrag()
        {
            _rb.linearDamping = IsGrounded ? groundDrag : airDrag;
        }

        private void FixedUpdate()
        {
            MovePlayer();
            ApplyFallMultiplier();
        }

        private void ApplyFallMultiplier()
        {
            if (_rb.linearVelocity.y < 0)
            {
                _rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
            }
        }

        private void MovePlayer()
        {
            if (IsGrounded)
            {
                _rb.AddForce(_moveDirection.normalized * moveSpeed * MovementMultiplier, ForceMode.Acceleration);
            }
            else if (!IsGrounded)
            {
                _rb.AddForce(_moveDirection.normalized * moveSpeed * MovementMultiplier * airMultiplier,
                    ForceMode.Acceleration);
            }
            else if (isCrouching)
            {
                _rb.AddForce(_moveDirection.normalized * moveSpeed * CrouchMultiplier, ForceMode.Acceleration);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(BlockTag))
            {
                if (isSprinting)
                {
                    cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 90f, 8f * Time.deltaTime);
                    isSprinting = false;
                }
            }
        }
    }
}