using UnityEngine;

public class PropObject : MonoBehaviour
{
    public int propID;
    public float controllerHeight;
    public float controllerWidth;
    public Vector3 controllerCentre;

    private void Awake()
    {
        controllerHeight = GetComponent<Collider>().bounds.size.y;
        controllerWidth = GetComponent<Collider>().bounds.size.x;
        controllerCentre = GetComponent<Collider>().bounds.center - transform.position;
    }
}

