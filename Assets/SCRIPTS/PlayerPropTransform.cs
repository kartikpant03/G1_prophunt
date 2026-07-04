using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPropTransform : NetworkBehaviour
{
    private InputSystem_Actions inputActions;
    private PlayerData playerData;

    [SerializeField] private GameObject player;
    [SerializeField] private Transform propParent;
    [SerializeField] private GameObject characterModel;
    [SerializeField] private CinemachineCamera playerFPPCamera;
    [SerializeField] private CinemachineCamera playerTPPCamera;
    [SerializeField] private Camera mainCamera;

    private GameObject propModel;
    private CharacterController playerController;

    private float defaultHeight;
    private float defaultRadius;
    private Vector3 defaultCenter;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }
    public override void OnNetworkSpawn()
    {
        playerData = player.GetComponent<PlayerData>();

        playerData.CurrentPropId.OnValueChanged += OnPropChanged;
        playerData.IsProp.OnValueChanged += OnPropStateChanged;

        if (IsOwner)
        {
            inputActions.Enable();
            playerController = player.GetComponent<CharacterController>();
            EnableFPPCamera(true);
        }

        defaultHeight = playerController.height;
        defaultRadius = playerController.radius;
        defaultCenter = playerController.center;
    }
    public override void OnNetworkDespawn() 
    {
        if (IsOwner)
            inputActions.Disable();

        playerData.CurrentPropId.OnValueChanged -= OnPropChanged;
        playerData.IsProp.OnValueChanged -= OnPropStateChanged;
    }
    private void Update()
    {
        if (!IsOwner) return;

        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            Debug.Log("B key pressed");
            TransformToCharacterServerRpc();
        }

        if (inputActions.Player.Interact.WasPressedThisFrame())
        {
            Debug.Log("E key pressed");
            Ray propRay = new (mainCamera.transform.position, mainCamera.transform.forward);
            if (Physics.Raycast(propRay, out RaycastHit propHit, 5f))
            {
                GameObject newProp = propHit.collider.gameObject;
                if (newProp.layer == LayerMask.NameToLayer("Props"))
                {
                    Debug.Log("Prop hit: " + newProp.name);
                    PropObject propDetail = propHit.collider.gameObject.GetComponent<PropObject>();

                    TransformToPropServerRpc(propDetail.propID);
                    EnableTPPCamera(true);
                    UpdateCharacterController(propDetail);
                }
            }
        }
        if (Keyboard.current.vKey.wasPressedThisFrame)
        {
            if (playerFPPCamera.enabled)
                EnableTPPCamera(true);
            else
                EnableFPPCamera(true);
        }
    }
    [Rpc(SendTo.Server)] private void TransformToPropServerRpc(int propId)
    {
        playerData.CurrentPropId.Value = propId;
        playerData.IsProp.Value = true;
    }
    [Rpc(SendTo.Server)] private void TransformToCharacterServerRpc()
    {
        playerData.IsProp.Value = false;
    }
    private void OnPropChanged(int oldValue, int newValue)
    {
        if (newValue == -1)
            return;

        if (propModel != null)
            Destroy(propModel);

        PropObject prop = PropDatabase.Instance.props[newValue];
        propModel = Instantiate(prop.propPrefab, propParent);
        UpdateCharacterController(prop);
    }
    private void OnPropStateChanged(bool oldValue, bool newValue)
    {
        if (newValue)
            TransformCharacterToProp();
        else
            TransformPropToCharacter();
    }
    private void TransformPropToCharacter()
    {
        characterModel.SetActive(true);
        propParent.gameObject.SetActive(false);

        if (propModel != null)
        {
            Destroy(propModel);
            propModel = null;
        }

        playerController.height = defaultHeight;
        playerController.radius = defaultRadius;
        playerController.center = defaultCenter;
    }
    private void TransformCharacterToProp()
    {
        characterModel.SetActive(false);
        propParent.gameObject.SetActive(true);
    }
    private void UpdateCharacterController(PropObject propDetails)
    {
        playerController.height = propDetails.controllerHeight;
        playerController.radius = propDetails.controllerWidth;
        playerController.center = propDetails.controllerCentre;
    }
    public void EnableFPPCamera(bool value)
    {
        playerFPPCamera.enabled = value;
        playerTPPCamera.enabled = !value;
        foreach (GameObject child in characterModel.transform)
        {
            child.layer = LayerMask.NameToLayer("FPPplayer");
        }
    }
    public void EnableTPPCamera(bool value)
    {
        playerTPPCamera.enabled = value;
        playerFPPCamera.enabled = !value;
        foreach (GameObject child in characterModel.transform)
        {
            child.layer = LayerMask.NameToLayer("TPPplayer");
        }
    }
}
