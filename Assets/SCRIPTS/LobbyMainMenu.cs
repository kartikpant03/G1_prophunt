using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;

public class MainLobby : MonoBehaviour
{
    public static MainLobby Instance { get; private set; }

    [SerializeField] private Button createLobby;
    [SerializeField] private Button quickJoin;
    [SerializeField] private Button joinCode;
    [SerializeField] private TMP_InputField inputCode;
    [SerializeField] private Button exitMainMenu;
    public TMP_InputField clientName;
    [SerializeField] private Transform lobbyContainer;
    [SerializeField] private Transform lobbyTemplate;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        gameObject.SetActive(false);

        createLobby.onClick.AddListener(() =>
        {
            CreateLobby.Instance.gameObject.SetActive(true);
            gameObject.SetActive(false);
        });
        quickJoin.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            SceneLoader.Load(SceneLoader.Scene.MultiplayerScene);
            LobbySettings.Instance.QuickJoinLobby();
        });
        joinCode.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            SceneLoader.Load(SceneLoader.Scene.MultiplayerScene);
            LobbySettings.Instance.JoinLobbyByCode(inputCode.text);
        });
        exitMainMenu.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            HomeMenu.Instance.gameObject.SetActive(true);
        });
    }
    private void Start()
    {
        LobbySettings.Instance.LobbyListChanged += LobbySettings_LobbyListChanged;
        UpdateLobbyList(new List<Lobby>());
        lobbyTemplate.gameObject.SetActive(false);
    }
    private void LobbySettings_LobbyListChanged(object sender, LobbySettings.LobbyListChangedEventArgs e)
    {
        UpdateLobbyList(e.lobbyList);
    }
    private void UpdateLobbyList(List<Lobby> lobbyList)
    {
        foreach (Transform child in lobbyContainer)
        {
            if (child == lobbyTemplate) continue;
            Destroy(child.gameObject);
        }
        foreach (Lobby lobby in lobbyList)
        {
            Transform lobbyTransform = Instantiate(lobbyTemplate, lobbyContainer);
            lobbyTransform.gameObject.SetActive(true);
            lobbyTransform.GetComponent<LobbyListJoin>().SetLobby(lobby);
        }
    }
    private void OnDestroy()
    {
        LobbySettings.Instance.LobbyListChanged -= LobbySettings_LobbyListChanged;
    }
}
