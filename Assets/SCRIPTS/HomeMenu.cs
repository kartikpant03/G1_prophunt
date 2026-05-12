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

    [SerializeField] private GameObject[] characterModels;
    [SerializeField] private Button[] changeModelsButton;
    private GameObject currentCharacter;

    private void Start()
    {
        currentCharacter = Instantiate(characterModels[0]);
        characterPanel.SetActive(false);
        CharacterAnimation.Instance.ActivateLobbyAnimation();
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
            changeModelsButton[i].onClick.AddListener(() => SelectCharacter(index));
        }

    }
    private void SelectCharacter(int index)
    {
        Animator oldAnimator = currentCharacter.GetComponent<Animator>();
        AnimatorStateInfo currentState = oldAnimator.GetCurrentAnimatorStateInfo(0);
        float currentTime = currentState.normalizedTime % 1f;
        string stateName = currentState.IsName("ArmStrech") ? "ArmStrech" : currentState.shortNameHash.ToString();

        if (currentCharacter != null)
        {
            Destroy(currentCharacter);
        }
        currentCharacter = Instantiate(characterModels[index]);

        Animator newAnimator = currentCharacter.GetComponent<Animator>();
        newAnimator.Play(stateName, 0, currentTime);
    }
}
