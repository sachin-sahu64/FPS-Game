using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("Player References")]
    public Health playerHealth;
    public WeaponController weaponController;

    [Header("UI Elements")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI weaponNameText;
    public Image healthFill;

    [Header("Crosshair")]
    public RectTransform crosshair;
    public Image hitmarkerImage; // New reference
    public float baseSize = 50f;
    public float currentSize;

    private float hitmarkerTimer;

    void Start()
    {
        if (hitmarkerImage != null) hitmarkerImage.color = new Color(1, 1, 1, 0);
    }

    void Update()
    {
        UpdateHealthUI();
        UpdateWeaponUI();
        UpdateCrosshair();
        UpdateHitmarker();
    }

    public void ShowHitmarker()
    {
        if (hitmarkerImage == null) return;
        hitmarkerImage.color = Color.white;
        hitmarkerTimer = 0.1f; // Show for 0.1 seconds
    }

    void UpdateHitmarker()
    {
        if (hitmarkerImage == null) return;
        if (hitmarkerTimer > 0)
        {
            hitmarkerTimer -= Time.deltaTime;
        }
        else
        {
            hitmarkerImage.color = Color.Lerp(hitmarkerImage.color, new Color(1, 1, 1, 0), Time.deltaTime * 10f);
        }
    }

    void UpdateHealthUI()
    {
        if (playerHealth == null) return;

        healthText.text = "HP: " + playerHealth.GetCurrentHealth().ToString("F0");
        if (healthFill != null) healthFill.fillAmount = playerHealth.GetCurrentHealth() / playerHealth.maxHealth;
    }

    void UpdateWeaponUI()
    {
        if (weaponController == null || weaponController.activeWeapon == null)
        {
            ammoText.text = "- / -";
            weaponNameText.text = "NONE";
            return;
        }

        Weapon active = weaponController.activeWeapon;
        
        if (active.weaponType == Weapon.WeaponType.Melee)
        {
            ammoText.text = "∞";
        }
        else
        {
            ammoText.text = active.currentAmmo + " / " + active.totalAmmo;
        }

        weaponNameText.text = active.weaponName.ToUpper();
    }

    void UpdateCrosshair()
    {
        if (crosshair == null) return;

        // Dynamic crosshair size based on movement or firing
        float targetSize = baseSize;
        
        // If we had velocity from movement:
        // targetSize += playerVelocity * spreadFactor;

        currentSize = Mathf.Lerp(currentSize, targetSize, Time.deltaTime * 10f);
        crosshair.sizeDelta = new Vector2(currentSize, currentSize);
    }
}
