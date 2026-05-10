using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    public delegate void OnDeath();
    public event OnDeath onDeath;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log(gameObject.name + " took " + amount + " damage. HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        onDeath?.Invoke();
        Debug.Log(gameObject.name + " Died!");
        // For now, let's just destroy the object or disable it
        // Destroy(gameObject);
    }
}
