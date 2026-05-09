using UnityEngine;

namespace FPSGame.Weapons
{
    [CreateAssetMenu(menuName = "FPS Game/Weapons/Recoil Pattern", fileName = "RecoilPattern")]
    public class RecoilPattern : ScriptableObject
    {
        [SerializeField] private Vector2[] sprayOffsets = { new(0.1f, 0.8f), new(-0.15f, 0.95f), new(0.2f, 1.1f) };
        [SerializeField] private float resetDelay = 0.4f;

        public float ResetDelay => resetDelay;

        public Vector2 GetShotKick(int shotIndex)
        {
            if (sprayOffsets == null || sprayOffsets.Length == 0)
            {
                return Vector2.zero;
            }

            int index = Mathf.Clamp(shotIndex, 0, sprayOffsets.Length - 1);
            return sprayOffsets[index];
        }
    }
}
