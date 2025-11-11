using System;
using UnityEngine;

namespace SPHSimulator.Controls
{
    public class FreeCameraController : MonoBehaviour
    {
        public Action OnCursorLocked;
        public Action OnCursorUnlocked;
        public Action<Quaternion> OnCameraRotation;

        private readonly Vector3 defaultPosition = new Vector3(8.3f, 3.2f, 8.2f);

        private readonly Vector3 defaultRotation = new Vector3(11, 226, 0);

        [Header("Movement Settings")]
        [SerializeField]
        private float moveSpeed = 5f;

        [SerializeField]
        private float boostMultiplier = 2f;

        [SerializeField]
        private float mouseSensitivity = 2f;

        private float rotationX = 0f;
        private float rotationY = 0f;

        public void InvokeCameraRotationCallback()
        {
            //Get rotation delta of identity rotation compared to current rotation.
            OnCameraRotation?.Invoke(Quaternion.Inverse(Quaternion.identity) * transform.rotation);
        }

        private void Start()
        {
            UnlockCursor();
            transform.position = defaultPosition;
            transform.eulerAngles = defaultRotation;

            rotationX = transform.eulerAngles.x;
            rotationY = transform.eulerAngles.y;

            InvokeCameraRotationCallback();
        }

        private void Update()
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                HandleMovement();
                HandleMouseLook();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                RefocusCamera();
            }
            HandleCursorToggle();
        }

        private void HandleCursorToggle()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UnlockCursor();
            }
            else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
            {
                LockCursor();
            }
        }

        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            OnCursorLocked?.Invoke();
        }

        private void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            OnCursorUnlocked?.Invoke();
        }

        private void HandleMovement()
        {
            float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? boostMultiplier : 1f);
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");
            float moveY = 0f;

            if (Input.GetKey(KeyCode.E))
            {
                moveY += 1;
            }
            if (Input.GetKey(KeyCode.Q))
            {
                moveY -= 1;
            }
            Vector3 move = transform.right * moveX + transform.up * moveY + transform.forward * moveZ;
            transform.position += move * speed * Time.deltaTime;
        }

        private void HandleMouseLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            rotationY += mouseX;
            rotationX -= mouseY;
            rotationX = Mathf.Clamp(rotationX, -90f, 90f);

            Quaternion fromRotation = transform.rotation;
            Quaternion toRotation = Quaternion.Euler(rotationX, rotationY, transform.rotation.eulerAngles.z);
            transform.rotation = toRotation;

            OnCameraRotation?.Invoke(Quaternion.Inverse(fromRotation) * toRotation);
        }

        private void RefocusCamera()
        {
            transform.position = defaultPosition;
            transform.eulerAngles = defaultRotation;
            rotationX = defaultRotation.x;
            rotationY = defaultRotation.y;
        }
    }
}