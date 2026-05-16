using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNameSync : NetworkBehaviour
{
    public TMPro.TextMeshProUGUI nameText;

    private NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>();

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            string savedName = PlayerPrefs.GetString("PlayerName", "Player");
            SetPlayerNameServerRpc(savedName);
        }

        playerName.OnValueChanged += OnPlayerNameChanged;

        nameText.text = playerName.Value.ToString();
    }
    private void OnPlayerNameChanged(FixedString32Bytes oldValue, FixedString32Bytes newValue)
    {
        nameText.text = newValue.ToString();
    }
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void SetPlayerNameServerRpc(string name)
    {
        playerName.Value = name;
    }
}
