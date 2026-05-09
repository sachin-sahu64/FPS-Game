using System.Collections.Generic;
using FPSGame.Weapons;
using UnityEngine;
using UnityEngine.AI;

namespace FPSGame.Rounds
{
    public class RoundActorLifecycle : MonoBehaviour
    {
        private const float NavMeshSampleDistance = 6f;

        [SerializeField] private TeamMember teamMember;
        [SerializeField] private CharacterController characterController;
        [SerializeField] private NavMeshAgent navMeshAgent;
        [SerializeField] private Collider[] collidersToToggle;
        [SerializeField] private Behaviour[] disableOnDeath;
        [SerializeField] private HitscanWeapon[] weaponsToReset;

        private void Awake()
        {
            EnsureReferences();

            if (teamMember == null)
            {
                teamMember = GetComponent<TeamMember>();
            }

            if (teamMember?.Health != null)
            {
                teamMember.Health.Died += HandleDeath;
            }
        }

        public void Respawn(Vector3 position, Quaternion rotation)
        {
            EnsureReferences();

            if (teamMember?.Health == null)
            {
                return;
            }

            if (characterController != null)
            {
                characterController.enabled = false;
            }

            transform.SetPositionAndRotation(position, rotation);

            if (navMeshAgent != null)
            {
                navMeshAgent.enabled = false;
            }

            teamMember.Health.ResetState();

            foreach (HitscanWeapon weapon in weaponsToReset)
            {
                if (weapon != null)
                {
                    weapon.ResetWeaponState();
                }
            }

            SetAliveState(true);
            TryEnableNavMeshAgent(position);

            if (characterController != null)
            {
                characterController.enabled = true;
            }
        }

        private void HandleDeath(Combat.Health health, Combat.DamageInfo damageInfo)
        {
            SetAliveState(false);
        }

        private void SetAliveState(bool isAlive)
        {
            EnsureReferences();

            foreach (Behaviour behaviour in disableOnDeath)
            {
                if (behaviour != null)
                {
                    behaviour.enabled = isAlive;
                }
            }

            foreach (Collider item in collidersToToggle)
            {
                if (item != null)
                {
                    item.enabled = isAlive;
                }
            }

            if (navMeshAgent != null)
            {
                if (!isAlive)
                {
                    navMeshAgent.enabled = false;
                }
            }
        }

        private void TryEnableNavMeshAgent(Vector3 preferredPosition)
        {
            if (navMeshAgent == null)
            {
                return;
            }

            if (!NavMesh.SamplePosition(preferredPosition, out NavMeshHit hit, NavMeshSampleDistance, NavMesh.AllAreas))
            {
                navMeshAgent.enabled = false;
                return;
            }

            navMeshAgent.enabled = true;

            if (!navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.enabled = false;
                return;
            }

            transform.position = hit.position;
            navMeshAgent.Warp(hit.position);
            navMeshAgent.nextPosition = hit.position;
        }

        private void EnsureReferences()
        {
            if (teamMember == null)
            {
                teamMember = GetComponent<TeamMember>();
            }

            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
            }

            if (navMeshAgent == null)
            {
                navMeshAgent = GetComponent<NavMeshAgent>();
            }

            if (collidersToToggle == null || collidersToToggle.Length == 0)
            {
                collidersToToggle = GetComponentsInChildren<Collider>(true);
            }

            if (weaponsToReset == null || weaponsToReset.Length == 0)
            {
                weaponsToReset = GetComponentsInChildren<HitscanWeapon>(true);
            }

            if (disableOnDeath == null || disableOnDeath.Length == 0)
            {
                List<Behaviour> behaviours = new();
                foreach (Behaviour behaviour in GetComponents<Behaviour>())
                {
                    if (behaviour == null || behaviour == this)
                    {
                        continue;
                    }

                    if (behaviour is FPSGame.Combat.Health || behaviour is TeamMember || behaviour is NavMeshAgent)
                    {
                        continue;
                    }

                    behaviours.Add(behaviour);
                }

                disableOnDeath = behaviours.ToArray();
            }
        }

        private void Reset()
        {
            teamMember = GetComponent<TeamMember>();
            characterController = GetComponent<CharacterController>();
            navMeshAgent = GetComponent<NavMeshAgent>();
            collidersToToggle = GetComponentsInChildren<Collider>(true);
            weaponsToReset = GetComponentsInChildren<HitscanWeapon>(true);

            List<Behaviour> behaviours = new();
            foreach (Behaviour behaviour in GetComponents<Behaviour>())
            {
                if (behaviour == null || behaviour == this)
                {
                    continue;
                }

                if (behaviour is FPSGame.Combat.Health || behaviour is TeamMember || behaviour is NavMeshAgent)
                {
                    continue;
                }

                behaviours.Add(behaviour);
            }

            disableOnDeath = behaviours.ToArray();
        }
    }
}
