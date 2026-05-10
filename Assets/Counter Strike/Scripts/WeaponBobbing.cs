using UnityEngine;

public class WeaponBobbing : MonoBehaviour
{
    [Header("Bobbing Settings")]
    public float walkBobSpeed = 10f;
    public float walkBobAmount = 0.05f;
    public float runBobSpeed = 14f;
    public float runBobAmount = 0.1f;
    public float idleBobSpeed = 2f;
    public float idleBobAmount = 0.01f;

    [Header("Jump & Land Settings")]
    public float jumpOffset = 0.1f;
    public float landOffset = -0.15f;
    public float offsetSmoothness = 5f;

    [Header("Tilt Settings")]
    public float sideTilt = 2f;
    public float tiltSmoothness = 5f;

    private PlayerMovement playerMovement;
    private Vector3 initialLocalPos;
    private Quaternion initialLocalRot;
    private float timer;
    private float currentOffset;

    void Start()
    {
        playerMovement = GetComponentInParent<PlayerMovement>();
        initialLocalPos = transform.localPosition;
        initialLocalRot = transform.localRotation;
    }

    void Update()
    {
        if (playerMovement == null) return;

        HandleBobbing();
        HandleJumpLand();
    }

    void HandleBobbing()
    {
        float speed = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).magnitude;
        float currentBobSpeed = idleBobSpeed;
        float currentBobAmount = idleBobAmount;

        if (playerMovement.isGrounded && speed > 0.1f)
        {
            // Check if running (Left Shift)
            bool isRunning = Input.GetKey(KeyCode.LeftShift) && Input.GetAxis("Vertical") > 0;
            currentBobSpeed = isRunning ? runBobSpeed : walkBobSpeed;
            currentBobAmount = isRunning ? runBobAmount : walkBobAmount;
        }

        timer += Time.deltaTime * currentBobSpeed;

        // Calculate Bob Position (Sine wave)
        float xOffset = Mathf.Cos(timer) * currentBobAmount;
        float yOffset = Mathf.Sin(timer * 2) * currentBobAmount; // Y moves twice as fast for a 'figure 8' or natural bounce

        Vector3 targetBobPos = initialLocalPos + new Vector3(xOffset, yOffset + currentOffset, 0);
        
        // Apply Side Tilt based on horizontal movement
        float horizontal = Input.GetAxis("Horizontal");
        Quaternion targetTilt = Quaternion.Euler(initialLocalRot.eulerAngles.x, initialLocalRot.eulerAngles.y, -horizontal * sideTilt);

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetBobPos, Time.deltaTime * 8f);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetTilt, Time.deltaTime * tiltSmoothness);
    }

    void HandleJumpLand()
    {
        // Procedural Jump/Land Offset
        float targetOffset = 0;

        if (!playerMovement.isGrounded)
        {
            targetOffset = jumpOffset; // Weapon moves up when jumping
        }
        else
        {
            // Simple landing impact logic could be added here, 
            // but for now we'll just return to 0
            targetOffset = 0;
        }

        currentOffset = Mathf.Lerp(currentOffset, targetOffset, Time.deltaTime * offsetSmoothness);
    }
}
