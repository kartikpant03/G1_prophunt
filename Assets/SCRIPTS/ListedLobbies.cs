using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListJoin : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI joinLobbyName;

    private Lobby lobby;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            SceneLoader.Load(SceneLoader.Scene.MultiplayerScene);
            LobbySettings.Instance.JoinLobbyByID(lobby.Id);
            MainLobby.Instance.gameObject.SetActive(false);
        });
    }
    public void SetLobby(Lobby lobby)
    {
        this.lobby = lobby;
        joinLobbyName.text = lobby.Name;
    }
}
