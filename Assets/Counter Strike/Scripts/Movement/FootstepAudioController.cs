using FPSGame.World;
using UnityEngine;

namespace FPSGame.Movement
{
    [RequireComponent(typeof(AudioSource))]
    public class FootstepAudioController : MonoBehaviour
    {
        [SerializeField] private PlayerMotor motor;
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private float minMoveSpeed = 1f;
        [SerializeField] private float walkStepInterval = 0.52f;
        [SerializeField] private float runStepInterval = 0.38f;
        [SerializeField] private AudioClip[] defaultFootsteps;

        private AudioSource audioSource;
        private float nextStepTime;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();

            if (motor == null)
            {
                motor = GetComponentInParent<PlayerMotor>();
            }
        }

        private void Update()
        {
            if (motor == null || !motor.IsGrounded || motor.CurrentPlanarSpeed < minMoveSpeed)
            {
                return;
            }

            if (Time.time < nextStepTime)
            {
                return;
            }

            PlayStep();

            float stepInterval = motor.NormalizedMovementPenalty > 0.75f ? runStepInterval : walkStepInterval;
            nextStepTime = Time.time + stepInterval;
        }

        private void PlayStep()
        {
            AudioClip[] pool = defaultFootsteps;
            Ray ray = new(transform.position + Vector3.up * 0.25f, Vector3.down);

            if (Physics.Raycast(ray, out RaycastHit hit, 2f, groundMask, QueryTriggerInteraction.Ignore) &&
                hit.collider.TryGetComponent(out SurfaceMaterial surface) &&
                surface.FootstepClips.Length > 0)
            {
                pool = surface.FootstepClips;
            }

            if (pool == null || pool.Length == 0)
            {
                return;
            }

            audioSource.PlayOneShot(pool[Random.Range(0, pool.Length)]);
        }
    }
}
