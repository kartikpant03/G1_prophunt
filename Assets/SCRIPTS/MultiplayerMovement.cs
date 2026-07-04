using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MultiplayerMovement : NetworkBehaviour
{
    [Header("Player References")]
    [SerializeField] private GameObject player;
    [SerializeField] private CinemachineCamera playerFPPCamera;
    [SerializeField] private CinemachineCamera playerTPPCamera;

    [Header("Sensitivity Settings")]
    [SerializeField] private float xSensitivity = 10f;
    [SerializeField] private float ySensitivity = 10f;

    [Header("Movement Settings")]
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float walkSpeed = 2f;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector3 groundCheckSize = new Vector3(0.2f, 0.05f, 0.2f);
    [SerializeField] private LayerMask groundMask;

    private MultiplayerMovement movementScript;
    private CharacterController controller;

    private float xRotation = 0f;
    private float yRotation = 0f;
    private bool isCursorLocked = true;
    private float gravity = -9.81f;
    private float verticalVelocity = 0f;
    private bool isGrounded = false;

    private CinemachineInputAxisController camController;
    private InputSystem_Actions inputActions;
    private Vector2 moveInput;
    private Vector2 lookInput;
   
    private void Awake()
    {
        movementScript = GetComponent<MultiplayerMovement>();
        inputActions = new InputSystem_Actions();

        if (SceneManager.GetActiveScene().name == "LobbyScene")
            movementScript.enabled = false;
        else
            movementScript.enabled = true;
    }
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
            inputActions.Enable();
    }
    public override void OnNetworkDespawn()
    {
        if (IsOwner)
            inputActions.Disable();
    }
    private void Start()
    {
        if (!IsOwner)
        {
            if (SceneManager.GetActiveScene().name != "LobbyScene")
            {
                playerFPPCamera.enabled = false;
                playerTPPCamera.enabled = false;
                GetComponentInChildren<AudioListener>().enabled = false;
                return;
            }
        }
        controller = GetComponent<CharacterController>();
        camController = playerTPPCamera.GetComponent<CinemachineInputAxisController>();
        Debug.Log((int)NetworkManager.Singleton.LocalClientId);
        camController.PlayerIndex = (int)NetworkManager.Singleton.LocalClientId;
        SetSensitivity();

        foreach (var controller in camController.Controllers)
            if (controller.Name == "Look Orbit Y")
                controller.Input.InputAction = InputActionReference.Create(inputActions.Player.Look); 
    }
    private void Update()
    {
        if (IsOwner)
        {
            if (isGrounded && verticalVelocity <= 0f)
                 verticalVelocity = -2f;

            if (isCursorLocked)
            {
                moveInput = inputActions.Player.Move.ReadValue<Vector2>();
                lookInput = inputActions.Player.Look.ReadValue<Vector2>();

                PlayerMovement();       // (currently client sided)
                CameraMovement();

                isGrounded = Physics.CheckBox(groundCheck.position, groundCheckSize, Quaternion.identity, groundMask);

                if (inputActions.Player.Jump.WasPressedThisFrame())
                    if (isGrounded)
                        verticalVelocity = Mathf.Sqrt(-3f * gravity);
            }
            
            controller.Move(verticalVelocity * Vector2.up * Time.deltaTime);
            verticalVelocity += gravity * Time.deltaTime;

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (isCursorLocked)
                {
                    UnlockCursor();
                    JoinedLobby.Instance.gameObject.SetActive(true);
                }
                else
                {
                    LockCursor();
                    JoinedLobby.Instance.gameObject.SetActive(false);
                }
            }
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.matrix = Matrix4x4.TRS(groundCheck.position, groundCheck.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, groundCheckSize * 2f);
    }
    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isCursorLocked = true;
        inputActions.Enable();
    }
    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isCursorLocked = false;
        inputActions.Disable();
    }
    public void SetSensitivity()
    {
        foreach (var controller in camController.Controllers)
            if (controller.Name == "Look Orbit Y")
                controller.Input.Gain = -ySensitivity;
    }
    private void CameraMovement()
    {
        float mouseX = lookInput.x * xSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * ySensitivity * Time.deltaTime;

        xRotation -= mouseY;
        yRotation += mouseX;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerFPPCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        player.transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
    }
    private void PlayerMovement()       // Local movement of player
    {
        Vector3 moveDirection = (transform.forward * moveInput.y + transform.right * moveInput.x).normalized;
        moveDirection.y = 0f;
        Vector3 velocity;

        velocity = moveDirection * (inputActions.Player.Sprint.IsPressed() ? walkSpeed : runSpeed);
        controller.Move(velocity * Time.deltaTime);
    }
    private void MovePlayerServerAuth()  // Server Movement of Player
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        MovePlayerServerRpc(moveX, moveZ);
    }
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void MovePlayerServerRpc(float moveX, float moveZ)
    {
        Vector3 moveDirection = (Camera.main.transform.forward * moveZ + Camera.main.transform.right * moveX).normalized;
        moveDirection.y = 0f;
        Vector3 velocity;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            velocity = moveDirection * runSpeed;
        }
        else
        {
            velocity = moveDirection * walkSpeed;
        }
    }
}
