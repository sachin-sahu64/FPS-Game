using FPSGame.Input;
using FPSGame.Rounds;
using FPSGame.Weapons;
using FPSGame.Combat;
using FPSGame.Objectives;
using UnityEngine;
using UnityEngine.AI;

namespace FPSGame.AI
{
    [RequireComponent(typeof(BotInputSource))]
    [RequireComponent(typeof(TeamMember))]
    public class BotController : MonoBehaviour
    {
        private const float MinimumSearchDistance = 200f;

        [SerializeField] private RoundManager roundManager;
        [SerializeField] private TeamMember teamMember;
        [SerializeField] private BotInputSource inputSource;
        [SerializeField] private NavMeshAgent navMeshAgent;
        [SerializeField] private HitscanWeapon primaryWeapon;
        [SerializeField] private Transform yawRoot;
        [SerializeField] private Transform pitchRoot;
        [SerializeField] private Transform eyePoint;
        [SerializeField] private LayerMask lineOfSightMask = ~0;
        [SerializeField] private float targetRefreshInterval = 0.35f;
        [SerializeField] private float pathRefreshInterval = 0.2f;
        [SerializeField] private float engageDistance = 35f;
        [SerializeField] private float stopDistance = 14f;
        [SerializeField] private float targetLossDistance = 65f;
        [SerializeField] private bool strictlyEnforceRange = true;
        [SerializeField] private float fireConeAngle = 5f;
        [SerializeField] private float yawTurnSpeed = 270f;
        [SerializeField] private float pitchTurnSpeed = 180f;
        [SerializeField] private float patrolRadius = 8f;
        [SerializeField] private float separationRadius = 2.4f;
        [SerializeField] private float separationStrength = 2.2f;

        private TeamMember currentTarget;
        private Vector3 homeAnchor;
        private Vector3 currentDestination;
        private float currentPitch;
        private float nextTargetRefreshTime;
        private float nextPathRefreshTime;
        private float nextPatrolRefreshTime;
        private float strafeDirection = 1f;
        private float nextStrafeSwitchTime;
        private bool hasHomeAnchor;
        private BombSite assignedSite;
        private readonly Collider[] separationHits = new Collider[24];

        private void Awake()
        {
            if (teamMember == null)
            {
                teamMember = GetComponent<TeamMember>();
            }

            if (inputSource == null)
            {
                inputSource = GetComponent<BotInputSource>();
            }

            if (navMeshAgent == null)
            {
                navMeshAgent = GetComponent<NavMeshAgent>();
            }

            if (primaryWeapon == null)
            {
                primaryWeapon = GetComponentInChildren<HitscanWeapon>();
            }

            if (roundManager == null)
            {
                roundManager = FindFirstObjectByType<RoundManager>();
            }

            if (yawRoot == null)
            {
                yawRoot = transform;
            }

            if (eyePoint == null)
            {
                eyePoint = transform;
            }

            if (navMeshAgent != null)
            {
                navMeshAgent.updatePosition = false;
                navMeshAgent.updateRotation = false;
                navMeshAgent.stoppingDistance = stopDistance;
            }
        }

        private void Update()
        {
            if (teamMember == null || teamMember.Health == null || !teamMember.Health.IsAlive)
            {
                inputSource.SetState(default);
                return;
            }

            if (roundManager == null || !roundManager.IsRoundActive)
            {
                currentTarget = null;
                currentDestination = transform.position;
                StopNavigation();
                inputSource.SetState(default);
                return;
            }

            // If we are a T carrying bomb or a CT with bomb planted, we should prioritize moving/interacting over searching for enemies
            bool isMissionPriority = ShouldPrioritizeObjective();

            if (Time.time >= nextTargetRefreshTime)
            {
                // If on mission, only target enemies if we aren't already at the interaction spot
                if (isMissionPriority && IsAtObjective())
                {
                    currentTarget = null; 
                }
                else
                {
                    currentTarget = SelectTarget();
                }
                nextTargetRefreshTime = Time.time + targetRefreshInterval;
            }

            bool hasSight = false;
            Vector3 aimPoint = transform.position + transform.forward * 5f;

            if (currentTarget != null)
            {
                hasSight = TryGetTargetAimPoint(currentTarget, out aimPoint);
                UpdateCombatDestination(currentTarget, hasSight);
            }
            else
            {
                if (isMissionPriority)
                {
                    UpdateObjectiveDestination();
                }
                else
                {
                    UpdatePatrolDestination();
                }
            }

            UpdateAim(aimPoint);
            UpdateInputState(hasSight, aimPoint);

            if (roundManager != null && roundManager.ActiveBomb != null)
            {
                roundManager.ActiveBomb.TryInteract(teamMember, inputSource.ReadState().InteractHeld);
            }
        }

