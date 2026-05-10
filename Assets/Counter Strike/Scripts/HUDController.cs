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
    public float baseSize = 50f;
    public float currentSize;

    void Update()
    {
        UpdateHealthUI();
        UpdateWeaponUI();
        UpdateCrosshair();
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
