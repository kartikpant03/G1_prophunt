using GLTFast.Schema;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchCharacter : MonoBehaviour
{
    public static SwitchCharacter Instance { get; private set; }
    [SerializeField] private GameObject[] characterModels;
    [SerializeField] private Transform characterParent;
    public GameObject currentCharacter;
    private int currentCharacterIndex;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        currentCharacterIndex = PlayerPrefs.GetInt("CurrentCharacter", 0);
    }
    private void Start()
    {
        currentCharacter = Instantiate(characterModels[currentCharacterIndex], characterParent);
        CharacterAnimation.Instance.animator = currentCharacter.GetComponent<Animator>();

        if (SceneManager.GetActiveScene().name == "LobbyScene")
            CharacterAnimation.Instance.ActivateLobbyAnimation();
        else
            CharacterAnimation.Instance.ActivateMovementAnimation();
    }
    public void SelectCharacter(int index)
    {
        PlayerPrefs.SetInt("CurrentCharacter", index);
        PlayerPrefs.Save();

        currentCharacterIndex = PlayerPrefs.GetInt("CurrentCharacter", 0);

        Animator oldAnimator = currentCharacter.GetComponent<Animator>();
        AnimatorStateInfo currentState = oldAnimator.GetCurrentAnimatorStateInfo(0);
        float currentTime = currentState.normalizedTime % 1f;
        string stateName = "ArmStrech";

        if (currentCharacter != null)
        {
            Destroy(currentCharacter);
        }
        currentCharacter = Instantiate(characterModels[currentCharacterIndex], characterParent);

        CharacterAnimation.Instance.animator = currentCharacter.GetComponent<Animator>();
        CharacterAnimation.Instance.ActivateLobbyAnimation();
        currentCharacter.GetComponent<Animator>().Play(stateName, 0, currentTime);
    }
}