        private bool IsAtObjective()
        {
            if (roundManager == null || roundManager.ActiveBomb == null) return false;

            if (teamMember.Side == TeamSide.Terrorists && roundManager.ActiveBomb.Carrier == teamMember)
            {
                return assignedSite != null && Vector3.Distance(transform.position, assignedSite.transform.position) <= assignedSite.PlantRadius;
            }
            
            if (teamMember.Side == TeamSide.CounterTerrorists && roundManager.ActiveBomb.State == BombState.Planted)
            {
                return Vector3.Distance(transform.position, roundManager.ActiveBomb.transform.position) <= 2.5f;
            }

            return false;
        }

        private void LateUpdate()
        {
            if (navMeshAgent != null && navMeshAgent.enabled)
            {
                navMeshAgent.nextPosition = transform.position;
            }
        }

        public void SetHomeAnchor(Vector3 anchor)
        {
            homeAnchor = anchor;
            hasHomeAnchor = true;
            currentDestination = anchor;

            if (navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.Warp(anchor);
                navMeshAgent.nextPosition = anchor;
            }
        }

        private TeamMember SelectTarget()
        {
            if (roundManager == null)
            {
                return null;
            }

            TeamMember bestTarget = null;
            float bestScore = float.MaxValue;
            Vector3 selfPosition = transform.position;
            float searchDistance = Mathf.Max(targetLossDistance, MinimumSearchDistance);

            foreach (TeamMember member in roundManager.Roster)
            {
                if (member == null || member == teamMember || member.Side == teamMember.Side || member.Health == null || !member.Health.IsAlive)
                {
                    continue;
                }

                float distance = Vector3.Distance(selfPosition, member.transform.position);

                if (distance > searchDistance)
                {
                    continue;
                }

                bool hasSight = TryGetTargetAimPoint(member, out _);
                float score = distance - (hasSight ? 8f : 0f);

                if (score < bestScore)
                {
                    bestScore = score;
                    bestTarget = member;
                }
            }

            return bestTarget;
        }

        private void UpdateCombatDestination(TeamMember target, bool hasSight)
        {
            // If we are on a mission, don't stop for combat
            if (ShouldPrioritizeObjective())
            {
                UpdateObjectiveDestination();
                return;
            }

            float distance = Vector3.Distance(transform.position, target.transform.position);

            if (hasSight && distance <= stopDistance)
            {
                StopNavigation();
                currentDestination = transform.position;
                return;
            }

            currentDestination = target.transform.position;

            if (Time.time >= nextPathRefreshTime)
            {
                SetDestination(currentDestination);
                nextPathRefreshTime = Time.time + pathRefreshInterval;
            }
        }

        private bool ShouldPrioritizeObjective()
        {
            if (roundManager == null || roundManager.ActiveBomb == null) return false;

            // Terrorist carrying the bomb
            if (teamMember.Side == TeamSide.Terrorists && roundManager.ActiveBomb.Carrier == teamMember)
            {
                return true;
            }

            // Counter-Terrorist when bomb is planted
            if (teamMember.Side == TeamSide.CounterTerrorists && roundManager.ActiveBomb.State == BombState.Planted)
            {
                return true;
            }

            return false;
        }

        private void UpdateObjectiveDestination()
        {
            if (roundManager == null || roundManager.ActiveBomb == null) return;

            if (teamMember.Side == TeamSide.Terrorists && roundManager.ActiveBomb.Carrier == teamMember)
            {
                if (assignedSite == null)
                {
                    BombSite[] sites = FindObjectsByType<BombSite>(FindObjectsSortMode.None);
                    if (sites.Length > 0) assignedSite = sites[Random.Range(0, sites.Length)];
                }

                if (assignedSite != null)
                {
                    currentDestination = assignedSite.transform.position;
                }
            }
            else if (teamMember.Side == TeamSide.CounterTerrorists && roundManager.ActiveBomb.State == BombState.Planted)
            {
                currentDestination = roundManager.ActiveBomb.transform.position;
            }

            if (Time.time >= nextPathRefreshTime)
            {
                SetDestination(currentDestination);
                nextPathRefreshTime = Time.time + pathRefreshInterval;
            }
        }

