using GLTFast.Schema;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchCharacter : MonoBehaviour
{
    public static SwitchCharacter Instance { get; private set; }

    [SerializeField] private GameObject[] characterModels;
    [SerializeField] private Transform characterParent;
    [SerializeField] private Animator animator;

    public GameObject characterModel;

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
        characterModel = Instantiate(characterModels[PlayerPrefs.GetInt("CurrentCharacter", 0)], characterParent);
        CharacterAnimation.Instance.animator = characterModel.GetComponent<Animator>();
        animator = characterModel.GetComponent<Animator>();

        if (SceneManager.GetActiveScene().name == "LobbyScene")
            ActivateLobbyAnimation();
        else
            ActivateMovementAnimation();
    }
    public void SelectCharacter(int index)
    {
        PlayerPrefs.SetInt("CurrentCharacter", index);
        PlayerPrefs.Save();

        Animator oldAnimator = animator;
        AnimatorStateInfo currentState = oldAnimator.GetCurrentAnimatorStateInfo(0);
        float currentTime = currentState.normalizedTime % 1f;

        if (characterModel != null)
        {
            Destroy(characterModel);
        }
        characterModel = Instantiate(characterModels[PlayerPrefs.GetInt("CurrentCharacter", 0)], characterParent);

        CharacterAnimation.Instance.animator = characterModel.GetComponent<Animator>();
        animator = characterModel.GetComponent<Animator>();
        ActivateLobbyAnimation();
        characterModel.GetComponent<Animator>().Play("ArmStrech", 0, currentTime);
    }

    private void ActivateLobbyAnimation()
    {
        animator.SetLayerWeight(0, 1f);
        animator.SetLayerWeight(1, 0f);
    }
    private void ActivateMovementAnimation()
    {
        animator.SetLayerWeight(0, 0f);
        animator.SetLayerWeight(1, 1f);
    }
}
