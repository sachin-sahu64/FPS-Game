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
    public float headInfluence = 0.5f; // 1 = full animation shake, 0 = no shake

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

        // 1. MOUSE INPUT (With Time.deltaTime for consistency)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Smoothing the rotation
        currentXRotation = Mathf.Lerp(currentXRotation, xRotation, Time.deltaTime * rotationSmoothing);
        currentYRotation = Mathf.Lerp(currentYRotation, yRotation, Time.deltaTime * rotationSmoothing);

        // 2. ROTATE PLAYER BODY (Horizontal Only)
        playerBody.rotation = Quaternion.Euler(0f, currentYRotation, 0f);

        // 3. POSITION: Snap to Head Bone
        transform.position = headBone.position + headBone.TransformDirection(positionOffset);

        // 4. ROTATION: Blend Animation Rotation + Mouse Look
        Quaternion mouseLookRotation = Quaternion.Euler(currentXRotation, currentYRotation, 0f);
        
        // If headInfluence is 1, it copies 100% of head tilt. 
        // If 0, it only uses mouse look.
        Quaternion animatedRotation = Quaternion.Slerp(mouseLookRotation, headBone.rotation * Quaternion.Euler(xRotation, 0, 0), headInfluence);
        
        transform.rotation = animatedRotation;
    }
}
