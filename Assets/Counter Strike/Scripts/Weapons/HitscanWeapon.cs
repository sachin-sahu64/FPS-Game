using System.Collections;
using FPSGame.Combat;
using FPSGame.Input;
using FPSGame.Movement;
using FPSGame.Rounds;
using FPSGame.World;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FPSGame.Weapons
{
    public class HitscanWeapon : MonoBehaviour
    {
        private static Material tracerMaterial;

        [SerializeField] private WeaponDefinition definition;
        [SerializeField] private Camera aimCamera;
        [SerializeField] private Transform muzzle;
        [SerializeField] private PlayerMotor motor;
        [SerializeField] private FirstPersonLook look;
        [SerializeField] private ActorInputSource inputSource;
        [SerializeField] private bool drawHumanTracers = true;
        [SerializeField] private bool drawBotTracers = true;
        [SerializeField] private bool logHumanShots = true;
        [SerializeField] private float humanTracerDuration = 0.08f;
        [SerializeField] private float botTracerDuration = 0.18f;
        [SerializeField] private float humanTracerWidth = 0.02f;
        [SerializeField] private float botTracerWidth = 0.06f;
        [SerializeField] private Color humanTracerColor = new(1f, 0.85f, 0.2f, 1f);
        [SerializeField] private Color terroristTracerColor = new(1f, 0.35f, 0.15f, 1f);
        [SerializeField] private Color counterTerroristTracerColor = new(0.15f, 0.7f, 1f, 1f);

        private int currentMagazineAmmo;
        private int currentReserveAmmo;
        private float nextShotTime;
        private float lastShotTime;
        private int shotIndex;
        private bool isReloading;
        private GameObject instigatorRoot;
        private TeamMember instigatorMember;
        private bool isBotControlled;

        public int CurrentMagazineAmmo => currentMagazineAmmo;

        public int CurrentReserveAmmo => currentReserveAmmo;

        public int MagazineCapacity => definition != null ? definition.MagazineSize : 0;

        public float EffectiveRange => definition != null ? definition.Range : 0f;

        public bool IsReloading => isReloading;

        private void Awake()
        {
            ResolveReferences();
            ResetWeaponState();
        }

        private void Update()
        {
            if (definition == null)
            {
                return;
            }

            ResolveReferences();
            ActorInputState inputState = ReadInputState();

            if (!definition.Automatic && inputState.FirePressed)
            {
                TryFire();
            }
            else if (definition.Automatic && (inputState.FireHeld || inputState.FirePressed))
            {
                TryFire();
            }

            if (inputState.ReloadPressed)
            {
                TryStartReload();
            }

            if (definition.RecoilPattern != null && Time.time - lastShotTime > definition.RecoilPattern.ResetDelay)
            {
                shotIndex = 0;
            }
        }

        public void TryFire()
        {
            if (isReloading || Time.time < nextShotTime || currentMagazineAmmo <= 0)
            {
                return;
            }

            currentMagazineAmmo--;
            nextShotTime = Time.time + (1f / definition.FireRate);
            lastShotTime = Time.time;

            Ray ray = BuildShotRay();
            ShotResult shotResult = ResolveShot(ray.origin, ray.direction, definition.Range, definition.Damage, definition.PenetrationPower);

            if (look != null && definition.RecoilPattern != null)
            {
                look.AddRecoil(definition.RecoilPattern.GetShotKick(shotIndex));
            }

            if (!isBotControlled)
            {
                if (drawHumanTracers)
                {
                    DrawTracer(ray.origin, shotResult.EndPoint, humanTracerColor, humanTracerWidth, humanTracerDuration);
                }

                if (logHumanShots)
                {
                    Debug.Log($"Player shot fired. Hit={shotResult.HitType} Target={shotResult.TargetName} End={shotResult.EndPoint}");
                }
            }
            else if (drawBotTracers)
            {
                DrawTracer(ray.origin, shotResult.EndPoint, GetBotTracerColor(), botTracerWidth, botTracerDuration);
            }

            shotIndex++;
        }

        public void ResetWeaponState()
        {
            if (isReloading)
            {
                StopAllCoroutines();
            }

            isReloading = false;
            nextShotTime = 0f;
            lastShotTime = -999f;
            shotIndex = 0;

            if (definition != null)
            {
                currentMagazineAmmo = definition.MagazineSize;
                currentReserveAmmo = definition.ReserveAmmo;
            }
        }

        public void TryStartReload()
        {
            if (isReloading || currentMagazineAmmo >= definition.MagazineSize || currentReserveAmmo <= 0)
            {
                return;
            }

            StartCoroutine(ReloadRoutine());
        }

        private IEnumerator ReloadRoutine()
        {
            isReloading = true;
            yield return new WaitForSeconds(definition.ReloadTime);

            int neededAmmo = definition.MagazineSize - currentMagazineAmmo;
            int ammoToLoad = Mathf.Min(neededAmmo, currentReserveAmmo);
            currentMagazineAmmo += ammoToLoad;
            currentReserveAmmo -= ammoToLoad;
            isReloading = false;
        }

        private Ray BuildShotRay()
        {
            Camera sourceCamera = aimCamera;
            Transform aimTransform = sourceCamera != null
                ? sourceCamera.transform
                : muzzle != null
                    ? muzzle
                    : transform;
            Vector3 origin = aimTransform.position;
            Vector3 forward = aimTransform.forward;
            float spread = definition.BaseSpread;

            if (motor != null)
            {
                spread += motor.NormalizedMovementPenalty * definition.MovementSpreadMultiplier;
            }

            Vector2 randomSpread = Random.insideUnitCircle * spread;
            Vector3 direction = (forward +
                                 aimTransform.right * randomSpread.x +
                                 aimTransform.up * randomSpread.y).normalized;

            if (muzzle != null)
            {
                origin = muzzle.position;
            }

            return new Ray(origin, direction);
        }

        private ShotResult ResolveShot(Vector3 origin, Vector3 direction, float remainingDistance, float remainingDamage, float remainingPenetration)
        {
            const float SurfacePadding = 0.05f;
            const int MaxPenetrations = 4;

            Vector3 currentOrigin = origin;
            float currentDistance = remainingDistance;
            float currentDamage = remainingDamage;
            float currentPenetration = remainingPenetration;

            for (int penetrationCount = 0; penetrationCount < MaxPenetrations && currentDistance > 0f; penetrationCount++)
            {
                Ray ray = new(currentOrigin, direction);
                RaycastHit[] hits = CombatRaycastUtility.GetSortedHits(ray, currentDistance, definition.HitMask);
                if (hits.Length == 0)
                {
                    return ShotResult.Miss(currentOrigin + direction * currentDistance);
                }

                RaycastHit hit = default;
                bool foundRelevantHit = false;

                for (int index = 0; index < hits.Length; index++)
                {
                    if (CombatRaycastUtility.ShouldIgnoreHit(instigatorMember, hits[index].collider))
                    {
                        continue;
                    }

                    hit = hits[index];
                    foundRelevantHit = true;
                    break;
                }

                if (!foundRelevantHit)
                {
                    return ShotResult.Miss(currentOrigin + direction * currentDistance);
                }

                if (hit.collider.GetComponentInParent<IDamageable>() is { } damageable)
                {
                    DamageInfo damageInfo = new(currentDamage, hit.point, hit.normal, instigatorRoot, this);
                    damageable.ApplyDamage(damageInfo);
                    return ShotResult.Damage(hit.point, hit.collider.name);
                }

                if (!hit.collider.TryGetComponent(out SurfaceMaterial surface))
                {
                    return ShotResult.Blocked(hit.point, hit.collider.name);
                }

                float thickness = EstimateThickness(hit.collider, hit.point, direction);
                float penetrationCost = Mathf.Max(0.1f, thickness * surface.PenetrationResistance);

                if (penetrationCost > currentPenetration)
                {
                    return ShotResult.Blocked(hit.point, hit.collider.name);
                }

                currentPenetration -= penetrationCost;
                currentDamage *= Mathf.Clamp01(Mathf.Pow(definition.DamageFalloffPerMeter, thickness));
                currentDistance -= hit.distance + thickness;
                currentOrigin = hit.point + direction * (thickness + SurfacePadding);
            }

            return ShotResult.Miss(currentOrigin + direction * Mathf.Max(currentDistance, 0f));
        }

        private float EstimateThickness(Collider collider, Vector3 entryPoint, Vector3 direction)
        {
            float probeDistance = Mathf.Max(definition.PenetrationProbeDistance, 0.25f);
            Vector3 probeStart = entryPoint + direction * probeDistance;
            Ray reverseRay = new(probeStart, -direction);

            if (collider.Raycast(reverseRay, out RaycastHit exitHit, probeDistance))
            {
                return Mathf.Clamp(probeDistance - exitHit.distance, 0.05f, probeDistance);
            }

            return probeDistance;
        }

        private void ResolveReferences()
        {
            if (motor == null)
            {
                motor = GetComponentInParent<PlayerMotor>();
            }

            if (look == null)
            {
                look = GetComponentInParent<FirstPersonLook>();
            }

            if (inputSource == null)
            {
                inputSource = GetComponentInParent<ActorInputSource>();
            }

            if (instigatorMember == null)
            {
                instigatorMember = GetComponentInParent<TeamMember>();
                isBotControlled = instigatorMember != null && instigatorMember.IsBot;
            }

            if (instigatorRoot == null)
            {
                instigatorRoot = instigatorMember != null ? instigatorMember.gameObject : transform.root.gameObject;
            }
        }

        private ActorInputState ReadInputState()
        {
            ActorInputState state = inputSource != null ? inputSource.ReadState() : default;

            if (!isBotControlled)
            {
                if (Mouse.current != null)
                {
                    state.FireHeld |= Mouse.current.leftButton.isPressed;
                    state.FirePressed |= Mouse.current.leftButton.wasPressedThisFrame;
                }

                if (Keyboard.current != null)
                {
                    state.ReloadPressed |= Keyboard.current.rKey.wasPressedThisFrame;
                }
            }

            return state;
        }

        private void DrawTracer(Vector3 start, Vector3 end, Color color, float width, float duration)
        {
            GameObject tracer = new("ShotTracer");
            tracer.transform.position = start;
            LineRenderer lineRenderer = tracer.AddComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
            lineRenderer.alignment = LineAlignment.View;
            lineRenderer.numCapVertices = 4;
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width * 0.65f;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.material = GetTracerMaterial();
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            Destroy(tracer, duration);
        }

        private Color GetBotTracerColor()
        {
            if (instigatorMember == null)
            {
                return humanTracerColor;
            }

            return instigatorMember.Side == TeamSide.Terrorists
                ? terroristTracerColor
                : counterTerroristTracerColor;
        }

        private static Material GetTracerMaterial()
        {
            if (tracerMaterial != null)
            {
                return tracerMaterial;
            }

            Shader shader = Shader.Find("Sprites/Default");
            tracerMaterial = new Material(shader)
            {
                color = Color.white
            };
            return tracerMaterial;
        }

        private readonly struct ShotResult
        {
            private ShotResult(Vector3 endPoint, string hitType, string targetName)
            {
                EndPoint = endPoint;
                HitType = hitType;
                TargetName = targetName;
            }

            public Vector3 EndPoint { get; }

            public string HitType { get; }

            public string TargetName { get; }

            public static ShotResult Miss(Vector3 endPoint)
            {
                return new ShotResult(endPoint, "Miss", string.Empty);
            }

            public static ShotResult Blocked(Vector3 endPoint, string targetName)
            {
                return new ShotResult(endPoint, "Blocked", targetName);
            }

            public static ShotResult Damage(Vector3 endPoint, string targetName)
            {
                return new ShotResult(endPoint, "Damage", targetName);
            }
        }
    }
}
