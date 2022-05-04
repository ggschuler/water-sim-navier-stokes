using UnityEngine;

public class Particle : MonoBehaviour
{
    public Vector3 position;
    public Vector3 velocity;
    public Vector3 force;
    public float density;
    public float pressure;

    public GameObject go; // associated GameObject from where position accessed;
}
