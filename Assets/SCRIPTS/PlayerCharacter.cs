using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerCharacter : NetworkBehaviour
{
    [SerializeField] private GameObject[] characterPrefabs;
    [SerializeField] private Transform characterParent;
    [SerializeField] private CharacterAnimation characterAnimation;
    private PlayerData playerData;

    private GameObject currentCharacter;
    

    public override void OnNetworkSpawn()
    {
        playerData = GetComponentInParent<PlayerData>();

        playerData.CurrentCharacterId.OnValueChanged += OnCharacterChanged;

        if (playerData.CurrentCharacterId.Value != -1)
        {
            OnCharacterChanged(-1, playerData.CurrentCharacterId.Value);
        }

        if (IsOwner)
        {
            int selected = PlayerPrefs.GetInt("CurrentCharacter", 0);
            SetCharacterServerRpc(selected);
        }
    }
    public override void OnNetworkDespawn()
    {
        playerData.CurrentCharacterId.OnValueChanged -= OnCharacterChanged;
    }

    [Rpc(SendTo.Server)] private void SetCharacterServerRpc(int index)
    {
        playerData.CurrentCharacterId.Value = index;
    }

    private void OnCharacterChanged(int oldValue, int newValue)
    {
        if (currentCharacter != null)
            Destroy(currentCharacter);

        currentCharacter = Instantiate(characterPrefabs[newValue], characterParent);
        characterAnimation.animator = currentCharacter.GetComponent<Animator>();
    }
}
