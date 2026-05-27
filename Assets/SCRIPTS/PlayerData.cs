using System;
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

    public static PlayerData Instance { get; private set; }
    public override void OnNetworkSpawn()
    {
        playerName.OnValueChanged += OnPlayerNameChanged;

        if (IsOwner)
        {
            string savedName = PlayerPrefs.GetString("PlayerName", "Player");
            SetPlayerNameServerRpc(savedName);
        }

        nameText.text = playerName.Value.ToString();
    }
    private void Update()
    {
        if (!IsOwner) return;
    }
    private void OnPlayerNameChanged(FixedString64Bytes oldValue, FixedString64Bytes newValue)
    {
        nameText.text = newValue.ToString();
    }
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void SetPlayerNameServerRpc(string name)
    {
        playerName.Value = name;
        GameManager.Instance.UpdatePlayerName(OwnerClientId, name);
    }
}
