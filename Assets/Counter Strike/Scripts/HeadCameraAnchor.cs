using UnityEngine;

public class HeadCameraAnchor : MonoBehaviour
{
    [Header("Settings")]
    public Transform headBone;    
    public Vector3 positionOffset; // Adjust if needed (e.g. 0, 0, 0.1)
    
    [Header("Rotation Settings")]
    public Transform playerBody;
    public float mouseSensitivity = 100f;
    public float rotationSmoothing = 20f;
    [Range(0, 1)] 
    public float headInfluence = 0.5f; 

    [Header("Sway Settings")]
    public float swayAmount = 0.02f;
    public float swaySpeed = 2f;
    public float swaySmooth = 5f;
    private Vector3 swayOffset;

    private float xRotation = 0f;
    private float yRotation = 0f;
    private float currentXRotation = 0f;
    private float currentYRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        if (playerBody != null)
        {
            yRotation = playerBody.eulerAngles.y;
            currentYRotation = yRotation;
        }
    }

    void LateUpdate()
    {
        if (headBone == null || playerBody == null) return;

        // 1. MOUSE INPUT
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Smoothing the rotation
        currentXRotation = Mathf.Lerp(currentXRotation, xRotation, Time.deltaTime * rotationSmoothing);
        currentYRotation = Mathf.Lerp(currentYRotation, yRotation, Time.deltaTime * rotationSmoothing);

        // 2. PROCEDURAL SWAY (Breathing/Movement)
        float speedFactor = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).magnitude;
        float currentSwaySpeed = speedFactor > 0.1f ? swaySpeed * 1.5f : swaySpeed;
        float currentSwayAmount = speedFactor > 0.1f ? swayAmount * 1.2f : swayAmount;

        float swX = Mathf.Sin(Time.time * currentSwaySpeed) * currentSwayAmount;
        float swY = Mathf.Sin(Time.time * currentSwaySpeed * 0.5f) * currentSwayAmount;
        Vector3 targetSway = new Vector3(swX, swY, 0);
        swayOffset = Vector3.Lerp(swayOffset, targetSway, Time.deltaTime * swaySmooth);

        // 3. APPLY POSITION & ROTATION
        playerBody.rotation = Quaternion.Euler(0f, currentYRotation, 0f);

        // Position: Head + Manual Offset + Sway
        transform.position = headBone.position + headBone.TransformDirection(positionOffset) + playerBody.TransformDirection(swayOffset);

        // Rotation: Blend Animation + Mouse Look
        Quaternion mouseLookRotation = Quaternion.Euler(currentXRotation, currentYRotation, 0f);
        Quaternion animatedRotation = Quaternion.Slerp(mouseLookRotation, headBone.rotation * Quaternion.Euler(xRotation, 0, 0), headInfluence);
        
        transform.rotation = animatedRotation;
    }
}
