using UnityEngine;
using FPSGame.Input;

namespace FPSGame.Movement
{
    public class FirstPersonLook : MonoBehaviour
    {
        [SerializeField] private Transform yawRoot;
        [SerializeField] private Transform pitchRoot;
        [SerializeField] private ActorInputSource inputSource;
        [SerializeField] private float sensitivity = 0.08f;
        [SerializeField] private float pitchClamp = 85f;
        [SerializeField] private bool lockCursorOnEnable = true;

        private float yaw;
        private float pitch;

        private void Awake()
        {
            if (inputSource == null)
            {
                inputSource = GetComponent<ActorInputSource>() ?? GetComponentInParent<ActorInputSource>();
            }
        }

        private void OnEnable()
        {
            if (!lockCursorOnEnable || (inputSource != null && !inputSource.ShouldLockCursor))
            {
                return;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            Vector2 delta = (inputSource != null ? inputSource.ReadState().LookDelta : Vector2.zero) * sensitivity;
            yaw += delta.x;
            pitch = Mathf.Clamp(pitch - delta.y, -pitchClamp, pitchClamp);

            if (yawRoot != null)
            {
                yawRoot.localRotation = Quaternion.Euler(0f, yaw, 0f);
            }

            if (pitchRoot != null)
            {
                pitchRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }
        }

        public void AddRecoil(Vector2 recoilDelta)
        {
            yaw += recoilDelta.x;
            pitch = Mathf.Clamp(pitch - recoilDelta.y, -pitchClamp, pitchClamp);
        }

        public void SetLookAngles(float yawAngle, float pitchAngle)
        {
            yaw = yawAngle;
            pitch = Mathf.Clamp(pitchAngle, -pitchClamp, pitchClamp);
        }
    }
}
