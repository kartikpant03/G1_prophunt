using System;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [NonSerialized] public NetworkVariable<MatchState> currentState = new(MatchState.Warmup);
    private NetworkVariable<float> countdownTimer = new(10f);
    public NetworkList<PlayerReadyData> playerReadyList;
    public bool IsInitialized;

    public enum MatchState
    {
        Warmup,
        Countdown,
        InGame,
        Finished
    }

    private bool countdownStarted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        IsInitialized = false;
        playerReadyList = new NetworkList<PlayerReadyData>();
    }
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Debug.Log("Player List Initialized");
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            IsInitialized = true;
        }
    }
    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }
    private void Update()
    {
        if (!IsHost) return;

        if (currentState.Value == MatchState.Countdown)
        {
            countdownTimer.Value -= Time.deltaTime;
            if (countdownTimer.Value <= 0)
                StartMatch();
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        for (int i = 0; i < playerReadyList.Count; i++) 
            if (playerReadyList[i].clientId == clientId)
            {
                playerReadyList.RemoveAt(i);
                break;
            }
    }
    public void AddPlayerData(ulong clientId, string newName)
    {
        foreach (var player in playerReadyList)
            if (player.clientId == clientId)
                return;

        PlayerReadyData data = new ()
        {
            playerName = newName,
            clientId = clientId,
            isReady = false
        };
        Debug.Log($"AddPlayerData called. IsSpawned={IsSpawned}");
        playerReadyList.Add(data);
        Debug.Log($"AddPlayerData called. IsSpawned={IsSpawned}");
    }
    [Rpc(SendTo.Server)] public void ToggleReadyServerRpc(RpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;

        if (currentState.Value == MatchState.Countdown && countdownTimer.Value <= 3f)
            return;

        for (int i = 0; i < playerReadyList.Count; i++)
            if (playerReadyList[i].clientId == senderId)
            {
                PlayerReadyData data = playerReadyList[i];
                data.isReady = !data.isReady;
                playerReadyList[i] = data;

                break;
            }
        
        CheckAllPlayersReady();
    }
    private void CheckAllPlayersReady()
    {
        foreach (var player in playerReadyList)
            if (player.isReady == false)
            {
                StopCountdown();
                return;
            } 

        StartCountdown();    
    }
    private void StartCountdown()
    {
        if (countdownStarted) return;

        countdownStarted = true;
        currentState.Value = MatchState.Countdown;
        countdownTimer.Value = 10f;
    }
    private void StopCountdown()
    {
        if (!countdownStarted) return;

        countdownStarted = false;
        currentState.Value = MatchState.Warmup;
        countdownTimer.Value = 10f;
    }
    private void StartMatch()
    {
        currentState.Value = MatchState.InGame;
        Debug.Log("MATCH STARTED");

        AssignHunters();
        AssignHiders();
        
    }
    private void AssignHunters()
    {
        
    }
    private void AssignHiders()
    {
        
    }
    public void ResetLobby()
    {
        playerReadyList.Clear();

        currentState.Value = MatchState.Warmup;
        countdownTimer.Value = 10f;

        countdownStarted = false;
    }
    public MatchState GetMatchState()
    {
        return currentState.Value;
    }
    public float GetCountdown()
    {
        return countdownTimer.Value;
    }
}
