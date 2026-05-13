using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateLobby : MonoBehaviour
{
    public static CreateLobby Instance { get; private set; }

    [SerializeField] private Button createPublic;
    [SerializeField] private Button createPrivate;
    [SerializeField] private Button exitLobbyButton;
    [SerializeField] private TMP_InputField lobbyName;
    [SerializeField] private TMP_InputField maxPeople;

    public int maxPlayers;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        gameObject.SetActive(false);

        exitLobbyButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            MainLobby.Instance.gameObject.SetActive(true);
        });
        createPublic.onClick.AddListener(() =>
        {
            maxPlayers = int.Parse(maxPeople.text);
            SceneLoader.Load(SceneLoader.Scene.MultiplayerScene);
            LobbySettings.Instance.CreateNewLobby(lobbyName.text, false);
            gameObject.SetActive(false);
        });
        createPrivate.onClick.AddListener(() =>
        {
            maxPlayers = int.Parse(maxPeople.text);
            SceneLoader.Load(SceneLoader.Scene.MultiplayerScene);
            LobbySettings.Instance.CreateNewLobby(lobbyName.text, true);
            gameObject.SetActive(false);
        });
    }
}
