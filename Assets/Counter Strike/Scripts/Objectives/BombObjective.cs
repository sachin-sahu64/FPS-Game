using FPSGame.Rounds;
using FPSGame.Input;
using UnityEngine;

namespace FPSGame.Objectives
{
    public enum BombState
    {
        Idle,
        Carried,
        Planted,
        Defused,
        Exploded
    }

    public class BombObjective : MonoBehaviour
    {
        [SerializeField] private RoundManager roundManager;
        [SerializeField] private float armedDuration = 40f;
        [SerializeField] private float plantTime = 3.5f;
        [SerializeField] private float defuseTime = 5f;

        private float explodeAt;
        private float currentInteractionProgress;
        private TeamMember carrier;
        private TeamMember currentInteractingActor;

        public BombState State { get; private set; } = BombState.Idle;

        public BombSite CurrentSite { get; private set; }

        public TeamMember Carrier => carrier;

        public float InteractionProgress => currentInteractionProgress;

        private void Awake()
        {
            if (roundManager == null)
            {
                roundManager = FindFirstObjectByType<RoundManager>();
            }
        }

        private void Update()
        {
            if (State == BombState.Planted && Time.time >= explodeAt)
            {
                State = BombState.Exploded;
                roundManager?.OnBombExploded(this);
            }

            if (State == BombState.Carried && carrier != null)
            {
                transform.position = carrier.transform.position;
                transform.rotation = carrier.transform.rotation;
            }
            else if (State == BombState.Idle)
            {
                CheckForPickup();
            }

            // Also check for local player interaction if they aren't a bot
            CheckLocalPlayerInteraction();
        }

        private void CheckForPickup()
        {
            if (roundManager == null) return;

            foreach (var member in roundManager.Roster)
            {
                if (member != null && member.Side == TeamSide.Terrorists && member.Health != null && member.Health.IsAlive)
                {
                    if (Vector3.Distance(transform.position, member.transform.position) <= 1.5f)
                    {
                        SetCarried(member);
                        break;
                    }
                }
            }
        }

        private void CheckLocalPlayerInteraction()
        {
            if (roundManager == null) return;
            
            foreach (var member in roundManager.Roster)
            {
                if (member != null && !member.IsBot)
                {
                    var input = member.GetComponent<ActorInputSource>();
                    if (input != null)
                    {
                        TryInteract(member, input.ReadState().InteractHeld);
                    }
                }
            }
        }

        public void SetCarried(TeamMember newCarrier)
        {
            carrier = newCarrier;
            State = BombState.Carried;
            CurrentSite = null;
            currentInteractionProgress = 0f;
            currentInteractingActor = null;
        }

        public void TryInteract(TeamMember actor, bool isHolding)
        {
            if (actor == null) return;

            // If someone else is already interacting, ignore this call
            if (currentInteractingActor != null && currentInteractingActor != actor)
            {
                return;
            }

            if (!isHolding)
            {
                if (currentInteractingActor == actor)
                {
                    currentInteractingActor = null;
                    currentInteractionProgress = 0f;
                }
                return;
            }

            bool canInteract = false;

            // Terrorist planting
            if (actor.Side == TeamSide.Terrorists && State == BombState.Carried && actor == carrier)
            {
                BombSite site = FindNearbySite(actor.transform.position);
                if (site != null)
                {
                    canInteract = true;
                }
            }
            // Counter-Terrorist defusing
            else if (actor.Side == TeamSide.CounterTerrorists && State == BombState.Planted)
            {
                float distance = Vector3.Distance(actor.transform.position, transform.position);
                if (distance <= 2.5f)
                {
                    canInteract = true;
                }
            }

            if (canInteract)
            {
                currentInteractingActor = actor;
                currentInteractionProgress += Time.deltaTime;

                float targetTime = (actor.Side == TeamSide.Terrorists) ? plantTime : defuseTime;
                if (currentInteractionProgress >= targetTime)
                {
                    if (actor.Side == TeamSide.Terrorists) Plant(FindNearbySite(actor.transform.position));
                    else Defuse();
                    
                    currentInteractingActor = null;
                    currentInteractionProgress = 0f;
                }
            }
            else
            {
                if (currentInteractingActor == actor)
                {
                    currentInteractingActor = null;
                    currentInteractionProgress = 0f;
                }
            }
        }

        private BombSite FindNearbySite(Vector3 position)
        {
            BombSite[] sites = FindObjectsByType<BombSite>(FindObjectsSortMode.None);
            foreach (BombSite site in sites)
            {
                if (Vector3.Distance(position, site.transform.position) <= site.PlantRadius)
                {
                    return site;
                }
            }
            return null;
        }

        public void Plant(BombSite site)
        {
            CurrentSite = site;
            State = BombState.Planted;
            explodeAt = Time.time + armedDuration;
            carrier = null;
            currentInteractionProgress = 0f;
            currentInteractingActor = null;

            if (site != null)
            {
                transform.position = site.transform.position;
                transform.rotation = site.transform.rotation;
            }

            roundManager?.OnBombPlanted(this);
        }

        public void Defuse()
        {
            if (State != BombState.Planted)
            {
                return;
            }

            State = BombState.Defused;
            currentInteractionProgress = 0f;
            currentInteractingActor = null;
            roundManager?.OnBombDefused(this);
        }

        public void ResetBomb()
        {
            State = BombState.Idle;
            CurrentSite = null;
            carrier = null;
            currentInteractionProgress = 0f;
            currentInteractingActor = null;
        }
    }
}
