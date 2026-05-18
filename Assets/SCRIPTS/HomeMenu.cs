using UnityEngine;
using UnityEngine.UI;

public class HomeMenu : MonoBehaviour
{
    public static HomeMenu Instance { get; private set; }

    [SerializeField] private Button playGame;
    [SerializeField] private Button quitGame;
    [SerializeField] private Button changeCharacter;
    [SerializeField] private GameObject characterPanel;
    [SerializeField] private Button characterPanelExit;
    [SerializeField] private Button[] changeModelsButton;

    private void Start()
    {
        characterPanel.SetActive(false);
    }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        playGame.onClick.AddListener(() =>
        {
            MainLobby.Instance.gameObject.SetActive(true);
            gameObject.SetActive(false);
        });
        quitGame.onClick.AddListener(() =>
        {
            Application.Quit();
        });
        changeCharacter.onClick.AddListener(() =>
        {
            characterPanel.SetActive(true);
            changeCharacter.gameObject.SetActive(false);
        });
        characterPanelExit.onClick.AddListener(() =>
        {
            characterPanel.SetActive(false);
            changeCharacter.gameObject.SetActive(true);
        });
        for (int i = 0; i < changeModelsButton.Length; i++)
        {
            int index = i;
            changeModelsButton[i].onClick.AddListener(() => SwitchCharacter.Instance.SelectCharacter(index));
        }
    }
}
