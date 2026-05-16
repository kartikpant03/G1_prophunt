using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class JoinedLobby : MonoBehaviour
{
    public static JoinedLobby Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI lobbyName;
    [SerializeField] private TextMeshProUGUI lobbyCode;
    [SerializeField] private Button leaveLobby;

    [SerializeField] private TMP_Dropdown inputDropdown;
    [SerializeField] private TMP_Dropdown outputDropdown;


    private List<VivoxInputDevice> inputOptions;
    private List<VivoxOutputDevice> outputOptions;

    private Lobby joinedLobby;
    private bool menuEnabled;

    private void Awake()
    {
        if (Instance != null && Instance != this)  // FOR CLEAN UP:
        {
            Destroy(gameObject);
            return;
        }
        menuEnabled = false;


        Instance = this;
        gameObject.SetActive(false);

        leaveLobby.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);

            LobbySettings.Instance.LeaveCurrentLobby();
            MainLobby.Instance.gameObject.SetActive(true);
        });

        inputDropdown.onValueChanged.AddListener(async (int index) =>
        {
            VivoxInputDevice selectedAudioInputDevice = inputOptions[index];
            await VivoxService.Instance.SetActiveInputDeviceAsync(selectedAudioInputDevice);
        });
            
        outputDropdown.onValueChanged.AddListener(async (int index) =>
        {
            VivoxOutputDevice selectedAudioOutputDevice = outputOptions[index];
            await VivoxService.Instance.SetActiveOutputDeviceAsync(selectedAudioOutputDevice);
        });
    }
    private void Update()
    {
        if (Keyboard.current.escapeKey.isPressed)
        {
            menuEnabled = !menuEnabled;
            if (menuEnabled)
            {
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
        if (Keyboard.current.mKey.isPressed && VivoxChat.Instance != null)
        {
            VivoxChat.Instance.MuteLocalMic();
        }   
    }
    private void OnEnable()
    {
        if (VivoxService.Instance.IsLoggedIn) 
        {
            VivoxService.Instance.AvailableInputDevicesChanged += UpdateInputMicOptions;
            VivoxService.Instance.AvailableOutputDevicesChanged += UpdateOutputSoundOptions;

            UpdateInputMicOptions();
            UpdateOutputSoundOptions();
        }

        joinedLobby = LobbySettings.Instance.GetLobby();
        if (joinedLobby != null)
        {
            leaveLobby.gameObject.SetActive(true);
            lobbyName.text = "Name : " + joinedLobby.Name;
            lobbyCode.text = "Code : " + joinedLobby.LobbyCode;
        }
        else
        {
            leaveLobby.gameObject.SetActive(false);
        }
    }
    private void UpdateInputMicOptions()
    {
        inputOptions = new List<VivoxInputDevice>(VivoxService.Instance.AvailableInputDevices);
        inputDropdown.ClearOptions();
        inputDropdown.AddOptions(inputOptions.ConvertAll(device => device.DeviceName));
    }
    private void UpdateOutputSoundOptions()
    {
        outputOptions = new List<VivoxOutputDevice>(VivoxService.Instance.AvailableOutputDevices);
        outputDropdown.ClearOptions();
        outputDropdown.AddOptions(outputOptions.ConvertAll(device => device.DeviceName));
    }
}
