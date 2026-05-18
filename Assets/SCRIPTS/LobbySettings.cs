using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class LobbySettings : MonoBehaviour
{
    public static LobbySettings Instance { get; private set; }
    private const string Relay_JoinCode_Key = "RelayJoinCode";
    [SerializeField] private MultiplayerMovement movementScript;

    public event EventHandler CreateLobbyFailed;
    public event EventHandler QuickJoinLobbyFailed;
    public event EventHandler JoinLobbyCodeFailed;
    public event EventHandler LeaveLobbyFailed;

    public event EventHandler<LobbyListChangedEventArgs> LobbyListChanged;
    public class LobbyListChangedEventArgs : EventArgs
    {
        public List<Lobby> lobbyList;
    }

    private float heartBeatTimer;
    private float lobbyListTimer;
    private Lobby joinedLobby;

    private void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        heartBeatTimer = 1f;
        lobbyListTimer = 1f;
        EventSystem.current.SetSelectedGameObject(null);
    }
    private void Awake()
    {
        if (Instance != null && Instance != this)  // FOR CLEAN UP:
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Update()
    {
        StartHeartBeat();
        UpdateLobbyList();
    }
    public Lobby GetLobby()
    {
        return joinedLobby;
    }
    public bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }
    public void StartHeartBeat()
    {
        if (IsLobbyHost())
        {
            heartBeatTimer -= Time.deltaTime;
            if (heartBeatTimer <= 0f)
            {
                float heartBeatMaxTime = 15f;
                heartBeatTimer = heartBeatMaxTime;

                LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }
    }
    public void UpdateLobbyList()
    {
        if (joinedLobby == null && AuthenticationService.Instance.IsSignedIn && SceneManager.GetActiveScene().name == SceneLoader.Scene.LobbyScene.ToString())
        {
            lobbyListTimer -= Time.deltaTime;
            if (lobbyListTimer <= 0f)
            {
                float lobbyListMaxTime = 2f;
                lobbyListTimer = lobbyListMaxTime;
                ListLobbies();
            }
        }
    }
    private async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions()
            {
                Filters = new List<QueryFilter>
            {
                new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
            }
            };
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);

            LobbyListChanged?.Invoke(this, new LobbyListChangedEventArgs
            {
                lobbyList = queryResponse.Results
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    private async Task<Allocation> AllocateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(CreateLobby.Instance.maxPlayers - 1);
            return allocation;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            return default;
        }
    }
    private async Task<string> GetRelayJoinCode(Allocation allocation)
    {
        try
        {
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            return relayJoinCode;
        }
        catch (RelayServiceException e) 
        {
            Debug.Log(e);
            return default;
        }
    }
    private async Task<JoinAllocation> JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            return joinAllocation;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            return default;
        }
    }
    public async void CreateNewLobby(string lobbyName, bool isPrivate)
    {
        try
        {
            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, CreateLobby.Instance.maxPlayers, new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
            });

            Allocation allocation = await AllocateRelay(); 
            string relayJoinCode = await GetRelayJoinCode(allocation);
            await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { Relay_JoinCode_Key, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode)}
                }
            });

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));

            StartHost();

        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            CreateLobbyFailed?.Invoke(this, EventArgs.Empty);
        }
    }
    public async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);

            string relayJoinCode = joinedLobby.Data[Relay_JoinCode_Key].Value;
            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode); 

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(joinAllocation, "dtls"));

            StartClient();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            JoinLobbyCodeFailed?.Invoke(this, EventArgs.Empty);
        }
    }
    public async void JoinLobbyByID(string lobbyID)
    {
        try
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyID);

            string relayJoinCode = joinedLobby.Data[Relay_JoinCode_Key].Value;
            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(joinAllocation, "dtls"));

            StartClient();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            JoinLobbyCodeFailed?.Invoke(this, EventArgs.Empty);
        }
    }
    public async void QuickJoinLobby()
    {
        try
        {
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();

            string relayJoinCode = joinedLobby.Data[Relay_JoinCode_Key].Value;
            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(joinAllocation, "dtls"));

            StartClient();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            QuickJoinLobbyFailed?.Invoke(this, EventArgs.Empty);
        }
    }
    public async void LeaveCurrentLobby()
    {
        try
        {
            if (IsLobbyHost())
            {
                StopHost();
                await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
            }
            else
            {
                StopClient();
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            }
            SceneLoader.SceneLoaderCallback();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            LeaveLobbyFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    private async void StartHost()
    {
        await VivoxChat.Instance.JoinVivoxRoom(joinedLobby.Name);
        VivoxChat.Instance.currentRoomName = "Lobby";

        NetworkManager.Singleton.StartHost();
        SceneLoader.LoadNetwork(SceneLoader.Scene.MultiplayerScene);    
    }
    private async void StopHost()
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.Shutdown();

            SceneLoader.Load(SceneLoader.Scene.LobbyScene);

            await VivoxChat.Instance.LeaveVivoxRoom(joinedLobby.Name);
            VivoxChat.Instance.currentRoomName = "Lobby";
        }
    }
    private async void StartClient()
    {
        VivoxChat.Instance.currentRoomName = joinedLobby.Name;
        await VivoxChat.Instance.JoinVivoxRoom(joinedLobby.Name);

        NetworkManager.Singleton.StartClient();
    }
    public async void StopClient()
    {
        if (NetworkManager.Singleton == null)
            return;

        await VivoxChat.Instance.LeaveVivoxRoom(joinedLobby.Name);
        VivoxChat.Instance.currentRoomName = "Lobby";

        if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
    private async void OnClientDisconnected(ulong clientId)
    {
        if (!IsLobbyHost())
        {
            SceneLoader.Load(SceneLoader.Scene.LobbyScene);

            await VivoxChat.Instance.LeaveVivoxRoom(VivoxChat.Instance.currentRoomName);
            VivoxChat.Instance.currentRoomName = "Lobby";

            SceneLoader.SceneLoaderCallback();
        }
    }
}
