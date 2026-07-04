using System;
using System.Collections;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public struct PlayerReadyData : INetworkSerializable, IEquatable<PlayerReadyData>
{
    public ulong clientId;
    public FixedString32Bytes playerName;
    public bool isReady;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref isReady);
    }

    public bool Equals(PlayerReadyData other)
    {
        return clientId == other.clientId && playerName.Equals(other.playerName) && isReady == other.isReady;
    }
}
public class PlayerData : NetworkBehaviour
{
    public TMPro.TextMeshProUGUI nameText;
    public NetworkVariable<FixedString64Bytes> playerName = new ();
    public NetworkVariable<int> CurrentPropId = new(-1);
    public NetworkVariable<int> CurrentCharacterId = new(0);
    public NetworkVariable<bool> IsProp = new(false);
    public NetworkVariable<float> Health = new(100f);

    public override void OnNetworkSpawn()
    {
        playerName.OnValueChanged += OnPlayerNameChanged;

        if (IsOwner)
        {
            StartCoroutine(WaitForGameManager());
        }

        nameText.text = playerName.Value.ToString();
    }
    private IEnumerator WaitForGameManager()
    {
        while (!GameManager.Instance.IsInitialized)
            yield return null;

        string savedName = PlayerPrefs.GetString("PlayerName", "Player");
        SetPlayerNameServerRpc(savedName);

    }
    
    private void OnPlayerNameChanged(FixedString64Bytes oldValue, FixedString64Bytes newValue)
    {
        nameText.text = newValue.ToString();
    }
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void SetPlayerNameServerRpc(string name)
    {
        playerName.Value = name;
        Debug.Log("Adding Player Data: " + OwnerClientId + " with Name: " + name + "to the Network List");
        GameManager.Instance.AddPlayerData(OwnerClientId, name);
    }
}
