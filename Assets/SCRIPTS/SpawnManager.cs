using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set;  }
    
    [SerializeField] private SpawnPoint[] spawnPoints;
    
    private void Awake()
    {
        Instance = this;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public Transform GetSpawnPoint(int index)
    {
        return spawnPoints[index % spawnPoints.Length].transform;
    }
}
