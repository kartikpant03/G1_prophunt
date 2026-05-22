using Unity.Netcode;
using UnityEngine;

public class CharacterAnimation : NetworkBehaviour
{
    public static CharacterAnimation Instance { get; private set; }

    private InputSystem_Actions inputActions;
    public Animator animator;

    public void ActivateLobbyAnimation()
    {
        animator.SetLayerWeight(0, 1f);
        animator.SetLayerWeight(1, 0f);
    }
    public void ActivateMovementAnimation()
    {
        animator.SetLayerWeight(0, 0f);
        animator.SetLayerWeight(1, 1f);
    }
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

        currentSpeed = animator.GetFloat("Move");
        animator.SetFloat("Move", Mathf.Lerp(currentSpeed, speed, Time.deltaTime * 20f));
        currentStrafe = animator.GetFloat("Strafe");
        animator.SetFloat("Strafe", Mathf.Lerp(currentStrafe, strafe, Time.deltaTime * 15f));
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