        private void UpdatePatrolDestination()
        {
            if (ShouldPrioritizeObjective())
            {
                UpdateObjectiveDestination();
                return;
            }

            if (!hasHomeAnchor)
            {
                currentDestination = transform.position;
                StopNavigation();
                return;
            }

            if (Time.time < nextPatrolRefreshTime && Vector3.Distance(transform.position, currentDestination) > 1.5f)
            {
                return;
            }

            Vector2 offset = Random.insideUnitCircle * patrolRadius;
            currentDestination = homeAnchor + new Vector3(offset.x, 0f, offset.y);
            nextPatrolRefreshTime = Time.time + Random.Range(2f, 4f);
            SetDestination(currentDestination);
        }

        private void UpdateAim(Vector3 aimPoint)
        {
            Vector3 origin = eyePoint != null ? eyePoint.position : transform.position + Vector3.up * 1.5f;
            Vector3 toAim = aimPoint - origin;

            if (toAim.sqrMagnitude < 0.001f)
            {
                return;
            }

            Vector3 flatDirection = Vector3.ProjectOnPlane(toAim, Vector3.up);

            if (flatDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetYaw = Quaternion.LookRotation(flatDirection.normalized, Vector3.up);
                yawRoot.rotation = Quaternion.RotateTowards(yawRoot.rotation, targetYaw, yawTurnSpeed * Time.deltaTime);
            }

            if (pitchRoot != null)
            {
                float targetPitch = -Mathf.Atan2(toAim.y, flatDirection.magnitude) * Mathf.Rad2Deg;
                currentPitch = Mathf.MoveTowardsAngle(currentPitch, targetPitch, pitchTurnSpeed * Time.deltaTime);
                pitchRoot.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);
            }
        }

        private void UpdateInputState(bool hasSight, Vector3 aimPoint)
        {
            ActorInputState state = default;
            Vector3 desiredVelocity = GetDesiredVelocity();
            Vector3 localVelocity = transform.InverseTransformDirection(desiredVelocity);

            state.Move = new Vector2(localVelocity.x, localVelocity.z);
            if (state.Move.sqrMagnitude > 1f)
            {
                state.Move.Normalize();
            }

            if (currentTarget != null)
            {
                float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
                float effectiveRange = primaryWeapon != null && primaryWeapon.EffectiveRange > 0f ? primaryWeapon.EffectiveRange : engageDistance;
                state.RunHeld = distance > Mathf.Min(effectiveRange * 0.55f, Mathf.Max(engageDistance, stopDistance + 1f)) || !hasSight;

                if (hasSight && distance <= stopDistance)
                {
                    if (Time.time >= nextStrafeSwitchTime)
                    {
                        strafeDirection *= -1f;
                        nextStrafeSwitchTime = Time.time + Random.Range(0.8f, 1.5f);
                    }

                    state.Move = new Vector2(strafeDirection * 0.55f, 0.15f);
                }

                if (primaryWeapon != null)
                {
                    Vector3 origin = eyePoint != null ? eyePoint.position : transform.position + Vector3.up * 1.5f;
                    Vector3 targetDirection = (aimPoint - origin).normalized;
                    float angle = Vector3.Angle(yawRoot.forward, targetDirection);
                    
                    float range = strictlyEnforceRange ? engageDistance : (primaryWeapon.EffectiveRange > 0f ? primaryWeapon.EffectiveRange : engageDistance);
                    bool inRange = distance <= range;
                    
                    // If on mission, only shoot if enemy is very close or we are already at the objective
                    bool isMission = ShouldPrioritizeObjective();
                    bool shouldShoot = hasSight && inRange && angle <= fireConeAngle;
                    
                    if (isMission && !IsAtObjective() && distance > 10f)
                    {
                        shouldShoot = false; // Prioritize running to the site
                    }

                    bool shouldReload = !primaryWeapon.IsReloading &&
                                        primaryWeapon.CurrentMagazineAmmo <= 0 &&
                                        primaryWeapon.CurrentReserveAmmo > 0;

                    state.FireHeld = shouldShoot;
                    state.FirePressed = shouldShoot;
                    state.ReloadPressed = shouldReload;
                }
            }

            // Objective interaction
            if (roundManager != null && roundManager.ActiveBomb != null)
            {
                bool isMission = ShouldPrioritizeObjective();
                if (teamMember.Side == TeamSide.Terrorists && roundManager.ActiveBomb.Carrier == teamMember)
                {
                    if (assignedSite != null && Vector3.Distance(transform.position, assignedSite.transform.position) <= assignedSite.PlantRadius)
                    {
                        state.InteractHeld = true;
                        state.Move = Vector2.zero; // Stop moving while planting
                        state.FireHeld = false; // Don't shoot while planting
                    }
                }
                else if (teamMember.Side == TeamSide.CounterTerrorists && roundManager.ActiveBomb.State == BombState.Planted)
                {
                    if (Vector3.Distance(transform.position, roundManager.ActiveBomb.transform.position) <= 2.5f)
                    {
                        state.InteractHeld = true;
                        state.Move = Vector2.zero; // Stop moving while defusing
                        state.FireHeld = false; // Don't shoot while defusing
                    }
                }
            }

            inputSource.SetState(state);
        }

