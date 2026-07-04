using Unity.Netcode;
using UnityEngine;

public class CharacterAnimation : NetworkBehaviour
{
    public static CharacterAnimation Instance { get; private set; }

    private InputSystem_Actions inputActions;
    public Animator animator;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector3 groundCheckSize = new Vector3(0.2f, 0.05f, 0.2f);
    [SerializeField] private LayerMask groundMask;

    private bool isGrounded = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        inputActions = new InputSystem_Actions();
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
        if (IsOwner)
        {
            SetLayer(gameObject, LayerMask.NameToLayer("LocalPlayer"));
        }
    }
    private void Update()
    {
        if (!IsOwner) return;

        if (animator == null)
            return;

        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();

        float speed = moveInput.y * 2f;
        float strafe = moveInput.x * 2f;
        float currentSpeed;
        float currentStrafe;

        if (speed < 0f)
            strafe = -strafe;

        if (inputActions.Player.Sprint.IsPressed())
        {
            speed = speed / 2;
            strafe = strafe / 2;
        }

        isGrounded = Physics.CheckBox(groundCheck.position, groundCheckSize, Quaternion.identity, groundMask);

        if (inputActions.Player.Jump.WasPressedThisFrame())
            if (isGrounded)
                animator.SetTrigger("JumpTrigger");

        currentSpeed = animator.GetFloat("Move");
        animator.SetFloat("Move", Mathf.Lerp(currentSpeed, speed, Time.deltaTime * 15f));
        currentStrafe = animator.GetFloat("Strafe");
        animator.SetFloat("Strafe", Mathf.Lerp(currentStrafe, strafe, Time.deltaTime * 10f));
        animator.SetFloat("Jump", ((speed + 2f) / 4f));
    }
    private void SetLayer(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            SetLayer(child.gameObject, newLayer);
        }
    }
}
