using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public PlayerMovement movement;
    public WeaponController weaponController;

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
        
        bool isRunning = Input.GetKey(KeyCode.LeftShift) && vertical > 0.1f && !isGrounded == false && !isCrouching;
        
        // 0 = Idle, 1 = Walk, 2 = Run
        float targetForward = vertical * (isRunning ? 2f : 1f);

        // Update Animator Parameters
        animator.SetFloat("Forward", targetForward, dampTime, Time.deltaTime);
        animator.SetFloat("Strafe", horizontal, dampTime, Time.deltaTime);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsCrouching", isCrouching);

        // Weapon Layer Logic (Optional: Adjust based on if weapon is equipped)
        if (weaponController != null && weaponController.activeWeapon != null)
        {
            animator.SetLayerWeight(1, 1f); // Assuming Layer 1 is for Weapons
        }
        else
        {
            animator.SetLayerWeight(1, 0f);
        }

        // Jump Trigger
        if (Input.GetButtonDown("Jump") && isGrounded && !isCrouching)
        {
            animator.SetTrigger("Jump");
        }
    }
}
