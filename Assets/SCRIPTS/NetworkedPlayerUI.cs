using UnityEngine;
using Unity.Netcode;

public class NetworkedPlayerUI : MonoBehaviour
{
    private Camera localCamera;

    private void Start()
    {
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
            return;

        localCamera = Camera.main; 
    }
    private void Update()
    {
        if (localCamera == null) 
            return;

        transform.rotation = Quaternion.LookRotation(transform.position - localCamera.transform.position);
    }
}
