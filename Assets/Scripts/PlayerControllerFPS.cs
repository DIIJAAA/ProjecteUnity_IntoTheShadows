using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private float gravity = 9.8f;
    
    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Light playerLight;
    [SerializeField] private Light fillLight;
    
    private CharacterController characterController;
    private float verticalRotation = 0f;
    private float verticalVelocity = 0f;
    private bool cursorLocked = true;
    
    void Start()
    {
        InitializeController();
        ConfigureLights();
        LockCursor();
    }

    private void InitializeController()
    {
        characterController = GetComponent<CharacterController>();
        
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main?.transform;
            if (cameraTransform == null)
            {
                Debug.LogError("[PlayerController] No s'ha trobat la càmera!");
            }
        }
    }

    private void ConfigureLights()
    {
        // Llum principal (Spot)
        if (playerLight != null)
        {
            playerLight.type = LightType.Spot;
            playerLight.intensity = 4f;
            playerLight.range = 18f;
            playerLight.spotAngle = 100f;
            playerLight.color = new Color(1f, 0.96f, 0.9f);
            playerLight.shadows = LightShadows.Soft;
            playerLight.shadowStrength = 0.5f;
            playerLight.shadowBias = 0.02f;
            playerLight.shadowNormalBias = 0.2f;
        }
        
        // Llum de reompliment (Point)
        if (fillLight != null)
        {
            fillLight.type = LightType.Point;
            fillLight.intensity = 0.8f;
            fillLight.range = 8f;
            fillLight.color = new Color(0.29f, 0.29f, 0.35f);
            fillLight.shadows = LightShadows.None;
        }
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cursorLocked = true;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        cursorLocked = false;
    }
    
    void Update()
    {
        HandleInput();
        HandleMovement();
        HandleMouseLook();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (cursorLocked)
                UnlockCursor();
            else
                LockCursor();
        }
    }
    
    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        Vector3 direction = transform.right * horizontal + transform.forward * vertical;
        Vector3 movement = direction * moveSpeed;
        
        // Gravity
        if (characterController.isGrounded)
        {
            verticalVelocity = -0.5f;
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }
        
        movement.y = verticalVelocity;
        characterController.Move(movement * Time.deltaTime);
    }
    
    private void HandleMouseLook()
    {
        if (!cursorLocked) return;

        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;
        
        // Rotació horitzontal
        transform.Rotate(Vector3.up * mouseX);
        
        // Rotació vertical (limitada)
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        
        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }
    }
}