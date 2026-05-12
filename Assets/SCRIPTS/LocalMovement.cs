using UnityEngine;
public class LocalMovement : MonoBehaviour
{
    public GameObject Player;
    public GameObject Camera;
    public AudioSource micAudioSource;
    public AudioSource speakerAudioSource;
    private AudioClip recordedClip;
    private const float stickDistance = 1.1f;
    private const float offsetAboveGround = 1f;


    [Header("Sensitivity Settings")]
    public float XSensitivity = 100f;
    public float YSensitivity = 100f;

    [Header("Movement Settings")]
    public float runSpeed = 5f;
    public float walkSpeed = 3f;

    private float xRotation = 0f;
    private float yRotation = 0f;
    private bool isCursorLocked = true;
    private string micDevice;
    private bool isRecording = false;

    private void Start()
    {
        LockCursor();
        if (Microphone.devices.Length > 0)
        {
            micDevice = Microphone.devices[0];
            Debug.Log("Microphone found: " + micDevice);
        }
        else
        {
            Debug.LogWarning("No microphone detected!");
        }
    }

    private void Update()
    {
        if (isCursorLocked)
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
                MovePlayer();
            CameraMove();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isCursorLocked)
                UnlockCursor();
            else
                LockCursor();
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
    private void StartRecording()
    {
        if (micDevice != null)
        {
            recordedClip = Microphone.Start(micDevice, false, 10, 44100);
            isRecording = true;
            Debug.Log("Recording started...");
        }
    }

    private void StopRecording()
    {
        if (isRecording)
        {
            Microphone.End(micDevice);
            isRecording = false;
            Debug.Log("Recording stopped.");
        }
    }

    private void PlayRecording()
    {
        if (recordedClip != null)
        {
            speakerAudioSource.clip = recordedClip;
            speakerAudioSource.Play();
            Debug.Log("Playing recording...");
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
        float mouseX = Input.GetAxis("Mouse X") * XSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * YSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        yRotation += mouseX;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        Camera.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }
    private void MovePlayer()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 moveDirection = (transform.forward * moveZ + transform.right * moveX);
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
        Player.transform.Translate(velocity * Time.deltaTime);
    }
}
