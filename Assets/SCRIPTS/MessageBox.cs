using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageBox : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button close;

    private void Start()
    {
        LobbySettings.Instance.CreateLobbyFailed += LobbySettings_CreateLobbyFailed;
        LobbySettings.Instance.QuickJoinLobbyFailed += LobbySettings_QuickJoinLobbyFailed;
        LobbySettings.Instance.JoinLobbyCodeFailed += LobbySettings_JoinLobbyCodeFailed;
        LobbySettings.Instance.LeaveLobbyFailed += LobbySettings_LeaveLobbyFailed;
        VivoxChat.Instance.LeftVivoxRoom += VivoxChat_LeftVivoxRoom;

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        LobbySettings.Instance.CreateLobbyFailed -= LobbySettings_CreateLobbyFailed;
        LobbySettings.Instance.QuickJoinLobbyFailed -= LobbySettings_QuickJoinLobbyFailed;
        LobbySettings.Instance.JoinLobbyCodeFailed -= LobbySettings_JoinLobbyCodeFailed;
        LobbySettings.Instance.LeaveLobbyFailed -= LobbySettings_LeaveLobbyFailed;
        VivoxChat.Instance.LeftVivoxRoom -= VivoxChat_LeftVivoxRoom;
    }
    private void Awake()
    {
        close.onClick.AddListener(() =>
        {
            messageText.text = "";
            gameObject.SetActive(false);
        });
    }
    private void DisplayMessage(string message)
    {
        gameObject.SetActive(true);
        messageText.text = message;
    }
    private void VivoxChat_LeftVivoxRoom(object sender, System.EventArgs e)
    {
        DisplayMessage("Left Vivox Room : " + VivoxChat.Instance.currentRoomName);
    }
    private void LobbySettings_CreateLobbyFailed(object sender, System.EventArgs e) 
    {
        DisplayMessage("Failed to Create Lobby.");
    }
    private void LobbySettings_QuickJoinLobbyFailed(object sender, System.EventArgs e)
    {
        DisplayMessage("No Lobby Found.");
    }
    private void LobbySettings_JoinLobbyCodeFailed(object sender, System.EventArgs e)
    {
        DisplayMessage("No Lobby Found.");
    }
    private void LobbySettings_LeaveLobbyFailed(object sender, System.EventArgs e)
    {
        DisplayMessage("Failed to Leave Lobby.");
    }
}
