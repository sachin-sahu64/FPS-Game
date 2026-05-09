using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public PlayerMovement movement;

    [Header("Settings")]
    public float dampTime = 0.1f;

    void Update()
    {
        if (animator == null || movement == null) return;

        // Input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        // State from Movement Script
        bool isGrounded = movement.isGrounded;
        bool isCrouching = movement.isCrouching;
        
        bool isRunning = Input.GetKey(KeyCode.LeftShift) && vertical > 0.1f && !isCrouching;
        
        // 0 = Idle, 1 = Walk, 2 = Run
        float targetForward = vertical * (isRunning ? 2f : 1f);

        // Update Animator Parameters
        animator.SetFloat("Forward", targetForward, dampTime, Time.deltaTime);
        animator.SetFloat("Strafe", horizontal, dampTime, Time.deltaTime);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsCrouching", isCrouching);

        // Jump Trigger
        if (Input.GetButtonDown("Jump") && isGrounded && !isCrouching)
        {
            animator.SetTrigger("Jump");
        }
    }
}
