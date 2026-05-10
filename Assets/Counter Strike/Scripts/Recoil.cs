using UnityEngine;

public class Recoil : MonoBehaviour
{
    // Rotations
    private Vector3 currentRotation;
    private Vector3 targetRotation;

    [Header("Settings")]
    public float snappiness = 10f;
    public float returnSpeed = 5f;

    void Update()
    {
        // Smoothly return to original rotation
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.fixedDeltaTime);
        
        // Apply rotation to the object
        transform.localRotation = Quaternion.Euler(currentRotation);
    }

    public void RecoilFire(Vector3 recoil)
    {
        // Add random variation to horizontal recoil
        float randomX = recoil.x;
        float randomY = Random.Range(-recoil.y, recoil.y);
        float randomZ = Random.Range(-recoil.z, recoil.z);

        targetRotation += new Vector3(randomX, randomY, randomZ);
    }
}
