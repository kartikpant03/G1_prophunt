using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

public class CharacterAnimation : NetworkBehaviour
{
    public static CharacterAnimation Instance { get; private set; }
    public Animator animator;

    public void ActivateLobbyAnimation()
    {
        animator.SetLayerWeight(0, 0f);
        animator.SetLayerWeight(1, 1f);
        animator.SetLayerWeight(2, 0f);
    }
    public void ActivateMovementAnimation()
    {
        animator.SetLayerWeight(0, 0f);
        animator.SetLayerWeight(1, 0f);
        animator.SetLayerWeight(2, 1f);
    }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    private void Start()
    {
        ActivateLobbyAnimation();

        if (IsOwner)
        {
            SetLayer(gameObject, LayerMask.NameToLayer("LocalPlayer"));

        }
    }
    private void Update()
    {
        if (!IsOwner) return;
        if (animator.GetLayerWeight(2) == 0f) return;

        float speed = 0f;
        float strafe = 0f;
        float currentSpeed;
        float currentStrafe;

        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D))
            strafe = 0f;

        else if (Input.GetKey(KeyCode.A))
            strafe = Input.GetKey(KeyCode.LeftShift) ? -2f : -1f;

        else if (Input.GetKey(KeyCode.D))
            strafe = Input.GetKey(KeyCode.LeftShift) ? 2f : 1f;

        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.S))
            speed = 0f;

        else if (Input.GetKey(KeyCode.W))
            speed = Input.GetKey(KeyCode.LeftShift) ? 2f : 1f;

        else if (Input.GetKey(KeyCode.S))
        {
            speed = Input.GetKey(KeyCode.LeftShift) ? -2f : -1f;

            if (Input.GetKey(KeyCode.A))
                strafe = Input.GetKey(KeyCode.LeftShift) ? 2f : 1f;

            if (Input.GetKey(KeyCode.D))
                strafe = Input.GetKey(KeyCode.LeftShift) ? -2f : -1f;
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
