using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Speeds")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float crouchSpeed = 2.5f;
    
    [Header("Jump & Gravity")]
    public float jumpHeight = 2f;
    public float gravity = -19.62f; // CS-like higher gravity
    
    [Header("Crouch Settings")]
    public float standingHeight = 2f;
    public float crouchingHeight = 1.2f;
    public float crouchTransitionSpeed = 10f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private CharacterController controller;
    private Vector3 velocity;
    public bool isGrounded { get; private set; }
    public bool isCrouching { get; private set; }
    private float currentHeight;

    [Header("References")]
    public Transform cameraHolder; 
    private float defaultCameraY;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        currentHeight = standingHeight;
        
        if (cameraHolder != null)
            defaultCameraY = cameraHolder.localPosition.y;
    }

    void Update()
    {
        HandleGroundCheck();
        HandleMovement();
        HandleCrouch();
        HandleJump();
        ApplyGravity();
    }

    void HandleGroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        bool isRunning = Input.GetKey(KeyCode.LeftShift) && !isCrouching && z > 0;
        float currentSpeed = isCrouching ? crouchSpeed : (isRunning ? runSpeed : walkSpeed);

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * currentSpeed * Time.deltaTime);
    }

    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded && !isCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = true;
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            isCrouching = false;
        }

        float targetHeight = isCrouching ? crouchingHeight : standingHeight;
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * crouchTransitionSpeed);
        
        controller.height = currentHeight;
        // Keep the bottom of the capsule at the pivot point (feet)
        controller.center = new Vector3(0, currentHeight / 2f, 0);
    }

    void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
