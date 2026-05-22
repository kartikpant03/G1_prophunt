using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPropTransform : NetworkBehaviour
{
    private InputSystem_Actions inputActions;

    [SerializeField] private GameObject player;
    [SerializeField] private Transform propParent;
    [SerializeField] private GameObject characterModel;
    [SerializeField] private CinemachineCamera playerFPPCamera;
    [SerializeField] private CinemachineCamera playerTPPCamera;
    [SerializeField] private Camera mainCamera;

    private GameObject propModel;
    private CharacterController playerController;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            inputActions.Enable();
            playerController = player.GetComponent<CharacterController>();
            EnableFPPCamera(true);
        }
    }
    public override void OnNetworkDespawn() 
    {
        if (IsOwner)
            inputActions.Disable();
    }
    private void Update()
    {
        if (!IsOwner) return;

        if (inputActions.Player.Interact.WasPressedThisFrame())
        {
            Debug.Log("E key pressed");
            Ray propRay = new (mainCamera.transform.position, mainCamera.transform.forward);
            if (Physics.Raycast(propRay, out RaycastHit propHit, 5f))
            {
                GameObject newProp = propHit.collider.gameObject;
                if (newProp.layer == LayerMask.NameToLayer("Prop"))
                {
                    if (characterModel.activeInHierarchy)
                    {
                        UpdateProp(newProp);
                        TransformCharacterToProp();
                        EnableTPPCamera(true);
                    }
                    else
                    {
                        UpdateProp(newProp);
                        EnableTPPCamera(true);
                    }
                        
                    PropObject propDetail = propHit.collider.gameObject.GetComponent<PropObject>();
                    UpdateCharacterController(propDetail);
                }
            }
        }
        if (Keyboard.current.vKey.wasPressedThisFrame)
        {
            if (playerFPPCamera.enabled)
            {
                EnableTPPCamera(true);
            }
            else
            {
                EnableFPPCamera(true);
            }
        }
    }
    private void UpdateCharacterController(PropObject propDetails)
    {
        playerController.height = propDetails.controllerHeight;
        playerController.radius = propDetails.controllerWidth;
        playerController.center = propDetails.controllerCentre;
    }
    private void UpdateProp(GameObject prop)
    {
        if (prop == null) return;

        characterModel.SetActive(false);
        if (propModel != null)
        {
            Destroy(propModel);
        }

        propModel = Instantiate(prop, propParent);
    }
    private void TransformPropToCharacter()
    {
        characterModel.SetActive(true);
        propParent.gameObject.SetActive(false);
    }
    private void TransformCharacterToProp()
    {
        characterModel.SetActive(false);
        propParent.gameObject.SetActive(true);
    }
    public void EnableFPPCamera(bool value)
    {
        playerFPPCamera.enabled = value;
        playerTPPCamera.enabled = !value;
    }
    public void EnableTPPCamera(bool value)
    {
        playerTPPCamera.enabled = value;
        playerFPPCamera.enabled = !value;
    }
}
