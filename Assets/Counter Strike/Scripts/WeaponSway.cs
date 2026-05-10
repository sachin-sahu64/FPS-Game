using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    [Header("Sway Settings")]
    public float amount = 0.02f;
    public float maxAmount = 0.06f;
    public float smoothAmount = 6f;

    [Header("Rotation Sway")]
    public float rotationAmount = 4f;
    public float maxRotationAmount = 5f;
    public float smoothRotation = 12f;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Start()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

    void Update()
    {
        float mouseX = -Input.GetAxis("Mouse X") * amount;
        float mouseY = -Input.GetAxis("Mouse Y") * amount;
        mouseX = Mathf.Clamp(mouseX, -maxAmount, maxAmount);
        mouseY = Mathf.Clamp(mouseY, -maxAmount, maxAmount);

        // Position Sway
        Vector3 targetPosition = new Vector3(mouseX, mouseY, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition + initialPosition, Time.deltaTime * smoothAmount);

        // Rotation Sway (Tilt)
        float tiltX = -Input.GetAxis("Mouse Y") * rotationAmount;
        float tiltY = -Input.GetAxis("Mouse X") * rotationAmount;
        tiltX = Mathf.Clamp(tiltX, -maxRotationAmount, maxRotationAmount);
        tiltY = Mathf.Clamp(tiltY, -maxRotationAmount, maxRotationAmount);

        Quaternion targetRotation = Quaternion.Euler(new Vector3(tiltX, tiltY, tiltY));
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation * initialRotation, Time.deltaTime * smoothRotation);
    }
}
