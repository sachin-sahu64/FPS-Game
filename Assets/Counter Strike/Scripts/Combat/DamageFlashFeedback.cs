using UnityEngine;

namespace FPSGame.Combat
{
    [DisallowMultipleComponent]
    public class DamageFlashFeedback : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        [SerializeField] private Health health;
        [SerializeField] private Renderer[] targetRenderers;
        [SerializeField] private Color flashColor = new(1f, 0.2f, 0.2f, 1f);
        [SerializeField] private float flashDuration = 0.12f;

        private MaterialPropertyBlock propertyBlock;
        private float flashUntilTime;

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();

            if (health == null)
            {
                health = GetComponent<Health>();
            }

            if (targetRenderers == null || targetRenderers.Length == 0)
            {
                targetRenderers = GetComponentsInChildren<Renderer>(true);
            }

            if (health != null)
            {
                health.Damaged += HandleDamaged;
                health.Died += HandleDied;
            }
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.Damaged -= HandleDamaged;
                health.Died -= HandleDied;
            }
        }

        private void LateUpdate()
        {
            float strength = flashDuration <= 0f ? 0f : Mathf.Clamp01((flashUntilTime - Time.time) / flashDuration);
            ApplyFlash(strength);
        }

        private void HandleDamaged(Health damagedHealth, DamageInfo damageInfo)
        {
            flashUntilTime = Time.time + flashDuration;
        }

        private void HandleDied(Health damagedHealth, DamageInfo damageInfo)
        {
            flashUntilTime = Time.time + flashDuration * 1.5f;
        }

        private void ApplyFlash(float strength)
        {
            if (propertyBlock == null || targetRenderers == null)
            {
                return;
            }

            Color tint = Color.Lerp(Color.white, flashColor, strength);

            for (int index = 0; index < targetRenderers.Length; index++)
            {
                Renderer rendererComponent = targetRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                rendererComponent.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(BaseColorId, tint);
                propertyBlock.SetColor(ColorId, tint);
                rendererComponent.SetPropertyBlock(propertyBlock);
            }
        }
    }
}
