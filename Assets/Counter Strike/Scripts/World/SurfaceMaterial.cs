using UnityEngine;

namespace FPSGame.World
{
    public enum SurfaceType
    {
        Default,
        Concrete,
        Metal,
        Wood,
        Sand,
        Flesh
    }

    public class SurfaceMaterial : MonoBehaviour
    {
        [SerializeField] private SurfaceType surfaceType = SurfaceType.Default;
        [SerializeField] [Min(0.1f)] private float penetrationResistance = 1f;
        [SerializeField] private AudioClip[] footstepClips;

        public SurfaceType Type => surfaceType;

        public float PenetrationResistance => penetrationResistance;

        public AudioClip[] FootstepClips => footstepClips;
    }
}
