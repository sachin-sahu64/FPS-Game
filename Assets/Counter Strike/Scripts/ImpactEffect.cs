using UnityEngine;

public class ImpactEffect : MonoBehaviour
{
    public float destroyAfter = 2f;

    void Start()
    {
        // Automatically destroy the impact effect (sparks/bullet holes) after some time
        Destroy(gameObject, destroyAfter);
    }
}
