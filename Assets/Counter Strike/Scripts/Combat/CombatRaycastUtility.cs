using System;
using FPSGame.Rounds;
using UnityEngine;

namespace FPSGame.Combat
{
    public static class CombatRaycastUtility
    {
        private static readonly Comparison<RaycastHit> DistanceComparison = (left, right) => left.distance.CompareTo(right.distance);

        public static RaycastHit[] GetSortedHits(Ray ray, float maxDistance, LayerMask layerMask)
        {
            RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance, layerMask, QueryTriggerInteraction.Ignore);
            Array.Sort(hits, DistanceComparison);
            return hits;
        }

        public static bool ShouldIgnoreHit(TeamMember owner, Collider collider)
        {
            if (collider == null)
            {
                return true;
            }

            TeamMember hitMember = collider.GetComponentInParent<TeamMember>();
            if (hitMember == null || owner == null)
            {
                return false;
            }

            return hitMember == owner || hitMember.Side == owner.Side;
        }

        public static bool HasLineOfSightToTarget(Vector3 origin, Vector3 targetPoint, TeamMember owner, TeamMember target, LayerMask layerMask)
        {
            Vector3 direction = targetPoint - origin;
            float distance = direction.magnitude;
            if (distance <= 0.01f)
            {
                return false;
            }

            Ray ray = new(origin, direction / distance);
            RaycastHit[] hits = GetSortedHits(ray, distance, layerMask);

            for (int index = 0; index < hits.Length; index++)
            {
                RaycastHit hit = hits[index];
                TeamMember hitMember = hit.collider.GetComponentInParent<TeamMember>();

                if (hitMember == owner || (hitMember != null && owner != null && hitMember.Side == owner.Side))
                {
                    continue;
                }

                return hitMember == target;
            }

            return true;
        }
    }
}
