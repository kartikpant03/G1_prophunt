using UnityEngine;

public class PropDatabase : MonoBehaviour
{
    public static PropDatabase Instance;
    public PropObject[] props;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}
