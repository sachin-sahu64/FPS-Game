using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour
{
    public enum FireMode { Semi, Auto, BoltAction }
    public enum WeaponType { Pistol, Rifle, SMG, Sniper, Melee }

    [Header("General Settings")]
    public string weaponName = "Weapon";
    public WeaponType weaponType = WeaponType.Rifle;
    public FireMode fireMode = FireMode.Auto;
    public float damage = 20f;
    public float range = 100f;
    public float fireRate = 0.08f; 
    public float reloadTime = 1.5f;

    [Header("Melee Settings")]
    public float meleeRadius = 1.5f;

    [Header("Sniper Settings")]
    public bool hasScope;
    public float scopedFOV = 15f;
    private float defaultFOV;
    private bool isScoped;

    [Header("Ammo Settings")]
    public int magazineSize = 30;
    public int currentAmmo;
    public int totalAmmo = 90;

    [Header("Recoil & Spread")]
    public float spread = 0.01f;
    public float runningSpreadMultiplier = 2f;
    public Vector3 recoilRotation = new Vector3(-2f, 1f, 0.5f); // Up, Left/Right, Tilt

    [Header("Procedural Kickback")]
    public float kickbackAmount = 0.05f;
    public float kickbackSmoothness = 10f;
    private Vector3 currentKickback;
    private Vector3 originalLocalPos;

    [Header("References")]
    public Transform shootPoint;
    public ParticleSystem muzzleFlash;
    public GameObject hitEffectPrefab;
    public LayerMask hitLayer;
    private Recoil recoilSystem;
    private Camera mainCam;

    private float nextFireTime;
    private bool isReloading;
    private Animator animator;

    void Start()
    {
        currentAmmo = magazineSize;
        animator = GetComponentInChildren<Animator>();
        recoilSystem = GetComponentInParent<Recoil>();
        mainCam = Camera.main;
        if (mainCam != null) defaultFOV = mainCam.fieldOfView;
        originalLocalPos = transform.localPosition;
    }

    void Update()
    {
        // Smoothly return weapon model after kickback
        currentKickback = Vector3.Lerp(currentKickback, Vector3.zero, Time.deltaTime * kickbackSmoothness);
        transform.localPosition = originalLocalPos + currentKickback;
    }

    void OnEnable()
    {
        isReloading = false;
    }

    public void TryShoot()
    {
        if (isReloading || Time.time < nextFireTime) return;

        if (weaponType != WeaponType.Melee && currentAmmo <= 0)
        {
            return;
        }

        if (weaponType == WeaponType.Melee) MeleeAttack();
        else Shoot();
    }

    private void Shoot()
    {
        nextFireTime = Time.time + fireRate;
        currentAmmo--;

        if (muzzleFlash != null) muzzleFlash.Play();
        if (animator != null) animator.SetTrigger("Shoot");

        // Procedural Kickback
        currentKickback -= Vector3.forward * kickbackAmount;

        // Apply Recoil
        if (recoilSystem != null) recoilSystem.RecoilFire(recoilRotation);

        Vector3 shootDir = shootPoint.forward;
        
        // Add spread (more if not scoped)
        float currentSpread = isScoped ? spread * 0.1f : spread;
        shootDir.x += Random.Range(-currentSpread, currentSpread);
        shootDir.y += Random.Range(-currentSpread, currentSpread);

        if (Physics.Raycast(shootPoint.position, shootDir, out RaycastHit hit, range, hitLayer))
        {
            if (hit.collider.TryGetComponent(out Health health)) health.TakeDamage(damage);
            if (hitEffectPrefab != null) Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        }

        if (weaponType == WeaponType.Sniper && isScoped) ToggleScope(); // Auto unscope for bolt action feel
    }

    private void MeleeAttack()
    {
        nextFireTime = Time.time + fireRate;
        if (animator != null) animator.SetTrigger("Shoot"); // Reuse shoot trigger for swing

        Collider[] hits = Physics.OverlapSphere(shootPoint.position, meleeRadius, hitLayer);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out Health health)) health.TakeDamage(damage);
        }
    }

    public void ToggleScope()
    {
        if (!hasScope || isReloading) return;

        isScoped = !isScoped;
        if (mainCam != null) mainCam.fieldOfView = isScoped ? scopedFOV : defaultFOV;
        
        // Hide/Show weapon model or overlay if needed
        // GetComponentInChildren<MeshRenderer>().enabled = !isScoped; 
    }

    public void TryReload()
    {
        if (isReloading || currentAmmo == magazineSize || totalAmmo <= 0) return;
        StartCoroutine(Reload());
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reloading...");

        if (animator != null) animator.SetTrigger("Reload");

        yield return new WaitForSeconds(reloadTime);

        int ammoToLoad = magazineSize - currentAmmo;
        int ammoAvailable = Mathf.Min(ammoToLoad, totalAmmo);

        currentAmmo += ammoAvailable;
        totalAmmo -= ammoAvailable;

        isReloading = false;
    }
}
