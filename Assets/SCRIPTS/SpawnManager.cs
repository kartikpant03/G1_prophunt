using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set;  }
    
    [SerializeField] private SpawnPoint[] spawnPoints;
    
    private void Awake()
    {
        Instance = this;
    }
    public Transform GetSpawnPoint(int index)
    {
        return spawnPoints[index % spawnPoints.Length].transform;
    }
}
