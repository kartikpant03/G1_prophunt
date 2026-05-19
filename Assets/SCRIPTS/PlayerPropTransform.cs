using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PlayerPropTransform : NetworkBehaviour
{
    private InputSystem_Actions inputActions;

    [SerializeField] private Transform propParent;
    [SerializeField] private GameObject characterModel;
    [SerializeField] private CinemachineCamera playerFPPCamera;
    [SerializeField] private CinemachineCamera playerTPPCamera;

    private GameObject propModel;

    private void Awake()
    {
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
            inputActions.Enable();
    }
    private void Update()
    {
        if (!IsOwner) return;

        if (inputActions.Player.Interact.WasPressedThisFrame())
        {
            Ray propRay = new (Camera.main.transform.position, Camera.main.transform.forward);
            if (Physics.Raycast(propRay, out RaycastHit propHit, 5f))
            {
                if (propHit.collider.gameObject.layer == LayerMask.NameToLayer("Prop"))
                {
                    TransformToProp(propHit.collider.gameObject);
                }
            }
        }
    }
    private void TransformToProp(GameObject prop)
    {
        if (prop == null) return;

        characterModel.SetActive(false);
        if (propModel != null)
        {
            Destroy(propModel);
        }

    }
    public void EnableFPPCamera(bool value)
    {
        playerFPPCamera.enabled = value;
    }
    public void EnableTPPCamera(bool value)
    {
        playerTPPCamera.enabled = value;
    }
}
