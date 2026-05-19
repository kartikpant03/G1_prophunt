using Unity.AppUI.UI;
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
    [SerializeField] private float xSensitivity = 100f;
    [SerializeField] private float ySensitivity = 100f;

    [Header("Movement Settings")]
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float walkSpeed = 2f;

    private MultiplayerMovement movementScript;
    private CharacterController controller;

    private const float stickDistance = 2f;
    private const float offsetAboveGround = 1.5f;
    private float xRotation = 0f;
    private float yRotation = 0f;
    private bool isCursorLocked = true;

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
    private void OnEnable()
    {
        inputActions.Enable();
    }
    private void OnDisable()
    {
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
    }
    private void Update()
    {
        if (IsOwner)
        {
            if (isCursorLocked)
            {
                moveInput = inputActions.Player.Move.ReadValue<Vector2>();
                lookInput = inputActions.Player.Look.ReadValue<Vector2>();

                PlayerMovement();       // (currently client sided)
                CameraMovement();
            }

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
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, stickDistance))
            {
                if (hit.collider.CompareTag("Ground"))
                {
                    Vector3 pos = transform.position;
                    pos.y = hit.point.y + offsetAboveGround;
                    transform.position = pos;
                }
            }
        }
    }
    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isCursorLocked = true;
    }
    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isCursorLocked = false;
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
