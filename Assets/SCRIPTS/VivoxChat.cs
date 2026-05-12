using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Vivox;
using System.Threading.Tasks;
using System;

public class VivoxChat : MonoBehaviour
{
    public static VivoxChat Instance { get; private set; }

    public string currentRoomName;
    public event EventHandler LeftVivoxRoom;
    private bool isMuted;

    [SerializeField] private GameObject speakingMic;
    [SerializeField] private GameObject mutedMic;

    private async void Start()
    {
        speakingMic.SetActive(false);
        mutedMic.SetActive(false);
        await StartUnityAuthenticationServices();
        await StartVivoxAuthenticationServices();
    }
    private void Awake()
    {
        if (Instance != null && Instance != this)  // FOR CLEAN UP:
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        isMuted = false;
    }
    
    private void Update()
    {
        if (VivoxService.Instance.IsLoggedIn && VivoxService.Instance.ActiveChannels.ContainsKey(currentRoomName))
        {
            if (Input.GetKeyDown(KeyCode.T) && isMuted == false)
            {
                speakingMic.SetActive(true);
                VivoxService.Instance.SetChannelTransmissionModeAsync(TransmissionMode.Single, currentRoomName);
            }

            if (Input.GetKeyUp(KeyCode.T) && isMuted == false)
            {
                speakingMic.SetActive(false);
                VivoxService.Instance.SetChannelTransmissionModeAsync(TransmissionMode.None);
            }
        }
    }
    private async Task StartUnityAuthenticationServices()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
            await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        Debug.Log("Logged in as - " + AuthenticationService.Instance.PlayerId);
    }
    private async Task StartVivoxAuthenticationServices()
    {
        await VivoxService.Instance.InitializeAsync();
        var loginOptions = new LoginOptions
        {
            DisplayName = AuthenticationService.Instance.PlayerId
        };
        await VivoxService.Instance.LoginAsync(loginOptions);

        Debug.Log("Started Vivox Services.");
    }
    public async Task JoinVivoxRoom(string RoomName)
    {
        await VivoxService.Instance.JoinEchoChannelAsync(RoomName, ChatCapability.AudioOnly);
        await VivoxService.Instance.SetChannelTransmissionModeAsync(TransmissionMode.None);

        Debug.Log("Joined voice channel successfully. " + RoomName);
    }
    public async Task LeaveVivoxRoom(string RoomName)
    {
        LeftVivoxRoom?.Invoke(this, EventArgs.Empty);
        await VivoxService.Instance.LeaveChannelAsync(RoomName);

        Debug.Log("Left voice channel successfully. " + RoomName);
    }
    public void MuteLocalMic()
    {
        if (!isMuted)
        {
            VivoxService.Instance.MuteInputDevice();
            mutedMic.SetActive(true);
            isMuted = true;
        }
        else
        {
            VivoxService.Instance.UnmuteInputDevice();
            mutedMic.SetActive(false);
            isMuted = false;
        }
    }
    public async void DisableVivoxServices()
    {
        if (VivoxService.Instance != null && VivoxService.Instance.IsLoggedIn)
        {
            await VivoxService.Instance.LeaveAllChannelsAsync();
            await VivoxService.Instance.LogoutAsync();

            Debug.Log("Logged out of Vivox on destroy.");
        }
    }

    // Entering Voice Channel on Collision Going IN.

    /*
    private void OnTriggerEnter(Collider Body)
    {
        if (gameObject.CompareTag("Player") && gameObject.GetComponent<NetworkObject>().IsOwner)
        {
            JoinVivoxRoom(Body.gameObject.name);
            CurrentRoomName = Body.gameObject.name;
        }
    }
    // Leaving Voice Channel on Collision Going OUT.
    private void OnTriggerExit(Collider Body)
    {
        if (gameObject.CompareTag("Player") && gameObject.GetComponent<NetworkObject>().IsOwner)
        {
            LeaveVivoxRoom(Body.gameObject.name);
            CurrentRoomName = "";
        }
    }
    */
}
