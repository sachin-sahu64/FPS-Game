using UnityEngine;

namespace FPSGame.Weapons
{
    [CreateAssetMenu(menuName = "FPS Game/Weapons/Weapon Definition", fileName = "WeaponDefinition")]
    public class WeaponDefinition : ScriptableObject
    {
        [SerializeField] private string weaponId = "rifle_ak";
        [SerializeField] private bool automatic = true;
        [SerializeField] private int magazineSize = 30;
        [SerializeField] private int reserveAmmo = 90;
        [SerializeField] private float reloadTime = 2.4f;
        [SerializeField] private float fireRate = 10f;
        [SerializeField] private float damage = 36f;
        [SerializeField] private float range = 100f;
        [SerializeField] private float baseSpread = 0.005f;
        [SerializeField] private float movementSpreadMultiplier = 0.05f;
        [SerializeField] private float penetrationPower = 1.5f;
        [SerializeField] private float penetrationProbeDistance = 2f;
        [SerializeField] private float damageFalloffPerMeter = 0.9f;
        [SerializeField] private LayerMask hitMask = ~0;
        [SerializeField] private RecoilPattern recoilPattern;

        public string WeaponId => weaponId;
        public bool Automatic => automatic;
        public int MagazineSize => magazineSize;
        public int ReserveAmmo => reserveAmmo;
        public float ReloadTime => reloadTime;
        public float FireRate => fireRate;
        public float Damage => damage;
        public float Range => range;
        public float BaseSpread => baseSpread;
        public float MovementSpreadMultiplier => movementSpreadMultiplier;
        public float PenetrationPower => penetrationPower;
        public float PenetrationProbeDistance => penetrationProbeDistance;
        public float DamageFalloffPerMeter => damageFalloffPerMeter;
        public LayerMask HitMask => hitMask;
        public RecoilPattern RecoilPattern => recoilPattern;
    }
}
