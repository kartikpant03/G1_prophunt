using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class JoinedPlayerList : MonoBehaviour
{
    public static JoinedPlayerList Instance { get; private set; }

    [SerializeField] private GameObject playerGameUI;
    [SerializeField] private TMP_Text warmupText;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private Button readyButton;
    [SerializeField] private Transform readyStatusParent;
    [SerializeField] private Transform playersNameParent;
    [SerializeField] private GameObject readyStatusTemplate;
    [SerializeField] private GameObject playerNameTemplate;

    private bool isReady;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void Start()
    {
        readyButton.onClick.AddListener(UpdateReadyButton);
        GameManager.Instance.playerReadyList.OnListChanged += OnReadyListChanged;
        isReady = false;
        UpdateReadyButton();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.playerReadyList.OnListChanged -= OnReadyListChanged;
    }
    private void Update()
    {
        UpdateCountdown();
        if (Keyboard.current.fKey.wasPressedThisFrame && GameManager.Instance.GetCountdown() >= 3f && (
            GameManager.Instance.GetMatchState() == GameManager.MatchState.Warmup ||
            GameManager.Instance.GetMatchState() == GameManager.MatchState.Countdown))
        {
            GameManager.Instance.ToggleReadyServerRpc();
            isReady = !isReady;
            UpdateReadyButton();
        }
    }
    private void OnReadyListChanged(NetworkListEvent<PlayerReadyData> changeEvent)
    {
        UpdatePlayerReadyListUI();
    }

    private void UpdateCountdown()
    {
        countdownText.text = Mathf.CeilToInt(GameManager.Instance.GetCountdown()).ToString();
    }

    private void UpdateReadyButton()
    {
        TMP_Text readyButtonText = readyButton.gameObject.GetComponentInChildren<TMP_Text>();
        if (isReady)
        {
            readyButtonText.text = "READY";
            var colors = GetComponentInChildren<Button>().colors;
            colors.normalColor = Color.green;
            GetComponentInChildren<Button>().colors = colors;
        }
        else
        {
            readyButtonText.text = "NOT READY";
            var colors = GetComponentInChildren<Button>().colors;
            colors.normalColor = Color.yellow;
            GetComponentInChildren<Button>().colors = colors;
        }
    }

    public void UpdatePlayerReadyListUI()
    {
        foreach (Transform child in playersNameParent)
            Destroy(child.gameObject);
        foreach (Transform child in readyStatusParent)
            Destroy(child.gameObject);

        foreach (var player in GameManager.Instance.playerReadyList)
        {
            GameObject playerName = Instantiate(playerNameTemplate, playersNameParent);
            TMP_Text playerNameText = playerName.GetComponent<TMP_Text>();
            playerNameText.text = player.playerName.ToString();

            GameObject readyStatus = Instantiate(readyStatusTemplate, readyStatusParent);
            TMP_Text readyStatusText = readyStatus.GetComponent<TMP_Text>();
            readyStatusText.text = player.isReady ? "READY" : "NOT READY";
            readyStatusText.color = player.isReady ? Color.green : Color.red;
        }
    }
}