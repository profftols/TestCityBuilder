using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    private PlayerControls playerControls;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction zoomAction;
    private InputAction rotateAction;
    private InputAction deleteAction;
    private InputAction selectBuilding1Action;
    private InputAction selectBuilding2Action;
    private InputAction selectBuilding3Action;

    [Header("Movement Settings")]
    [SerializeField] private float movementSpeed = 10f;
    [SerializeField] private float panBorderThickness = 10f;
    [SerializeField] private Vector2 moveBoundsX = new(-50, 50);
    [SerializeField] private Vector2 moveBoundsZ = new(-50, 50);

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 55f;
    [SerializeField] private float minZoom = 3f;
    [SerializeField] private float maxZoom = 8f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 100f;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private float zoomInput;

    private bool isDragging = false;
    private Vector3 dragStartPos;

    private void Awake()
    {
        playerControls = new PlayerControls();
        
        moveAction = playerControls.Camera.Move;
        lookAction = playerControls.Camera.Look;
        zoomAction = playerControls.Camera.Zoom;

        lookAction.started += OnLookStarted;
        lookAction.performed += OnLookPerformed;
        lookAction.canceled += OnLookCanceled;
        
        zoomAction.performed += OnZoom;
        rotateAction = playerControls.Camera.Rotate;
        deleteAction = playerControls.Camera.Delete;
        selectBuilding1Action = playerControls.Camera.SelectBuilding1;
        selectBuilding2Action = playerControls.Camera.SelectBuilding2;
        selectBuilding3Action = playerControls.Camera.SelectBuilding3;

        rotateAction.performed += OnRotate;
        deleteAction.performed += OnDelete;
        selectBuilding1Action.performed += OnSelectBuilding1;
        selectBuilding2Action.performed += OnSelectBuilding2;
        selectBuilding3Action.performed += OnSelectBuilding3;
    }

    private void OnEnable()
    {
        playerControls.Camera.Enable();
    }

    private void OnDisable()
    {
        playerControls.Camera.Disable();
    }

    private void OnLookStarted(InputAction.CallbackContext context)
    {
        if (Mouse.current.rightButton.isPressed)
        {
            isDragging = true;
            dragStartPos = Mouse.current.position.ReadValue();
        }
    }

    private void OnLookPerformed(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    private void OnLookCanceled(InputAction.CallbackContext context)
    {
        isDragging = false;
    }

    private void OnZoom(InputAction.CallbackContext context)
    {
        zoomInput = context.ReadValue<float>();
    }

    private void Update()
    {
        HandleMovement();
        HandleZoom();
        HandleLook();
    }

    private void HandleMovement()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector3 direction = new Vector3(input.x, 0, input.y).normalized;
        transform.Translate(direction * movementSpeed * Time.deltaTime, Space.World);

        Vector3 mousePosition = Mouse.current.position.ReadValue();
        if (mousePosition.x < panBorderThickness)
        {
            transform.Translate(Vector3.left * movementSpeed * Time.deltaTime, Space.World);
        }
        if (mousePosition.x > Screen.width - panBorderThickness)
        {
            transform.Translate(Vector3.right * movementSpeed * Time.deltaTime, Space.World);
        }
        if (mousePosition.y < panBorderThickness)
        {
            transform.Translate(Vector3.back * movementSpeed * Time.deltaTime, Space.World);
        }
        if (mousePosition.y > Screen.height - panBorderThickness)
        {
            transform.Translate(Vector3.forward * movementSpeed * Time.deltaTime, Space.World);
        }
        
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, moveBoundsX.x, moveBoundsX.y);
        clampedPosition.z = Mathf.Clamp(clampedPosition.z, moveBoundsZ.x, moveBoundsZ.y);
        transform.position = clampedPosition;
    }

    private void HandleZoom()
    {
        float scroll = zoomAction.ReadValue<float>() * 0.1f;
    
        if (scroll != 0)
        {
            Vector3 zoomDirection = transform.forward;
            transform.Translate(zoomDirection * scroll * zoomSpeed * Time.deltaTime, Space.World);

            Vector3 clampedPosition = transform.position;
            clampedPosition.y = Mathf.Clamp(clampedPosition.y, minZoom, maxZoom);
            transform.position = clampedPosition;
        }
    }

    private void HandleLook()
    {
        if (isDragging)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            transform.Rotate(Vector3.up, delta.x * rotationSpeed * Time.deltaTime, Space.World);
        }
    }
    
    private void OnRotate(InputAction.CallbackContext context)
    {
        Debug.Log("Клавиша R нажата! Вращаем объект...");
        // Здесь должна быть логика вращения выбранного объекта
    }

    private void OnDelete(InputAction.CallbackContext context)
    {
        Debug.Log("Клавиша Del нажата! Удаляем объект...");
        // Здесь должна быть логика удаления выбранного объекта
    }

    private void OnSelectBuilding1(InputAction.CallbackContext context)
    {
        Debug.Log("Выбрано здание 1.");
        // Здесь должна быть логика выбора префаба здания 1
    }

    private void OnSelectBuilding2(InputAction.CallbackContext context)
    {
        Debug.Log("Выбрано здание 2.");
        // Здесь должна быть логика выбора префаба здания 2
    }

    private void OnSelectBuilding3(InputAction.CallbackContext context)
    {
        Debug.Log("Выбрано здание 3.");
        // Здесь должна быть логика выбора префаба здания 3
    }
}

