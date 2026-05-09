using UnityEngine;

public class HeadCameraAnchor : MonoBehaviour
{
    [Header("Settings")]
    public Transform headBone;    
    public Vector3 positionOffset; // Adjust if needed (e.g. 0, 0, 0.1)
    
    [Header("Rotation Settings")]
    public float mouseSensitivity = 2f;
    public Transform playerBody;

    private float xRotation = 0f;
    private float yRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        if (playerBody != null)
            yRotation = playerBody.eulerAngles.y;
    }

    void LateUpdate()
    {
        if (headBone == null || playerBody == null) return;

        // 1. MOUSE INPUT
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // 2. ROTATE PLAYER BODY (Horizontal Only)
        playerBody.rotation = Quaternion.Euler(0f, yRotation, 0f);

        // 3. POSITION: Snap to Head Bone (This copies the Animation movement)
        transform.position = headBone.position + headBone.TransformDirection(positionOffset);

        // 4. ROTATION: Combine Animation Rotation + Mouse Look
        // This makes the camera "shake/tilt" exactly like the head animation
        Quaternion headRotation = headBone.rotation;
        Quaternion mouseLookRotation = Quaternion.Euler(xRotation, 0f, 0f);
        
        // Combine them: Head movement + Mouse Pitch
        transform.rotation = headRotation * mouseLookRotation;
    }
}
