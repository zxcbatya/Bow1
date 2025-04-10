using UnityEngine;

namespace Player
{
    public class PlayerLook : MonoBehaviour
    {
        [SerializeField] private float sensX = 100f;
        [SerializeField] private float sensY = 100f;

        [SerializeField] Transform cam = null;
        [SerializeField] Transform orientation = null;

        float _mouseX;
        float _mouseY;

        float multiplier = 0.01f;

        float _xRotation;
        float _yRotation;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            _mouseX = Input.GetAxisRaw("Mouse X");
            _mouseY = Input.GetAxisRaw("Mouse Y");

            _yRotation += _mouseX * sensX * multiplier;
            _xRotation -= _mouseY * sensY * multiplier;

            _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

            cam.transform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0f);
            ;
            orientation.transform.rotation = Quaternion.Euler(0, _yRotation, 0);
        }
    }
}