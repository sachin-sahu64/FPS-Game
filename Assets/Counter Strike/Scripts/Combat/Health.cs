using System;
using UnityEngine;

namespace FPSGame.Combat
{
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private bool destroyOnDeath;

        public event Action<Health, DamageInfo> Damaged;
        public event Action<Health, DamageInfo> Died;

        public float CurrentHealth { get; private set; }

        public bool IsAlive => CurrentHealth > 0f;

        private void Awake()
        {
            ResetState();
        }

        public void ApplyDamage(DamageInfo damageInfo)
        {
            if (!IsAlive)
            {
                return;
            }

            CurrentHealth = Mathf.Max(0f, CurrentHealth - damageInfo.Amount);
            Damaged?.Invoke(this, damageInfo);

            if (CurrentHealth > 0f)
            {
                return;
            }

            Died?.Invoke(this, damageInfo);

            if (destroyOnDeath)
            {
                Destroy(gameObject);
            }
        }

        public void Heal(float amount)
        {
            if (!IsAlive || amount <= 0f)
            {
                return;
            }

            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        }

        public void ResetState()
        {
            CurrentHealth = maxHealth;
        }
    }
}
