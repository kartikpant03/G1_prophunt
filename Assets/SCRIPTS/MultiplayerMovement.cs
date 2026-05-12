using UnityEngine;
using Unity.Netcode;

public class MultiplayerMovement : NetworkBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject playerModel;
    [SerializeField] private GameObject playerCamera;
    private CharacterController controller;
    private const float stickDistance = 2f;
    private const float offsetAboveGround = 1.5f;

    [Header("Sensitivity Settings")]
    [SerializeField] private float xSensitivity = 100f;
    [SerializeField] private float ySensitivity = 100f;

    [Header("Movement Settings")]
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float walkSpeed = 2f;

    private float xRotation = 0f;
    private float yRotation = 0f;
    private bool isCursorLocked = true;

    private void Start()
    {
        if (!IsOwner)
        {
            playerCamera.SetActive(false);
            GetComponentInChildren<AudioListener>().enabled = false;
            return;
        }
        controller = GetComponent<CharacterController>();
        LockCursor();
    }

    private void Update()
    {
        if (IsOwner)
        {
            if (isCursorLocked)
            {
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
                    MovePlayer();       // Movement of Player (currently client sided not from server)
                CameraMove();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
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
    private void CameraMove()
    {
        float mouseX = Input.GetAxis("Mouse X") * xSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * ySensitivity * Time.deltaTime;

        xRotation -= mouseY;
        yRotation += mouseX;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        player.transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
    }
    private void MovePlayer()       // Local movement of player
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 moveDirection = (transform.forward * moveZ + transform.right * moveX).normalized;
        moveDirection.y = 0f;
        Vector3 velocity;

        velocity = moveDirection * (Input.GetKey(KeyCode.LeftShift) ? walkSpeed : runSpeed);
        controller.Move(velocity * Time.deltaTime);
    }
    private void MovePlayerServerAuth()  // Server Movement of Player
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        MovePlayerServerRpc(moveX, moveZ);
    }
    [ServerRpc(RequireOwnership = false)]
    private void MovePlayerServerRpc(float moveX, float moveZ)
    {
        Vector3 moveDirection = (playerCamera.transform.forward * moveZ + playerCamera.transform.right * moveX).normalized;
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
