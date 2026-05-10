using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public Weapon[] loadout;
    public Transform weaponHolder;
    public Weapon activeWeapon;
    private int currentWeaponIndex = 0;
    
    [Header("Input Settings")]
    public KeyCode reloadKey = KeyCode.R;
    public KeyCode fireKey = KeyCode.Mouse0;
    public KeyCode secondaryFireKey = KeyCode.Mouse1;

    void Start()
    {
        if (loadout.Length > 0)
        {
            EquipWeapon(0);
        }
    }

    void Update()
    {
        // Weapon Switching
        if (Input.GetKeyDown(KeyCode.Alpha1)) EquipWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) EquipWeapon(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) EquipWeapon(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) EquipWeapon(3);

        if (activeWeapon == null) return;

        // Fire Input
        if (activeWeapon.fireMode == Weapon.FireMode.Auto)
        {
            if (Input.GetKey(fireKey)) activeWeapon.TryShoot();
        }
        else
        {
            if (Input.GetKeyDown(fireKey)) activeWeapon.TryShoot();
        }

        // Secondary Fire (Scope)
        if (Input.GetKeyDown(secondaryFireKey))
        {
            activeWeapon.ToggleScope();
        }

        // Reload Input
        if (Input.GetKeyDown(reloadKey))
        {
            activeWeapon.TryReload();
        }
    }

    public void EquipWeapon(int index)
    {
        if (index < 0 || index >= loadout.Length) return;

        if (activeWeapon != null) activeWeapon.gameObject.SetActive(false);
        
        currentWeaponIndex = index;
        activeWeapon = loadout[currentWeaponIndex];
        activeWeapon.gameObject.SetActive(true);
    }
}