        private Vector3 GetDesiredVelocity()
        {
            Vector3 separationVelocity = ComputeSeparationVelocity();

            if (navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
            {
                return Vector3.ClampMagnitude(navMeshAgent.desiredVelocity + separationVelocity, navMeshAgent.speed);
            }

            Vector3 flatDelta = Vector3.ProjectOnPlane(currentDestination - transform.position, Vector3.up);
            Vector3 pursuitVelocity = flatDelta.sqrMagnitude > 0.25f ? flatDelta.normalized : Vector3.zero;
            return Vector3.ClampMagnitude(pursuitVelocity + separationVelocity, 1f);
        }

        private bool TryGetTargetAimPoint(TeamMember target, out Vector3 aimPoint)
        {
            aimPoint = target.transform.position + Vector3.up * 1.4f;
            Vector3 origin = eyePoint != null ? eyePoint.position : transform.position + Vector3.up * 1.5f;
            return CombatRaycastUtility.HasLineOfSightToTarget(origin, aimPoint, teamMember, target, lineOfSightMask);
        }

        private void SetDestination(Vector3 destination)
        {
            if (navMeshAgent == null || !navMeshAgent.enabled || !navMeshAgent.isOnNavMesh)
            {
                currentDestination = destination;
                return;
            }

            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(destination);
            currentDestination = destination;
        }

        private void StopNavigation()
        {
            if (navMeshAgent != null && navMeshAgent.enabled)
            {
                navMeshAgent.isStopped = true;
                navMeshAgent.ResetPath();
            }
        }

        private Vector3 ComputeSeparationVelocity()
        {
            if (separationRadius <= 0f)
            {
                return Vector3.zero;
            }

            int hitCount = Physics.OverlapSphereNonAlloc(transform.position, separationRadius, separationHits, lineOfSightMask, QueryTriggerInteraction.Ignore);
            Vector3 separation = Vector3.zero;

            for (int index = 0; index < hitCount; index++)
            {
                Collider hit = separationHits[index];
                if (hit == null)
                {
                    continue;
                }

                TeamMember otherMember = hit.GetComponentInParent<TeamMember>();
                if (otherMember == null || otherMember == teamMember)
                {
                    continue;
                }

                Vector3 offset = transform.position - otherMember.transform.position;
                offset.y = 0f;
                float distance = Mathf.Max(0.1f, offset.magnitude);
                float weight = 1f - Mathf.Clamp01(distance / separationRadius);

                if (otherMember.Side == teamMember.Side)
                {
                    weight *= 1.15f;
                }

                separation += offset.normalized * weight;
            }

            if (separation.sqrMagnitude <= 0.001f)
            {
                return Vector3.zero;
            }

            return separation.normalized * separationStrength;
        }
    }
}
