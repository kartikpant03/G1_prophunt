using UnityEngine;

public class SwitchCharacter : MonoBehaviour
{
    public static SwitchCharacter Instance { get; private set; }
    [SerializeField] private GameObject[] characterModels;
    [SerializeField] private Transform characterParent;
    [SerializeField] private GameObject currentCharacter;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    public void SelectCharacter(int index)
    {
        Animator oldAnimator = currentCharacter.GetComponent<Animator>();
        AnimatorStateInfo currentState = oldAnimator.GetCurrentAnimatorStateInfo(0);
        float currentTime = currentState.normalizedTime % 1f;
        string stateName = currentState.IsName("ArmStrech") ? "ArmStrech" : currentState.shortNameHash.ToString();

        if (currentCharacter != null)
        {
            Destroy(currentCharacter);
        }
        currentCharacter = Instantiate(characterModels[index], characterParent);
        currentCharacter.transform.localPosition = Vector3.zero;
        currentCharacter.transform.localRotation = Quaternion.identity;

        Animator newAnimator = currentCharacter.GetComponent<Animator>();
        newAnimator.Play(stateName, 0, currentTime);
    }
}
