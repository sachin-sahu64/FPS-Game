using UnityEngine;
using FPSGame.Input;

namespace FPSGame.Movement
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMotor : MonoBehaviour
    {
        [SerializeField] private PlayerMovementSettings settings;
        [SerializeField] private ActorInputSource inputSource;

        private CharacterController controller;
        private Vector3 planarVelocity;
        private float verticalVelocity;

        public Vector2 MoveInput { get; private set; }

        public bool IsGrounded => controller != null && controller.isGrounded;

        public bool IsCrouching { get; private set; }

        public float CurrentPlanarSpeed => new Vector3(planarVelocity.x, 0f, planarVelocity.z).magnitude;

        public float NormalizedMovementPenalty
        {
            get
            {
                if (settings == null || settings.RunSpeed <= 0f)
                {
                    return 0f;
                }

                return Mathf.Clamp01(CurrentPlanarSpeed / settings.RunSpeed);
            }
        }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();

            if (inputSource == null)
            {
                inputSource = GetComponent<ActorInputSource>() ?? GetComponentInParent<ActorInputSource>();
            }
        }

        private void Update()
        {
            if (settings == null)
            {
                return;
            }

            ReadInputState(inputSource != null ? inputSource.ReadState() : default);
            UpdateStance();
            UpdatePlanarVelocity();
            UpdateVerticalVelocity();

            Vector3 frameVelocity = planarVelocity + Vector3.up * verticalVelocity;
            controller.Move(frameVelocity * Time.deltaTime);
        }

        private void ReadInputState(ActorInputState inputState)
        {
            Vector2 input = inputState.Move;
            IsCrouching = inputState.CrouchHeld;
            MoveInput = input.sqrMagnitude > 1f ? input.normalized : input;
        }

        private void UpdateStance()
        {
            float targetHeight = IsCrouching ? settings.CrouchedHeight : settings.StandingHeight;
            controller.height = Mathf.Lerp(controller.height, targetHeight, settings.CrouchTransitionSpeed * Time.deltaTime);
            controller.center = Vector3.up * controller.height * 0.5f;
        }

        private void UpdatePlanarVelocity()
        {
            float targetSpeed = settings.WalkSpeed;

            if (IsCrouching)
            {
                targetSpeed = settings.CrouchSpeed;
            }
            else if (inputSource != null && inputSource.ReadState().RunHeld)
            {
                targetSpeed = settings.RunSpeed;
            }

            Vector3 desiredDirection = transform.right * MoveInput.x + transform.forward * MoveInput.y;
            Vector3 desiredVelocity = desiredDirection * targetSpeed;
            float acceleration = IsGrounded ? settings.Acceleration : settings.AirAcceleration;

            planarVelocity = Vector3.MoveTowards(planarVelocity, desiredVelocity, acceleration * Time.deltaTime);

            if (IsGrounded && MoveInput.sqrMagnitude < 0.01f)
            {
                planarVelocity = Vector3.MoveTowards(planarVelocity, Vector3.zero, settings.GroundFriction * Time.deltaTime);
            }
        }

        private void UpdateVerticalVelocity()
        {
            if (IsGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            if (IsGrounded && inputSource != null && inputSource.ReadState().JumpPressed)
            {
                verticalVelocity = Mathf.Sqrt(settings.JumpHeight * -2f * settings.Gravity);
            }

            verticalVelocity += settings.Gravity * Time.deltaTime;
        }
    }
}
