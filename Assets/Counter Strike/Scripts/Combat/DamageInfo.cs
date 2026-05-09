using UnityEngine;

namespace FPSGame.Combat
{
    public readonly struct DamageInfo
    {
        public DamageInfo(float amount, Vector3 point, Vector3 normal, GameObject instigator, Object source)
        {
            Amount = amount;
            Point = point;
            Normal = normal;
            Instigator = instigator;
            Source = source;
        }

        public float Amount { get; }

        public Vector3 Point { get; }

        public Vector3 Normal { get; }

        public GameObject Instigator { get; }

        public Object Source { get; }
    }
}
