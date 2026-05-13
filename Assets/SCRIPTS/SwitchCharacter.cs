using GLTFast.Schema;
using UnityEngine;

public class SwitchCharacter : MonoBehaviour
{
    public static SwitchCharacter Instance { get; private set; }
    [SerializeField] private GameObject[] characterModels;
    [SerializeField] private Transform characterParent;
    public GameObject currentCharacter;


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
        currentCharacter = Instantiate(characterModels[0], characterParent);
        CharacterAnimation.Instance.animator = currentCharacter.GetComponent<Animator>();
    }
    public void SelectCharacter(int index)
    {
        Animator oldAnimator = currentCharacter.GetComponent<Animator>();
        AnimatorStateInfo currentState = oldAnimator.GetCurrentAnimatorStateInfo(0);
        float currentTime = currentState.normalizedTime % 1f;
        string stateName = "ArmStrech";

        if (currentCharacter != null)
        {
            Destroy(currentCharacter);
        }
        currentCharacter = Instantiate(characterModels[index], characterParent);

        CharacterAnimation.Instance.animator = currentCharacter.GetComponent<Animator>();
        CharacterAnimation.Instance.ActivateLobbyAnimation();
        currentCharacter.GetComponent<Animator>().Play(stateName, 1, currentTime);
    }
}
