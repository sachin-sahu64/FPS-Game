using UnityEngine;

namespace FPSGame.Movement
{
    [CreateAssetMenu(menuName = "FPS Game/Movement/Player Movement Settings", fileName = "PlayerMovementSettings")]
    public class PlayerMovementSettings : ScriptableObject
    {
        [Header("Ground Movement")]
        [SerializeField] private float walkSpeed = 3.5f;
        [SerializeField] private float runSpeed = 5.75f;
        [SerializeField] private float crouchSpeed = 2f;
        [SerializeField] private float acceleration = 18f;
        [SerializeField] private float groundFriction = 10f;

        [Header("Air Movement")]
        [SerializeField] private float airAcceleration = 5f;
        [SerializeField] private float gravity = -25f;
        [SerializeField] private float jumpHeight = 1.2f;

        [Header("Stance")]
        [SerializeField] private float standingHeight = 1.8f;
        [SerializeField] private float crouchedHeight = 1.15f;
        [SerializeField] private float crouchTransitionSpeed = 10f;

        public float WalkSpeed => walkSpeed;
        public float RunSpeed => runSpeed;
        public float CrouchSpeed => crouchSpeed;
        public float Acceleration => acceleration;
        public float GroundFriction => groundFriction;
        public float AirAcceleration => airAcceleration;
        public float Gravity => gravity;
        public float JumpHeight => jumpHeight;
        public float StandingHeight => standingHeight;
        public float CrouchedHeight => crouchedHeight;
        public float CrouchTransitionSpeed => crouchTransitionSpeed;
    }
}
